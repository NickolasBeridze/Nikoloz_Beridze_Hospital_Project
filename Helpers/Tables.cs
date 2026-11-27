using Hospital_Project.Enums;
using Hospital_Project.Models;
using Hospital_Project.Services;
using Spectre.Console;
using static Hospital_Project.Helpers.Translator;

namespace Hospital_Project.Helpers
{
    internal static class Tables
    {
        private static string Money(decimal amount) => $"[green]${amount:F2}[/]";

        private static string Bill(decimal amount)
            => amount > 0 ? $"[red]${amount:F2}[/]" : "[green]$0.00[/]";

        public static string StatusText(AppointmentStatus status) => status switch
        {
            AppointmentStatus.Scheduled => "[yellow]Scheduled[/]",
            AppointmentStatus.Confirmed => "[cyan]Confirmed[/]",
            AppointmentStatus.Completed => "[green]Completed[/]",
            AppointmentStatus.Cancelled => "[red]Cancelled[/]",
            _ => status.ToString()
        };

        public static void ShowDoctors(List<Doctor> doctors)
        {
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Cyan1)
                .Title($"[bold cyan]{T("Hospital Doctors")}[/]")
                .Expand();

            table.AddColumn(T("ID"));
            table.AddColumn(T("Doctor"));
            table.AddColumn(T("Specialty"));
            table.AddColumn(T("Experience"));
            table.AddColumn(T("Fee / Hour"));

            Filters.ForEachItem(doctors, doctor => table.AddRow(
                doctor.Id.ToString(),
                Markup.Escape(doctor.FullName),
                Markup.Escape(doctor.Specialize.ToString()),
                $"{doctor.Expertise} years",
                Money(doctor.ConsultationFee)));

            AnsiConsole.Write(table);
        }

        public static void ShowDoctorsDetailed(List<Doctor> doctors)
        {
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Cyan1)
                .Title($"[bold cyan]{T("Doctor Directory")}[/]")
                .Expand();

            table.AddColumn(T("ID"));
            table.AddColumn(T("Name"));
            table.AddColumn(T("Email"));
            table.AddColumn(T("Specialty"));
            table.AddColumn(T("Phone"));
            table.AddColumn(T("Experience"));
            table.AddColumn(T("Fee"));
            table.AddColumn(T("Balance"));

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
                    Money(doctor.Balance));
            }

            AnsiConsole.Write(table);
        }

        public static void ShowDoctorProfile(Doctor doctor)
        {
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Cyan1)
                .Title($"[bold cyan]{T("Doctor Profile")}[/]");

            table.AddColumn(T("Field"));
            table.AddColumn(T("Information"));

            table.AddRow(T("ID"), doctor.Id.ToString());
            table.AddRow(T("Name"), Markup.Escape(doctor.FullName));
            table.AddRow(T("Email"), Markup.Escape(doctor.Email));
            table.AddRow(T("Phone"), Markup.Escape(doctor.PN));
            table.AddRow(T("Specialty"), Markup.Escape(doctor.Specialize.ToString()));
            table.AddRow(T("Experience"), $"{doctor.Expertise} years");
            table.AddRow(T("Consultation Fee"), $"[green]${doctor.ConsultationFee:F2}/hr[/]");
            table.AddRow(T("Account Balance"), Money(doctor.Balance));

            AnsiConsole.Write(table);
        }

        public static void ShowPatients(List<Patient> patients)
        {
            if (patients.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{T("No registered patients found.")}[/]");
                return;
            }

            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .Title($"[bold blue]{T("Registered Patients")}[/]")
                .Expand();

            table.AddColumn(T("ID"));
            table.AddColumn(T("Name"));
            table.AddColumn(T("Email"));
            table.AddColumn(T("Disease"));
            table.AddColumn(T("Phone"));
            table.AddColumn(T("Insurance"));
            table.AddColumn(T("Blood Type"));
            table.AddColumn(T("Balance"));
            table.AddColumn(T("Pending Bill"));

            foreach (Patient patient in patients)
            {
                table.AddRow(
                    patient.Id.ToString(),
                    Markup.Escape(patient.FullName),
                    Markup.Escape(patient.Email),
                    Markup.Escape(string.IsNullOrWhiteSpace(patient.Disease) ? "None" : patient.Disease),
                    Markup.Escape(patient.PN),
                    Markup.Escape(string.IsNullOrWhiteSpace(patient.Insurance) ? "N/A" : patient.Insurance),
                    Markup.Escape(BloodTypeHelper.ToText(patient.BloodType)),
                    Money(patient.Balance),
                    Bill(patient.PendingBill));
            }

            AnsiConsole.Write(table);
        }

        public static void ShowPatientProfile(Patient patient)
        {
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green)
                .Title($"[bold green]{T("Patient Profile")}[/]");

            table.AddColumn(T("Field"));
            table.AddColumn(T("Information"));

            table.AddRow(T("Patient ID"), patient.Id.ToString());
            table.AddRow(T("Full Name"), Markup.Escape(patient.FullName));
            table.AddRow(T("Email"), Markup.Escape(patient.Email));
            table.AddRow(T("Phone"), Markup.Escape(patient.PN));
            table.AddRow(T("Disease"), Markup.Escape(
                string.IsNullOrWhiteSpace(patient.Disease) ? T("None recorded") : patient.Disease));
            table.AddRow(T("Blood Type"), Markup.Escape(BloodTypeHelper.ToText(patient.BloodType)));
            table.AddRow(T("Balance"), Money(patient.Balance));
            table.AddRow(T("Pending Bill"), Bill(patient.PendingBill));

            AnsiConsole.Write(table);
        }

        public static void ShowPatientSummary(Patient patient, List<Appointment> history)
        {
            List<Appointment> paid = history.Where(a => a.PatientId == patient.Id && a.IsPaid).ToList();
            decimal totalPaid = Filters.SumBy(paid, a => a.Payment);

            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .Title($"[bold blue]{T("Patient Summary")}[/]");

            table.AddColumn(T("Field"));
            table.AddColumn(T("Information"));

            table.AddRow(T("Patient ID"), patient.Id.ToString());
            table.AddRow(T("Full Name"), Markup.Escape(patient.FullName));
            table.AddRow(T("Email"), Markup.Escape(patient.Email));
            table.AddRow(T("Phone"), Markup.Escape(patient.PN));
            table.AddRow(T("Disease"), Markup.Escape(
                string.IsNullOrWhiteSpace(patient.Disease) ? T("None recorded") : patient.Disease));
            table.AddRow(T("Insurance"), Markup.Escape(
                string.IsNullOrWhiteSpace(patient.Insurance) ? "N/A" : patient.Insurance));
            table.AddRow(T("Allergies"), Markup.Escape(
                string.IsNullOrWhiteSpace(patient.Allergies) ? T("None recorded") : patient.Allergies));
            table.AddRow(T("Blood Type"), Markup.Escape(BloodTypeHelper.ToText(patient.BloodType)));
            table.AddRow(T("Can donate to"),
                Markup.Escape(BloodTypeHelper.JoinText(BloodTypeHelper.GetRecipients(patient.BloodType))));
            table.AddRow(T("Can receive from"),
                Markup.Escape(BloodTypeHelper.JoinText(BloodTypeHelper.GetDonors(patient.BloodType))));
            table.AddRow(T("Balance"), Money(patient.Balance));
            table.AddRow(T("Pending Bill"), Bill(patient.PendingBill));
            table.AddRow(T("Active appointments"), patient.Appointments.Count.ToString());
            table.AddRow(T("Total paid appointments:"), paid.Count.ToString());
            table.AddRow(T("Total amount paid:"), Money(totalPaid));

            AnsiConsole.Write(table);
        }

        public static void ShowPatientBalance(Patient patient)
        {
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Yellow)
                .Title($"[bold yellow]{T("Account Balance")}[/]");

            table.AddColumn(T("Account"));
            table.AddColumn(T("Amount"));

            table.AddRow(T("Available Balance"), Money(patient.Balance));
            table.AddRow(T("Pending Bill"), Bill(patient.PendingBill));

            AnsiConsole.Write(table);
        }

        public static void ShowAppointments(string title, List<AppointmentRow> rows, bool showPatientColumn, bool showDoctorColumn)
        {
            if (rows.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{T("No appointments found.")}[/]");
                return;
            }

            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .Title($"[bold blue]{Markup.Escape(title)}[/]")
                .Expand();

            if (showPatientColumn) table.AddColumn(T("Patient"));
            if (showDoctorColumn) table.AddColumn(T("Doctor"));
            table.AddColumn(T("Disease"));
            table.AddColumn(T("Date & Time"));
            table.AddColumn(T("Hours"));
            table.AddColumn(T("Cost"));
            table.AddColumn(T("Status"));
            table.AddColumn(T("Payment"));

            foreach (AppointmentRow row in rows)
            {
                AppointmentInfo a = row.Appointment;
                List<string> cells = [];

                if (showPatientColumn) cells.Add(Markup.Escape(row.Patient.FullName));
                if (showDoctorColumn) cells.Add(Markup.Escape(a.DoctorName));

                cells.Add(Markup.Escape(a.Disease));
                cells.Add(AppointmentService.FormatTime(a.ScheduledAt));
                cells.Add(a.Hours.ToString());
                cells.Add(Money(a.Cost));
                cells.Add(StatusText(a.Status));
                cells.Add(a.IsPaid ? $"[green]{T("Paid")}[/]" : $"[red]{T("Unpaid")}[/]");

                table.AddRow(cells.ToArray());
            }

            AnsiConsole.Write(table);
        }

        public static void ShowHistory(List<Appointment> history)
        {
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green)
                .Title($"[bold green]{T("My Appointment History")}[/]")
                .Expand();

            table.AddColumn(T("Doctor"));
            table.AddColumn(T("Disease"));
            table.AddColumn(T("Hours"));
            table.AddColumn(T("Payment"));
            table.AddColumn(T("Status"));
            table.AddColumn(T("Visit Time"));
            table.AddColumn(T("Date & Time"));

            foreach (Appointment appointment in history)
            {
                string payStatus = appointment.IsPaid
                    ? $"[green]{T("Paid")}[/]"
                    : $"[red]{T("Unpaid")}[/]";

                table.AddRow(
                    Markup.Escape(appointment.DoctorName),
                    Markup.Escape(appointment.Disease),
                    $"{appointment.Hours:F2}",
                    Money(appointment.Payment),
                    $"{StatusText(appointment.Status)} / {payStatus}",
                    AppointmentService.FormatTime(appointment.ScheduledAt),
                    appointment.History.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            AnsiConsole.Write(table);
        }
    }
}
