using Hospital_Project.Enums;
using Hospital_Project.Exceptions;
using Hospital_Project.Models;
using Spectre.Console;
using System.Text.Json;

namespace Hospital_Project
{
    internal class Program
    {
        private const string PatientDataFilePath = @"C:\Users\User\source\repos\Hospital_Project\Text_Files\Patient_Data.txt";
        private const string DoctorBalanceFilePath = @"C:\Users\User\source\repos\Hospital_Project\Text_Files\Doctor_Balances.txt";
        private static readonly JsonSerializerOptions CachedJsonOptions = new() { WriteIndented = true };

        private const string AdminEmail = "admin@hospital.com";
        private const string AdminPassword = "admin123";

        static async Task Main()
        {
            ShowWelcome();

            List<Doctor> doctors =
            [
                new StaffDoctor(1, "Dr. Giorgi Beridze", "g.beridze@hospital.com", "555-0101", "GDoc123", Doctor_ENUMS.GENERAL_DOCTOR, 5),
                new StaffDoctor(2, "Dr. Nino Kapanadze", "n.kapanadze@hospital.com", "555-0102", "NDoc123", Doctor_ENUMS.SURGEON, 12),
                new StaffDoctor(3, "Dr. Luka Lomidze", "l.lomidze@hospital.com", "555-0103", "LDoc123", Doctor_ENUMS.CARDIOLOGIST, 9),
                new StaffDoctor(4, "Dr. Mariam Tsiklauri", "m.tsiklauri@hospital.com", "555-0104", "MDoc123", Doctor_ENUMS.PEDIATRICIAN, 7),
                new StaffDoctor(5, "Dr. Davit Maisuradze", "d.maisuradze@hospital.com", "555-0105", "DDoc123", Doctor_ENUMS.DERMATOLOGIST, 6),
                new StaffDoctor(6, "Dr. Tamar Gelashvili", "t.gelashvili@hospital.com", "555-0106", "TDoc123", Doctor_ENUMS.NEUROLOGIST, 10),
                new StaffDoctor(7, "Dr. Sandro Chkhaidze", "s.chkhaidze@hospital.com", "555-0107", "SDoc123", Doctor_ENUMS.DENTIST, 4)
            ];

            await LoadDoctorBalancesAsync(doctors);
            await Config(doctors);
        }

        private static void ShowWelcome()
        {
            AnsiConsole.Write(
                new FigletText("HOSPITAL")
                    .Centered()
                    .Color(Color.Cyan1));

            AnsiConsole.Write(
                new Panel(
                    Align.Center(
                        new Markup("[bold white]Hospital Management System[/]\n[grey]Patient • Doctor • Administration[/]"),
                        VerticalAlignment.Middle))
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Cyan1)
                    .Padding(2, 1));
        }

        public static async Task Config(List<Doctor> doctors)
        {
            bool running = true;

            while (running)
            {
                AnsiConsole.Write(new Rule("[cyan]Main Menu[/]").RuleStyle(Color.Grey));

                string choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[cyan]Select a service:[/]")
                        .HighlightStyle(new Style(Color.Cyan1, decoration: Decoration.Bold))
                        .WrapAround()
                        .AddChoices(
                            "Patient Registration",
                            "Login",
                            "View Doctors",
                            "Exit"));

                try
                {
                    switch (choice)
                    {
                        case "Patient Registration":
                            await RegisterPatientAsync(doctors);
                            break;

                        case "Login":
                            await LoginAsync(doctors);
                            break;

                        case "View Doctors":
                            ShowDoctors(doctors);
                            break;

                        case "Exit":
                            running = false;
                            AnsiConsole.MarkupLine("\n[grey]Thank you for using Hospital Management System.[/]");
                            break;
                    }
                }
                catch (Exception_Class ex)
                {
                    ShowError(ex.Message);
                }
            }
        }

        private static async Task RegisterPatientAsync(List<Doctor> doctors)
        {
            AnsiConsole.Write(new Rule("[green]Patient Registration[/]"));

            string fullName = AnsiConsole.Ask<string>("[cyan]Full Name:[/]").Trim();
            string email = AnsiConsole.Ask<string>("[cyan]Email:[/]").Trim();
            string phone = AnsiConsole.Ask<string>("[cyan]Phone Number:[/]").Trim();
            string password = AnsiConsole.Ask<string>("[cyan]Password:[/]");
            string pin = AnsiConsole.Ask<string>("[cyan]Create a 4-digit Payment PIN:[/]").Trim();

            if (string.IsNullOrWhiteSpace(fullName))
                throw new Exception_Class("Full name cannot be empty.");

            if (!email.Contains('@') || !email.Contains('.'))
                throw new Exception_Class("Please enter a valid email address.");

            if (email.Equals(AdminEmail, StringComparison.OrdinalIgnoreCase) ||
                doctors.Any(d => d.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception_Class("This email is reserved.");
            }

            if (string.IsNullOrWhiteSpace(password))
                throw new Exception_Class("Password cannot be empty.");

            if (pin.Length != 4 || !pin.All(char.IsDigit))
                throw new Exception_Class("PIN must be exactly 4 digits.");

            List<Patient> users = await LoadUsersAsync();

            if (users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                throw new Exception_Class("A user with this email already exists.");

            int newId = users.Count == 0 ? 1 : users.Max(u => u.Id) + 1;

            Patient user = new()
            {
                Id = newId,
                FullName = fullName,
                Email = email,
                PN = phone,
                Password = password,
                Pin = pin
            };

            users.Add(user);
            await SaveUsersAsync(users);

            AnsiConsole.Write(
                new Panel("[bold green]Registration completed successfully![/]")
                    .BorderColor(Color.Green)
                    .RoundedBorder());

            AnsiConsole.MarkupLine($"[grey]Your Patient ID: [white]{newId}[/][/]");
        }

        private static async Task LoginAsync(List<Doctor> doctors)
        {
            AnsiConsole.Write(new Rule("[yellow]Login[/]"));

            string email = AnsiConsole.Ask<string>("[cyan]Email:[/]").Trim();
            string password = AnsiConsole.Ask<string>("[cyan]Password:[/]");

            if (email.Equals(AdminEmail, StringComparison.OrdinalIgnoreCase) && password == AdminPassword)
            {
                await HandleAdminMenuAsync(doctors);
                return;
            }

            Doctor? loggedInDoctor = doctors.FirstOrDefault(d =>
                d.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                d.Password == password);

            if (loggedInDoctor is not null)
            {
                await HandleDoctorMenuAsync(loggedInDoctor);
                return;
            }

            List<Patient> users = await LoadUsersAsync();

            Patient? loggedInUser = users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);

            if (loggedInUser is null)
                throw new Exception_Class("Invalid email or password.");

            await HandlePatientMenuAsync(loggedInUser, users, doctors);
        }

        private static void ShowDoctors(List<Doctor> doctors)
        {
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Cyan1)
                .Title("[bold cyan]Hospital Doctors[/]")
                .Expand();

            table.AddColumn("[bold]ID[/]");
            table.AddColumn("[bold]Doctor[/]");
            table.AddColumn("[bold]Specialty[/]");
            table.AddColumn("[bold]Experience[/]");
            table.AddColumn("[bold]Fee / Hour[/]");

            foreach (Doctor doctor in doctors)
            {
                table.AddRow(
                    doctor.Id.ToString(),
                    Markup.Escape(doctor.FullName),
                    Markup.Escape(doctor.Specialize.ToString()),
                    $"{doctor.Expertise} years",
                    $"[green]${doctor.ConsultationFee:F2}[/]");
            }

            AnsiConsole.Write(table);
        }

        private static async Task HandleAdminMenuAsync(List<Doctor> doctors)
        {
            AnsiConsole.Write(
                new Panel("[bold yellow]Administrator Access[/]")
                    .BorderColor(Color.Yellow)
                    .RoundedBorder());

            bool running = true;

            while (running)
            {
                string choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Admin Menu[/]")
                        .PageSize(8)
                        .WrapAround()
                        .AddChoices(
                            "View All Patients",
                            "Add Balance to Patient",
                            "View All Doctors",
                            "Add Balance to Doctor",
                            "Logout"));

                try
                {
                    List<Patient> patients = await LoadUsersAsync();

                    switch (choice)
                    {
                        case "View All Patients":
                            ShowPatients(patients);
                            break;

                        case "Add Balance to Patient":
                            await AddPatientBalanceAsync(patients);
                            break;

                        case "View All Doctors":
                            ShowDoctorsDetailed(doctors);
                            break;

                        case "Add Balance to Doctor":
                            await AddDoctorBalanceAsync(doctors);
                            break;

                        case "Logout":
                            running = false;
                            ShowSuccess("Administrator logged out.");
                            break;
                    }
                }
                catch (Exception_Class ex)
                {
                    ShowError(ex.Message);
                }
            }
        }

        private static void ShowPatients(List<Patient> patients)
        {
            if (patients.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No registered patients found.[/]");
                return;
            }

            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .Title("[bold blue]Registered Patients[/]")
                .Expand();

            table.AddColumn("ID");
            table.AddColumn("Name");
            table.AddColumn("Email");
            table.AddColumn("Disease");
            table.AddColumn("Phone");
            table.AddColumn("Insurance");
            table.AddColumn("Blood Type");
            table.AddColumn("Balance");
            table.AddColumn("Pending Bill");

            foreach (Patient patient in patients)
            {
                table.AddRow(
                    patient.Id.ToString(),
                    Markup.Escape(patient.FullName),
                    Markup.Escape(patient.Email),
                    Markup.Escape(string.IsNullOrWhiteSpace(patient.Disease) ? "None" : patient.Disease),
                    Markup.Escape(patient.PN),
                    Markup.Escape(patient.Insurance?.ToString() ?? "N/A"),
                    Markup.Escape(patient.BloodType.ToString()),
                    $"[green]${patient.Balance:F2}[/]",
                    patient.PendingBill > 0
                        ? $"[red]${patient.PendingBill:F2}[/]"
                        : "[green]$0.00[/]");
            }

            AnsiConsole.Write(table);
        }

        private static async Task AddPatientBalanceAsync(List<Patient> patients)
        {
            if (patients.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No registered patients found.[/]");
                return;
            }

            string identifier = AnsiConsole.Ask<string>("[cyan]Enter Patient Email or ID:[/]").Trim();

            Patient? targetPatient = int.TryParse(identifier, out int patientId)
                ? patients.FirstOrDefault(p => p.Id == patientId)
                : patients.FirstOrDefault(p => p.Email.Equals(identifier, StringComparison.OrdinalIgnoreCase));

            if (targetPatient is null)
                throw new Exception_Class("Patient not found.");

            decimal amount = AnsiConsole.Ask<decimal>(
                $"[cyan]Amount to add to {Markup.Escape(targetPatient.FullName)} ($):[/]");

            if (amount <= 0)
                throw new Exception_Class("Amount must be greater than zero.");

            if (!AnsiConsole.Confirm($"Add [green]${amount:F2}[/] to {Markup.Escape(targetPatient.FullName)}?"))
                return;

            targetPatient.Balance += amount;
            await SavePatientsAsync(patients);

            ShowSuccess($"${amount:F2} added successfully. New balance: ${targetPatient.Balance:F2}");
        }

        private static void ShowDoctorsDetailed(List<Doctor> doctors)
        {
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Cyan1)
                .Title("[bold cyan]Doctor Directory[/]")
                .Expand();

            table.AddColumn("ID");
            table.AddColumn("Name");
            table.AddColumn("Email");
            table.AddColumn("Specialty");
            table.AddColumn("Phone");
            table.AddColumn("Experience");
            table.AddColumn("Fee");
            table.AddColumn("Balance");

            foreach (Doctor doctor in doctors)
            {
                table.AddRow(
                    doctor.Id.ToString(),
                    Markup.Escape(doctor.FullName),
                    Markup.Escape(doctor.Email),
                    Markup.Escape(doctor.Specialize.ToString()),
                    Markup.Escape(doctor.PN),
                    $"{doctor.Expertise} years",
                    $"[green]${doctor.ConsultationFee:F2}/hr[/]",
                    $"[green]${doctor.Balance:F2}[/]");
            }

            AnsiConsole.Write(table);
        }

        private static async Task HandleDoctorMenuAsync(Doctor doctor)
        {
            AnsiConsole.Write(
                new Panel(
                    $"[bold cyan]Welcome, {Markup.Escape(doctor.FullName)}[/]\n" +
                    $"[grey]{Markup.Escape(doctor.Specialize.ToString())}[/]")
                    .BorderColor(Color.Cyan1)
                    .RoundedBorder());

            bool running = true;

            while (running)
            {
                string choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[cyan]Doctor Menu[/]")
                        .WrapAround()
                        .AddChoices(
                            "View Doctor Profile",
                            "View Appointments",
                            "Logout"));

                switch (choice)
                {
                    case "View Doctor Profile":
                        ShowDoctorProfile(doctor);
                        break;

                    case "View Appointments":
                        await ShowDoctorAppointmentsAsync(doctor);
                        break;

                    case "Logout":
                        running = false;
                        ShowSuccess("Doctor logged out.");
                        break;
                }
            }
        }

        private static void ShowDoctorProfile(Doctor doctor)
        {
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Cyan1)
                .Title("[bold cyan]Doctor Profile[/]");

            table.AddColumn("Field");
            table.AddColumn("Information");

            table.AddRow("ID", doctor.Id.ToString());
            table.AddRow("Name", Markup.Escape(doctor.FullName));
            table.AddRow("Email", Markup.Escape(doctor.Email));
            table.AddRow("Phone", Markup.Escape(doctor.PN));
            table.AddRow("Specialty", Markup.Escape(doctor.Specialize.ToString()));
            table.AddRow("Experience", $"{doctor.Expertise} years");
            table.AddRow("Consultation Fee", $"[green]${doctor.ConsultationFee:F2}/hr[/]");
            table.AddRow("Account Balance", $"[green]${doctor.Balance:F2}[/]");

            AnsiConsole.Write(table);
        }

        private static async Task ShowDoctorAppointmentsAsync(Doctor doctor)
        {
            List<Patient> patients = await LoadUsersAsync();

            var appointments = patients
                .SelectMany(p => (p.Appointments ?? [])
                    .Where(a => a.DoctorId == doctor.Id)
                    .Select(a => new { Patient = p, Appointment = a }))
                .ToList();

            if (appointments.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No appointments found.[/]");
                return;
            }

            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .Title("[bold blue]Doctor Appointments[/]")
                .Expand();

            table.AddColumn("Patient");
            table.AddColumn("Disease");
            table.AddColumn("Duration");
            table.AddColumn("Cost");
            table.AddColumn("Payment");

            foreach (var item in appointments)
            {
                table.AddRow(
                    Markup.Escape(item.Patient.FullName),
                    Markup.Escape(item.Appointment.Disease),
                    $"{item.Appointment.Hours} hrs",
                    $"[green]${item.Appointment.Cost:F2}[/]",
                    item.Appointment.IsPaid ? "[green]Paid[/]" : "[red]Unpaid[/]");
            }

            AnsiConsole.Write(table);
        }

        private static async Task HandlePatientMenuAsync(
            Patient loggedInUser,
            List<Patient> registeredUsers,
            List<Doctor> doctors)
        {
            AnsiConsole.Write(
                new Panel(
                    $"[bold green]Welcome back, {Markup.Escape(loggedInUser.FullName)}![/]\n" +
                    "[grey]Patient Portal[/]")
                    .BorderColor(Color.Green)
                    .RoundedBorder());

            bool running = true;

            while (running)
            {
                string choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[green]Patient Menu[/]")
                        .WrapAround()
                        .AddChoices(
                            "View Profile",
                            "Update Profile",
                            "Make an Appointment",
                            "View Balance",
                            "Pay Bills",
                            "View My Appointments",
                            "Logout"));

                try
                {
                    switch (choice)
                    {
                        case "View Profile":
                            ShowPatientProfile(loggedInUser);
                            break;

                        case "Update Profile":
                            await UpdatePatientProfileAsync(loggedInUser, registeredUsers);
                            break;

                        case "Make an Appointment":
                            await MakeAppointmentAsync(loggedInUser, registeredUsers, doctors);
                            break;

                        case "View Balance":
                            ShowPatientBalance(loggedInUser);
                            break;

                        case "Pay Bills":
                            await PayPatientBillsAsync(loggedInUser, registeredUsers, doctors);
                            break;

                        case "View My Appointments":
                            ShowPatientAppointments(loggedInUser);
                            break;

                        case "Logout":
                            running = false;
                            ShowSuccess("Patient logged out.");
                            break;
                    }
                }
                catch (Exception_Class ex)
                {
                    ShowError(ex.Message);
                }
            }
        }

        private static void ShowPatientProfile(Patient patient)
        {
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green)
                .Title("[bold green]Patient Profile[/]");

            table.AddColumn("Field");
            table.AddColumn("Information");

            table.AddRow("Patient ID", patient.Id.ToString());
            table.AddRow("Full Name", Markup.Escape(patient.FullName));
            table.AddRow("Email", Markup.Escape(patient.Email));
            table.AddRow("Phone", Markup.Escape(patient.PN));
            table.AddRow("Disease", Markup.Escape(string.IsNullOrWhiteSpace(patient.Disease) ? "None recorded" : patient.Disease));
            table.AddRow("Balance", $"[green]${patient.Balance:F2}[/]");
            table.AddRow("Pending Bill", patient.PendingBill > 0 ? $"[red]${patient.PendingBill:F2}[/]" : "[green]$0.00[/]");

            AnsiConsole.Write(table);
        }

        private static async Task UpdatePatientProfileAsync(
            Patient patient,
            List<Patient> registeredUsers)
        {
            AnsiConsole.Write(new Rule("[yellow]Update Profile[/]"));

            string newName = AnsiConsole.Ask<string>("[cyan]New Full Name:[/]").Trim();
            string newEmail = AnsiConsole.Ask<string>("[cyan]New Email:[/]").Trim();
            string newPhone = AnsiConsole.Ask<string>("[cyan]New Phone Number:[/]").Trim();

            if (string.IsNullOrWhiteSpace(newName))
                throw new Exception_Class("Name cannot be empty.");

            if (!newEmail.Contains('@') || !newEmail.Contains('.'))
                throw new Exception_Class("Please enter a valid email.");

            bool emailUsed = registeredUsers.Any(p =>
                p.Id != patient.Id &&
                p.Email.Equals(newEmail, StringComparison.OrdinalIgnoreCase));

            if (emailUsed)
                throw new Exception_Class("This email is already in use.");

            if (!AnsiConsole.Confirm("[yellow]Save profile changes?[/]"))
                return;

            patient.FullName = newName;
            patient.Email = newEmail;
            patient.PN = newPhone;

            await SaveUsersAsync(registeredUsers);
            ShowSuccess("Profile updated successfully.");
        }

        private static async Task MakeAppointmentAsync(
            Patient patient,
            List<Patient> registeredUsers,
            List<Doctor> doctors)
        {
            ShowDoctors(doctors);

            Doctor chosenDoctor = AnsiConsole.Prompt(
                new SelectionPrompt<Doctor>()
                    .Title("[cyan]Select a doctor:[/]")
                    .PageSize(8)
                    .UseConverter(d => $"{d.FullName} | {d.Specialize} | ${d.ConsultationFee:F2}/hr")
                    .AddChoices(doctors));

            string disease = AnsiConsole.Ask<string>("[cyan]Enter your disease or symptoms:[/]").Trim();

            if (string.IsNullOrWhiteSpace(disease))
                throw new Exception_Class("Disease or symptoms cannot be empty.");

            decimal duration = AnsiConsole.Ask<decimal>("[cyan]Consultation duration in hours:[/]");

            if (duration <= 0)
                throw new Exception_Class("Duration must be greater than zero.");

            patient.Disease = disease;
            patient.Consultation_Time = duration;

            decimal price = chosenDoctor.CalculatePrice(patient);

            AnsiConsole.Write(
                new Panel(
                    $"[bold]Doctor:[/] {Markup.Escape(chosenDoctor.FullName)}\n" +
                    $"[bold]Specialty:[/] {Markup.Escape(chosenDoctor.Specialize.ToString())}\n" +
                    $"[bold]Duration:[/] {duration} hours\n" +
                    $"[bold]Estimated Cost:[/] [green]${price:F2}[/]")
                    .BorderColor(Color.Yellow)
                    .RoundedBorder());

            if (!AnsiConsole.Confirm("[yellow]Confirm appointment?[/]"))
                return;

            patient.PendingBill += price;

            patient.Appointments.Add(new AppointmentInfo
            {
                DoctorId = chosenDoctor.Id,
                DoctorName = chosenDoctor.FullName,
                Disease = disease,
                Hours = duration,
                Cost = price,
                IsPaid = false
            });

            await SaveUsersAsync(registeredUsers);

            ShowSuccess("Appointment confirmed.");
            AnsiConsole.MarkupLine($"[grey]Pending bill: [red]${patient.PendingBill:F2}[/][/]");
        }

        private static void ShowPatientBalance(Patient patient)
        {
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Yellow)
                .Title("[bold yellow]Account Balance[/]");

            table.AddColumn("Account");
            table.AddColumn("Amount");

            table.AddRow("Available Balance", $"[green]${patient.Balance:F2}[/]");
            table.AddRow("Pending Bill", patient.PendingBill > 0 ? $"[red]${patient.PendingBill:F2}[/]" : "[green]$0.00[/]");

            AnsiConsole.Write(table);
        }

        private static async Task PayPatientBillsAsync(
            Patient patient,
            List<Patient> registeredUsers,
            List<Doctor> doctors)
        {
            if (patient.PendingBill <= 0)
            {
                AnsiConsole.MarkupLine("[green]You have no unpaid bills.[/]");
                return;
            }

            AnsiConsole.Write(
                new Panel(
                    $"[bold]Amount Due:[/] [red]${patient.PendingBill:F2}[/]\n" +
                    $"[bold]Available Balance:[/] [green]${patient.Balance:F2}[/]")
                    .BorderColor(Color.Yellow)
                    .RoundedBorder());

            if (patient.Balance < patient.PendingBill)
            {
                throw new Exception_Class(
                    $"Insufficient balance. You need an additional ${(patient.PendingBill - patient.Balance):F2}.");
            }

            if (!AnsiConsole.Confirm("[yellow]Continue with payment?[/]"))
                return;

            string pin = AnsiConsole.Ask<string>("[cyan]Enter your 4-digit payment PIN:[/]").Trim();

            if (pin != patient.Pin)
                throw new Exception_Class("Incorrect PIN. Payment cancelled.");

            decimal amountToPay = patient.PendingBill;

            patient.Balance -= amountToPay;
            patient.PendingBill = 0;
            patient.Consultation_Time = 0;

            foreach (AppointmentInfo appointment in patient.Appointments.Where(a => !a.IsPaid))
            {
                Doctor? doctor = doctors.FirstOrDefault(d => d.Id == appointment.DoctorId);

                if (doctor is not null)
                    doctor.Balance += appointment.Cost;

                appointment.IsPaid = true;
            }

            await SavePatientsAsync(registeredUsers);
            await SaveDoctorBalancesAsync(doctors);

            AnsiConsole.Write(
                new Panel(
                    $"[bold green]Payment Successful![/]\n" +
                    $"[bold]Paid:[/] ${amountToPay:F2}\n" +
                    $"[bold]Remaining Balance:[/] ${patient.Balance:F2}")
                    .BorderColor(Color.Green)
                    .RoundedBorder());
        }

        private static void ShowPatientAppointments(Patient patient)
        {
            if (patient.Appointments is not { Count: > 0 })
            {
                AnsiConsole.MarkupLine("[yellow]You have no appointments.[/]");
                return;
            }

            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .Title("[bold blue]My Appointments[/]")
                .Expand();

            table.AddColumn("Doctor");
            table.AddColumn("Disease");
            table.AddColumn("Hours");
            table.AddColumn("Cost");
            table.AddColumn("Status");

            foreach (AppointmentInfo appointment in patient.Appointments)
            {
                table.AddRow(
                    Markup.Escape(appointment.DoctorName),
                    Markup.Escape(appointment.Disease),
                    $"{appointment.Hours}",
                    $"${appointment.Cost:F2}",
                    appointment.IsPaid ? "[green]Paid[/]" : "[red]Unpaid[/]");
            }

            AnsiConsole.Write(table);
        }

        private static async Task<List<Patient>> LoadUsersAsync()
        {
            if (!File.Exists(PatientDataFilePath))
                return [];

            string content = await File.ReadAllTextAsync(PatientDataFilePath);

            if (string.IsNullOrWhiteSpace(content))
                return [];

            return JsonSerializer.Deserialize<List<Patient>>(content) ?? [];
        }

        private static async Task SavePatientsAsync(List<Patient> patients)
        {
            await SaveUsersAsync(patients);
        }

        private static async Task SaveUsersAsync(List<Patient> users)
        {
            string? directory = Path.GetDirectoryName(PatientDataFilePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            string json = JsonSerializer.Serialize(users, CachedJsonOptions);
            await File.WriteAllTextAsync(PatientDataFilePath, json);
        }

        private static void ShowSuccess(string message)
        {
            AnsiConsole.MarkupLine($"[green]✔ {Markup.Escape(message)}[/]");
        }

        private static void ShowError(string message)
        {
            AnsiConsole.Write(
                new Panel($"[bold red]✖ {Markup.Escape(message)}[/]")
                    .BorderColor(Color.Red)
                    .RoundedBorder());
        }

        private static async Task AddDoctorBalanceAsync(List<Doctor> doctors)
        {
            if (doctors.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No doctors found.[/]");
                return;
            }

            Doctor doctor = AnsiConsole.Prompt(
                new SelectionPrompt<Doctor>()
                    .Title("[cyan]Select a doctor:[/]")
                    .PageSize(8)
                    .WrapAround()
                    .UseConverter(d => $"{d.FullName} | {d.Specialize} | Balance: ${d.Balance:F2}")
                    .AddChoices(doctors));

            decimal amount = AnsiConsole.Ask<decimal>("[cyan]Amount to add ($):[/]");

            if (amount <= 0)
                throw new Exception_Class("Amount must be greater than zero.");

            if (!AnsiConsole.Confirm($"Add [green]${amount:F2}[/] to {Markup.Escape(doctor.FullName)}?"))
                return;

            doctor.Balance += amount;
            await SaveDoctorBalancesAsync(doctors);

            ShowSuccess($"${amount:F2} added to {doctor.FullName}. New balance: ${doctor.Balance:F2}");
        }

        private static async Task SaveDoctorBalancesAsync(List<Doctor> doctors)
        {
            string? directory = Path.GetDirectoryName(DoctorBalanceFilePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            Dictionary<int, decimal> balances = doctors.ToDictionary(
                doctor => doctor.Id,
                doctor => doctor.Balance);

            string json = JsonSerializer.Serialize(balances, CachedJsonOptions);
            await File.WriteAllTextAsync(DoctorBalanceFilePath, json);
        }

        private static async Task LoadDoctorBalancesAsync(List<Doctor> doctors)
        {
            if (!File.Exists(DoctorBalanceFilePath))
                return;

            string content = await File.ReadAllTextAsync(DoctorBalanceFilePath);

            if (string.IsNullOrWhiteSpace(content))
                return;

            Dictionary<int, decimal> balances =
                JsonSerializer.Deserialize<Dictionary<int, decimal>>(content) ?? [];

            foreach (Doctor doctor in doctors)
            {
                if (balances.TryGetValue(doctor.Id, out decimal balance))
                    doctor.Balance = balance;
            }
        }
    }

    internal class StaffDoctor(
        int id,
        string fullName,
        string email,
        string pn,
        string password,
        Doctor_ENUMS specialize,
        int expertise)
        : Doctor(id, fullName, email, pn, password, specialize, 0, expertise);
}