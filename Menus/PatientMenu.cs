using Hospital_Project.Enums;
using Hospital_Project.Exceptions;
using Hospital_Project.Helpers;
using Hospital_Project.Models;
using Hospital_Project.Services;
using Spectre.Console;
using static Hospital_Project.Helpers.Translator;

namespace Hospital_Project.Menus
{
    internal static class PatientMenu
    {
        public static async Task RunAsync(
            Patient loggedInUser,
            List<Patient> registeredUsers,
            List<Doctor> doctors)
        {
            AnsiConsole.Write(
                new Panel(
                    $"[bold green]{T("Welcome back")}, {Markup.Escape(loggedInUser.FullName)}![/]\n" +
                    $"[grey]{T("Patient Portal")}[/]")
                    .BorderColor(Color.Green)
                    .RoundedBorder());

            bool running = true;

            while (running)
            {
                string choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[green]{T("Patient Menu")}[/]")
                        .PageSize(13)
                        .WrapAround()
                        .AddChoices(
                            T("View Profile"),
                            T("Update Profile"),
                            T("Update Medical Info"),
                            T("Make an Appointment"),
                            T("View Balance"),
                            T("Pay Bills"),
                            T("View My Appointments"),
                            T("Reschedule Appointment"),
                            T("Cancel Appointment"),
                            T("View Appointment History"),
                            T("Change Password"),
                            T("Logout")));

                await ConsoleUI.RunSafeAsync(async () =>
                {
                    switch (choice)
                    {
                        case var c when c == T("View Profile"):
                            Tables.ShowPatientProfile(loggedInUser);
                            break;

                        case var c when c == T("Update Profile"):
                            await UpdateProfileAsync(loggedInUser, registeredUsers);
                            break;

                        case var c when c == T("Update Medical Info"):
                            await UpdateMedicalInfoAsync(loggedInUser, registeredUsers);
                            break;

                        case var c when c == T("Make an Appointment"):
                            await MakeAppointmentAsync(loggedInUser, registeredUsers, doctors);
                            break;

                        case var c when c == T("View Balance"):
                            Tables.ShowPatientBalance(loggedInUser);
                            break;

                        case var c when c == T("Pay Bills"):
                            await PayBillsAsync(loggedInUser, registeredUsers, doctors);
                            break;

                        case var c when c == T("View My Appointments"):
                            ShowMyAppointments(loggedInUser);
                            break;

                        case var c when c == T("Reschedule Appointment"):
                            await RescheduleAsync(loggedInUser, registeredUsers);
                            break;

                        case var c when c == T("Cancel Appointment"):
                            await CancelAsync(loggedInUser, registeredUsers);
                            break;

                        case var c when c == T("View Appointment History"):
                            await ShowHistoryAsync(loggedInUser);
                            break;

                        case var c when c == T("Change Password"):
                            await AuthService.ChangePasswordAsync(
                                loggedInUser,
                                () => DataService.SavePatientsAsync(registeredUsers));
                            break;

                        case var c when c == T("Logout"):
                            running = false;
                            ConsoleUI.ShowSuccess(T("Patient logged out."));
                            break;
                    }
                });
            }
        }

        private static async Task UpdateProfileAsync(Patient patient, List<Patient> registeredUsers)
        {
            AnsiConsole.Write(new Rule($"[yellow]{T("Update Profile")}[/]"));

            string newName = AnsiConsole.Ask<string>(T("New Full Name:")).Trim();
            string newEmail = AnsiConsole.Ask<string>(T("New Email:")).Trim();
            string newPhone = AnsiConsole.Ask<string>(T("New Phone Number:")).Trim();

            if (string.IsNullOrWhiteSpace(newName))
                throw new Exception_Class(T("Name cannot be empty."));

            if (!Validator.IsValidEmail(newEmail))
                throw new Exception_Class(T("Please enter a valid email."));

            if (!Validator.IsValidPhone(newPhone))
                throw new Exception_Class(T("Please enter a valid phone number."));

            bool emailUsed = registeredUsers.Any(p =>
                p.Id != patient.Id &&
                p.Email.Equals(newEmail, StringComparison.OrdinalIgnoreCase));

            if (emailUsed)
                throw new Exception_Class(T("This email is already in use."));

            if (!AnsiConsole.Confirm(T("Save profile changes?")))
                return;

            string oldName = patient.FullName, oldEmail = patient.Email, oldPhone = patient.PN;

            patient.FullName = newName;
            patient.Email = newEmail;
            patient.PN = newPhone;

            try
            {
                await DataService.SavePatientsAsync(registeredUsers);
            }
            catch
            {
                patient.FullName = oldName;
                patient.Email = oldEmail;
                patient.PN = oldPhone;
                throw;
            }

            ConsoleUI.ShowSuccess(T("Profile updated successfully."));
        }

        private static async Task UpdateMedicalInfoAsync(Patient patient, List<Patient> registeredUsers)
        {
            AnsiConsole.Write(new Rule($"[yellow]{T("Update Medical Info")}[/]"));

            Blood_Type_ENUMS bloodType = ConsoleUI.AskBloodType($"[cyan]{T("Select your blood type:")}[/]");

            string insurance = AnsiConsole.Prompt(
                new TextPrompt<string>(T("Insurance (leave empty for none):")).AllowEmpty()).Trim();

            string allergies = AnsiConsole.Prompt(
                new TextPrompt<string>(T("Allergies (leave empty for none):")).AllowEmpty()).Trim();

            if (insurance.Length > 60 || allergies.Length > 120)
                throw new Exception_Class(T("The text is too long."));

            if (!AnsiConsole.Confirm(T("Save profile changes?")))
                return;

            Blood_Type_ENUMS oldBlood = patient.BloodType;
            string oldInsurance = patient.Insurance, oldAllergies = patient.Allergies;

            patient.BloodType = bloodType;
            patient.Insurance = insurance;
            patient.Allergies = allergies;

            try
            {
                await DataService.SavePatientsAsync(registeredUsers);
            }
            catch
            {
                patient.BloodType = oldBlood;
                patient.Insurance = oldInsurance;
                patient.Allergies = oldAllergies;
                throw;
            }

            ConsoleUI.ShowSuccess(T("Profile updated successfully."));
        }

        private static async Task MakeAppointmentAsync(
            Patient patient,
            List<Patient> registeredUsers,
            List<Doctor> doctors)
        {
            Tables.ShowDoctors(doctors);

            Doctor chosenDoctor = AnsiConsole.Prompt(
                new SelectionPrompt<Doctor>()
                    .Title($"[cyan]{T("Select a doctor:")}[/]")
                    .PageSize(8)
                    .UseConverter(d => Markup.Escape($"{d.FullName} | {d.Specialize} | ${d.ConsultationFee:F2}/hr"))
                    .AddChoices(doctors));

            string disease = AnsiConsole.Ask<string>(T("Enter your disease or symptoms:")).Trim();

            if (string.IsNullOrWhiteSpace(disease))
                throw new Exception_Class(T("Disease or symptoms cannot be empty."));

            DateTime when = ConsoleUI.AskDateTime($"{T("Appointment date and time")} ({ConsoleUI.DateFormat}):");
            decimal duration = ConsoleUI.AskHours(T("Consultation duration in hours:"));

            AppointmentService.ValidateDate(when);

            List<Appointment> history = await DataService.LoadHistoryAsync();
            AppointmentService.EnsureNoConflict(
                registeredUsers, history, chosenDoctor.Id, patient.Id, when, duration);

            string oldDisease = patient.Disease;
            decimal oldTime = patient.Consultation_Time;

            patient.Disease = disease;
            patient.Consultation_Time = duration;

            decimal price = chosenDoctor.CalculatePrice(patient);

            AnsiConsole.Write(
                new Panel(
                    $"[bold]{T("Doctor")}:[/] {Markup.Escape(chosenDoctor.FullName)}\n" +
                    $"[bold]{T("Specialty")}:[/] {Markup.Escape(chosenDoctor.Specialize.ToString())}\n" +
                    $"[bold]{T("Date & Time")}:[/] {AppointmentService.FormatTime(when)}\n" +
                    $"[bold]{T("Duration")}[/] {duration} {T("hours")}\n" +
                    $"[bold]{T("Estimated Cost:")}[/] [green]${price:F2}[/]")
                    .BorderColor(Color.Yellow)
                    .RoundedBorder());

            if (!AnsiConsole.Confirm(T("Confirm appointment?")))
            {
                patient.Disease = oldDisease;
                patient.Consultation_Time = oldTime;
                return;
            }

            AppointmentInfo newAppointment = new()
            {
                DoctorId = chosenDoctor.Id,
                DoctorName = chosenDoctor.FullName,
                Disease = disease,
                Hours = duration,
                Cost = price,
                IsPaid = false,
                ScheduledAt = when,
                Status = AppointmentStatus.Scheduled
            };

            patient.PendingBill += price;
            patient.Appointments.Add(newAppointment);

            try
            {
                await DataService.SavePatientsAsync(registeredUsers);
            }
            catch
            {
                patient.PendingBill -= price;
                patient.Appointments.Remove(newAppointment);
                patient.Disease = oldDisease;
                patient.Consultation_Time = oldTime;
                throw;
            }

            Logger.Log($"Appointment booked: patient {patient.Id} with doctor {chosenDoctor.Id} at {AppointmentService.FormatTime(when)}");

            ConsoleUI.ShowSuccess(T("Appointment confirmed."));
            AnsiConsole.MarkupLine(
                $"[grey]{T("Pending Bill")}: [red]${patient.PendingBill:F2}[/][/]");
        }

        private static void ShowMyAppointments(Patient patient)
        {
            if (patient.Appointments.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{T("You have no appointments.")}[/]");
                return;
            }

            List<AppointmentRow> rows = patient.Appointments
                .OrderBy(a => a.ScheduledAt)
                .Select(a => new AppointmentRow(patient, a))
                .ToList();

            Tables.ShowAppointments(T("My Appointments"), rows, false, true);
        }

        private static AppointmentInfo PickAppointment(List<AppointmentInfo> choices)
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<AppointmentInfo>()
                    .Title($"[cyan]{T("Select an appointment:")}[/]")
                    .PageSize(8)
                    .UseConverter(a => Markup.Escape(
                        $"{a.DoctorName} | {AppointmentService.FormatTime(a.ScheduledAt)} | {a.Disease} | {a.Status}"))
                    .AddChoices(choices));
        }

        private static async Task RescheduleAsync(Patient patient, List<Patient> registeredUsers)
        {
            List<AppointmentInfo> movable = patient.Appointments
                .Where(a => a.Status == AppointmentStatus.Scheduled)
                .ToList();

            if (movable.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{T("You have no appointments that can be rescheduled.")}[/]");
                return;
            }

            AppointmentInfo chosen = PickAppointment(movable);

            DateTime newTime = ConsoleUI.AskDateTime($"{T("New date and time")} ({ConsoleUI.DateFormat}):");
            AppointmentService.ValidateDate(newTime);

            List<Appointment> history = await DataService.LoadHistoryAsync();
            AppointmentService.EnsureNoConflict(
                registeredUsers, history, chosen.DoctorId, patient.Id, newTime, chosen.Hours, chosen);

            if (!AnsiConsole.Confirm(T("Reschedule this appointment?")))
                return;

            DateTime oldTime = chosen.ScheduledAt;
            chosen.ScheduledAt = newTime;

            try
            {
                await DataService.SavePatientsAsync(registeredUsers);
            }
            catch
            {
                chosen.ScheduledAt = oldTime;
                throw;
            }

            Logger.Log($"Appointment rescheduled: patient {patient.Id}, doctor {chosen.DoctorId}, new time {AppointmentService.FormatTime(newTime)}");
            ConsoleUI.ShowSuccess(T("Appointment rescheduled."));
        }

        private static async Task CancelAsync(Patient patient, List<Patient> registeredUsers)
        {
            List<AppointmentInfo> cancellable = patient.Appointments
                .Where(a => AppointmentService.GetAllowedNext(a.Status).Contains(AppointmentStatus.Cancelled))
                .ToList();

            if (cancellable.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{T("You have no appointments that can be cancelled.")}[/]");
                return;
            }

            AppointmentInfo chosen = PickAppointment(cancellable);

            if (!AnsiConsole.Confirm(T("Cancel this appointment?")))
                return;

            await AppointmentService.CancelAsync(patient, chosen, registeredUsers);

            ConsoleUI.ShowSuccess(T("Appointment cancelled."));
        }

        private static async Task PayBillsAsync(
            Patient patient,
            List<Patient> registeredUsers,
            List<Doctor> doctors)
        {
            if (patient.PendingBill <= 0)
            {
                AnsiConsole.MarkupLine($"[green]{T("You have no unpaid bills.")}[/]");
                return;
            }

            patient.Appointments ??= new List<AppointmentInfo>();

            AnsiConsole.Write(
                new Panel(
                    $"[bold]{T("Amount Due:")}[/] [red]${patient.PendingBill:F2}[/]\n" +
                    $"[bold]{T("Available Balance:")}[/] [green]${patient.Balance:F2}[/]")
                    .BorderColor(Color.Yellow)
                    .RoundedBorder());

            if (patient.Balance < patient.PendingBill)
            {
                throw new Exception_Class(
                    $"{T("Insufficient balance.")} " +
                    $"${patient.PendingBill - patient.Balance:F2}");
            }

            if (!AnsiConsole.Confirm(T("Continue with payment?")))
                return;

            string pin = AnsiConsole.Ask<string>(T("Enter your 4-digit payment PIN:")).Trim();

            if (pin != patient.Pin)
                throw new Exception_Class(T("Incorrect PIN. Payment cancelled."));

            List<AppointmentInfo> appointmentsToPay =
                patient.Appointments.Where(a => !a.IsPaid).ToList();

            if (appointmentsToPay.Count == 0)
                throw new Exception_Class(T("No unpaid appointments were found."));

            decimal appointmentTotal = appointmentsToPay.Sum(a => a.Cost);

            if (appointmentTotal != patient.PendingBill)
            {
                throw new Exception_Class(
                    T("The pending bill does not match the total of unpaid appointments. Please contact the administrator."));
            }

            List<Appointment> appointmentHistory = appointmentsToPay
                .Select(a => AppointmentService.ToHistory(patient, a, a.Cost, true))
                .ToList();

            await DataService.AppendHistoryAsync(appointmentHistory);

            decimal amountToPay = patient.PendingBill;
            patient.Balance -= amountToPay;
            patient.PendingBill = 0;
            patient.Consultation_Time = 0;

            foreach (AppointmentInfo appointment in appointmentsToPay)
            {
                Doctor? doctor = doctors.FirstOrDefault(d => d.Id == appointment.DoctorId);

                if (doctor is not null)
                    doctor.Balance += appointment.Cost;

                appointment.IsPaid = true;
            }

            patient.Appointments.RemoveAll(a => a.IsPaid);

            await DataService.SavePatientsAsync(registeredUsers);
            await DataService.SaveDoctorBalancesAsync(doctors);

            Logger.Log($"Bill paid: patient {patient.Id}, ${amountToPay:F2}");

            AnsiConsole.Write(
                new Panel(
                    $"[bold green]{T("Payment Successful!")}[/]\n" +
                    $"[bold]{T("Paid")}:[/] ${amountToPay:F2}\n" +
                    $"[bold]{T("Remaining Balance:")}[/] ${patient.Balance:F2}\n" +
                    $"[bold]{T("Appointments moved to history:")}[/] {appointmentHistory.Count}")
                    .BorderColor(Color.Green)
                    .RoundedBorder());
        }

        private static async Task ShowHistoryAsync(Patient patient)
        {
            List<Appointment> allAppointments = await DataService.LoadHistoryAsync();

            if (allAppointments.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{T("Your appointment history is empty.")}[/]");
                return;
            }

            List<Appointment> patientHistory = allAppointments
                .Where(a => a.PatientId == patient.Id)
                .OrderByDescending(a => a.History)
                .ToList();

            if (patientHistory.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{T("You have no paid appointment history yet.")}[/]");
                return;
            }

            Tables.ShowHistory(patientHistory);

            decimal totalPaid = Filters.SumBy(patientHistory.Where(a => a.IsPaid), a => a.Payment);

            AnsiConsole.MarkupLine(
                $"\n[bold]{T("Total paid appointments:")}[/] {patientHistory.Count(a => a.IsPaid)}");

            AnsiConsole.MarkupLine(
                $"[bold]{T("Total amount paid:")}[/] [green]${totalPaid:F2}[/]");
        }
    }
}
