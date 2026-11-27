using Hospital_Project.Enums;
using Hospital_Project.Exceptions;
using Hospital_Project.Helpers;
using Hospital_Project.Models;
using Hospital_Project.Services;
using Spectre.Console;
using static Hospital_Project.Helpers.Translator;

namespace Hospital_Project.Menus
{
    internal static class AdminMenu
    {
        public static async Task RunAsync(Admin admin, List<Doctor> doctors)
        {
            AnsiConsole.Write(
                new Panel($"[bold yellow]{T("Administrator Access")}[/]")
                    .BorderColor(Color.Yellow)
                    .RoundedBorder());

            bool running = true;

            while (running)
            {
                string choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[yellow]{T("Admin Menu")}[/]")
                        .PageSize(14)
                        .WrapAround()
                        .AddChoices(
                            T("View All Patients"),
                            T("Search Patients"),
                            T("Patient Summary"),
                            T("Blood Compatibility"),
                            T("Add Balance to Patient"),
                            T("View All Doctors"),
                            T("Add Balance to Doctor"),
                            T("Doctor Payout"),
                            T("View All Appointments"),
                            T("Hospital Statistics"),
                            T("Backup Data"),
                            T("Change Password"),
                            T("Logout")));

                await ConsoleUI.RunSafeAsync(async () =>
                {
                    List<Patient> patients = await DataService.LoadPatientsAsync();

                    switch (choice)
                    {
                        case var c when c == T("View All Patients"):
                            Tables.ShowPatients(patients);
                            break;

                        case var c when c == T("Search Patients"):
                            SearchPatients(patients);
                            break;

                        case var c when c == T("Patient Summary"):
                            await ShowPatientSummaryAsync(patients);
                            break;

                        case var c when c == T("Blood Compatibility"):
                            ShowBloodCompatibility(patients);
                            break;

                        case var c when c == T("Add Balance to Patient"):
                            await AddPatientBalanceAsync(patients);
                            break;

                        case var c when c == T("View All Doctors"):
                            Tables.ShowDoctorsDetailed(doctors);
                            break;

                        case var c when c == T("Add Balance to Doctor"):
                            await ChangeDoctorBalanceAsync(doctors, true);
                            break;

                        case var c when c == T("Doctor Payout"):
                            await ChangeDoctorBalanceAsync(doctors, false);
                            break;

                        case var c when c == T("View All Appointments"):
                            ShowAllAppointments(patients);
                            break;

                        case var c when c == T("Hospital Statistics"):
                            await ShowStatisticsAsync(patients, doctors);
                            break;

                        case var c when c == T("Backup Data"):
                            string folder = await DataService.BackupAsync();
                            ConsoleUI.ShowSuccess($"{T("Backup saved to:")} {folder}");
                            break;

                        case var c when c == T("Change Password"):
                            await AuthService.ChangePasswordAsync(
                                admin,
                                () => DataService.SaveStaffPasswordsAsync(admin, doctors));
                            break;

                        case var c when c == T("Logout"):
                            running = false;
                            ConsoleUI.ShowSuccess(T("Administrator logged out."));
                            break;
                    }
                });
            }
        }

        private static void SearchPatients(List<Patient> patients)
        {
            if (patients.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{T("No registered patients found.")}[/]");
                return;
            }

            string mode = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[cyan]{T("Search by:")}[/]")
                    .AddChoices(T("By ID"), T("By Name"), T("By Blood Type")));

            Predicate<Patient> match;

            if (mode == T("By ID"))
                match = Filters.PatientById(ConsoleUI.AskPositiveInt(T("Patient ID:")));
            else if (mode == T("By Name"))
                match = Filters.PatientByName(ConsoleUI.AskText(T("Name contains:")));
            else
                match = Filters.PatientByBloodType(ConsoleUI.AskBloodType($"[cyan]{T("Select blood type:")}[/]"));

            List<Patient> results = Filters.Find(patients, match)
                .OrderBy(p => p.FullName)
                .ToList();

            if (results.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{T("No patients matched your search.")}[/]");
                return;
            }

            Tables.ShowPatients(results);
        }

        private static async Task ShowPatientSummaryAsync(List<Patient> patients)
        {
            if (patients.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{T("No registered patients found.")}[/]");
                return;
            }

            Patient patient = AnsiConsole.Prompt(
                new SelectionPrompt<Patient>()
                    .Title($"[cyan]{T("Select a patient:")}[/]")
                    .PageSize(10)
                    .UseConverter(p => Markup.Escape($"{p.Id} | {p.FullName}"))
                    .AddChoices(patients));

            List<Appointment> history = await DataService.LoadHistoryAsync();
            Tables.ShowPatientSummary(patient, history);
        }

        private static void ShowBloodCompatibility(List<Patient> patients)
        {
            Blood_Type_ENUMS donor = ConsoleUI.AskBloodType($"[cyan]{T("Select blood type:")}[/]");

            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .Title($"[bold red]{T("Blood Compatibility")}[/]");

            table.AddColumn(T("Field"));
            table.AddColumn(T("Information"));

            table.AddRow(T("Blood Type"), BloodTypeHelper.ToText(donor));
            table.AddRow(T("Can donate to"), BloodTypeHelper.JoinText(BloodTypeHelper.GetRecipients(donor)));
            table.AddRow(T("Can receive from"), BloodTypeHelper.JoinText(BloodTypeHelper.GetDonors(donor)));

            List<Patient> canReceive = patients.FindAll(p => BloodTypeHelper.CanDonate(donor, p.BloodType));
            string names = canReceive.Count == 0
                ? "-"
                : string.Join(", ", canReceive.Select(p => p.FullName));

            table.AddRow(T("Registered patients who can receive"), Markup.Escape(names));

            AnsiConsole.Write(table);
        }

        private static async Task AddPatientBalanceAsync(List<Patient> patients)
        {
            if (patients.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{T("No registered patients found.")}[/]");
                return;
            }

            string identifier = AnsiConsole.Ask<string>(
                "[cyan]Enter Patient Email or ID:[/]").Trim();

            Patient? target = int.TryParse(identifier, out int patientId)
                ? patients.FirstOrDefault(p => p.Id == patientId)
                : patients.FirstOrDefault(p =>
                    p.Email.Equals(identifier, StringComparison.OrdinalIgnoreCase));

            if (target is null)
                throw new Exception_Class(T("Patient not found."));

            decimal amount = ConsoleUI.AskAmount(
                $"[cyan]Amount to add to {Markup.Escape(target.FullName)} ($):[/]");

            if (!AnsiConsole.Confirm(
                $"Add [green]${amount:F2}[/] to {Markup.Escape(target.FullName)}?"))
                return;

            decimal oldBalance = target.Balance;
            target.Balance += amount;

            try
            {
                await DataService.SavePatientsAsync(patients);
            }
            catch
            {
                target.Balance = oldBalance;
                throw;
            }

            Logger.Log($"Admin added ${amount:F2} to patient {target.Id}");
            ConsoleUI.ShowSuccess($"${amount:F2} added successfully. New balance: ${target.Balance:F2}");
        }

        private static async Task ChangeDoctorBalanceAsync(List<Doctor> doctors, bool add)
        {
            if (doctors.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{T("No doctors found.")}[/]");
                return;
            }

            Doctor doctor = AnsiConsole.Prompt(
                new SelectionPrompt<Doctor>()
                    .Title($"[cyan]{T("Select a doctor:")}[/]")
                    .PageSize(8)
                    .WrapAround()
                    .UseConverter(d => Markup.Escape($"{d.FullName} | {d.Specialize} | Balance: ${d.Balance:F2}"))
                    .AddChoices(doctors));

            decimal amount = ConsoleUI.AskAmount("[cyan]Amount ($):[/]");

            if (!add && amount > doctor.Balance)
                throw Exception_Class.Invalid_Amount(T("Insufficient balance."));

            string verb = add ? "Add" : "Pay out";

            if (!AnsiConsole.Confirm(
                $"{verb} [green]${amount:F2}[/] - {Markup.Escape(doctor.FullName)}?"))
                return;

            decimal oldBalance = doctor.Balance;
            doctor.Balance += add ? amount : -amount;

            try
            {
                await DataService.SaveDoctorBalancesAsync(doctors);
            }
            catch
            {
                doctor.Balance = oldBalance;
                throw;
            }

            Logger.Log($"Doctor {doctor.Id} balance {(add ? "+" : "-")}${amount:F2}");
            ConsoleUI.ShowSuccess($"{doctor.FullName}: ${doctor.Balance:F2}");
        }

        private static void ShowAllAppointments(List<Patient> patients)
        {
            List<string> choices = [T("All"), .. Enum.GetNames<AppointmentStatus>()];

            string filter = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[cyan]{T("Filter by status:")}[/]")
                    .AddChoices(choices));

            Predicate<AppointmentInfo> match = a => true;

            if (filter != T("All"))
            {
                AppointmentStatus wanted = Enum.Parse<AppointmentStatus>(filter);
                match = a => a.Status == wanted;
            }

            List<AppointmentRow> rows = AppointmentService.GetRows(patients, match);
            Tables.ShowAppointments(T("All Appointments"), rows, true, true);
        }

        private static async Task ShowStatisticsAsync(List<Patient> patients, List<Doctor> doctors)
        {
            List<Appointment> history = await DataService.LoadHistoryAsync();

            decimal patientBalances = Filters.SumBy(patients, p => p.Balance);
            decimal outstanding = Filters.SumBy(patients, p => p.PendingBill);
            decimal doctorBalances = Filters.SumBy(doctors, d => d.Balance);
            decimal revenue = Filters.SumBy(history.Where(h => h.IsPaid), h => h.Payment);

            Doctor? topDoctor = doctors.OrderByDescending(d => d.Balance).FirstOrDefault();

            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Yellow)
                .Title($"[bold yellow]{T("Hospital Statistics")}[/]");

            table.AddColumn(T("Field"));
            table.AddColumn(T("Information"));

            table.AddRow(T("Registered Patients"), patients.Count.ToString());
            table.AddRow(T("Doctors"), doctors.Count.ToString());
            table.AddRow(T("Total patient balances"), $"[green]${patientBalances:F2}[/]");
            table.AddRow(T("Total unpaid bills"), $"[red]${outstanding:F2}[/]");
            table.AddRow(T("Total doctor balances"), $"[green]${doctorBalances:F2}[/]");
            table.AddRow(T("Total revenue (paid)"), $"[green]${revenue:F2}[/]");
            table.AddRow(T("Highest doctor balance"),
                topDoctor is null ? "-" : Markup.Escape($"{topDoctor.FullName} (${topDoctor.Balance:F2})"));

            foreach (AppointmentStatus status in Enum.GetValues<AppointmentStatus>())
            {
                int count = patients.Sum(p => p.Appointments.Count(a => a.Status == status));
                table.AddRow($"{T("Active appointments")}: {status}", count.ToString());
            }

            table.AddRow(T("Appointments in history"), history.Count.ToString());

            AnsiConsole.Write(table);
        }
    }
}
