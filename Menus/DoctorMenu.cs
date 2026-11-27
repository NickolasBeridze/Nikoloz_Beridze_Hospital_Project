using Hospital_Project.Enums;
using Hospital_Project.Exceptions;
using Hospital_Project.Helpers;
using Hospital_Project.Models;
using Hospital_Project.Services;
using Spectre.Console;
using static Hospital_Project.Helpers.Translator;

namespace Hospital_Project.Menus
{
    internal static class DoctorMenu
    {
        public static async Task RunAsync(Doctor doctor, Admin admin, List<Doctor> doctors)
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
                        .Title($"[cyan]{T("Doctor Menu")}[/]")
                        .WrapAround()
                        .AddChoices(
                            T("View Doctor Profile"),
                            T("View Appointments"),
                            T("Manage Appointments"),
                            T("Change Password"),
                            T("Logout")));

                await ConsoleUI.RunSafeAsync(async () =>
                {
                    switch (choice)
                    {
                        case var c when c == T("View Doctor Profile"):
                            Tables.ShowDoctorProfile(doctor);
                            break;

                        case var c when c == T("View Appointments"):
                            await ShowAppointmentsAsync(doctor);
                            break;

                        case var c when c == T("Manage Appointments"):
                            await ManageAppointmentsAsync(doctor);
                            break;

                        case var c when c == T("Change Password"):
                            await AuthService.ChangePasswordAsync(
                                doctor,
                                () => DataService.SaveStaffPasswordsAsync(admin, doctors));
                            break;

                        case var c when c == T("Logout"):
                            running = false;
                            ConsoleUI.ShowSuccess(T("Doctor logged out."));
                            break;
                    }
                });
            }
        }

        private static async Task ShowAppointmentsAsync(Doctor doctor)
        {
            List<Patient> patients = await DataService.LoadPatientsAsync();

            List<AppointmentRow> rows = AppointmentService.GetRows(patients, a => a.DoctorId == doctor.Id);

            Tables.ShowAppointments(T("View Appointments"), rows, true, false);
        }

        private static async Task ManageAppointmentsAsync(Doctor doctor)
        {
            List<Patient> patients = await DataService.LoadPatientsAsync();

            List<AppointmentRow> rows = AppointmentService.GetRows(
                patients,
                a => a.DoctorId == doctor.Id && AppointmentService.GetAllowedNext(a.Status).Count > 0);

            if (rows.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{T("No appointments found.")}[/]");
                return;
            }

            AppointmentRow chosen = AnsiConsole.Prompt(
                new SelectionPrompt<AppointmentRow>()
                    .Title($"[cyan]{T("Select an appointment:")}[/]")
                    .PageSize(8)
                    .UseConverter(r => Markup.Escape(
                        $"{r.Patient.FullName} | {AppointmentService.FormatTime(r.Appointment.ScheduledAt)} | {r.Appointment.Status}"))
                    .AddChoices(rows));

            AppointmentStatus newStatus = AnsiConsole.Prompt(
                new SelectionPrompt<AppointmentStatus>()
                    .Title($"[cyan]{T("Select the new status:")}[/]")
                    .AddChoices(AppointmentService.GetAllowedNext(chosen.Appointment.Status)));

            if (!AnsiConsole.Confirm(T("Change the status of this appointment?")))
                return;

            if (newStatus == AppointmentStatus.Cancelled)
            {
                await AppointmentService.CancelAsync(chosen.Patient, chosen.Appointment, patients);
            }
            else
            {
                AppointmentStatus oldStatus = chosen.Appointment.Status;
                AppointmentService.ChangeStatus(chosen.Appointment, newStatus);

                try
                {
                    await DataService.SavePatientsAsync(patients);
                }
                catch
                {
                    chosen.Appointment.Status = oldStatus;
                    throw;
                }

                Logger.Log($"Appointment status: doctor {doctor.Id}, patient {chosen.Patient.Id}, {oldStatus} -> {newStatus}");
            }

            ConsoleUI.ShowSuccess(T("Appointment status updated."));
        }
    }
}
