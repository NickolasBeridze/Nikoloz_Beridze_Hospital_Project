using Hospital_Project.Enums;
using Hospital_Project.Exceptions;
using Hospital_Project.Helpers;
using Hospital_Project.Models;
using Hospital_Project.Services;
using Spectre.Console;
using static Hospital_Project.Helpers.Translator;

namespace Hospital_Project.Menus
{
    internal class HospitalApp
    {
        private readonly Admin admin = new(Admin.DefaultEmail, Admin.DefaultPassword);

        private readonly List<Doctor> doctors =
        [
            new StaffDoctor(1, "Dr. Giorgi Beridze", "g.beridze@hospital.com", "555-0101", "GDoc123", Doctor_ENUMS.GENERAL_DOCTOR, 5),
            new StaffDoctor(2, "Dr. Nino Kapanadze", "n.kapanadze@hospital.com", "555-0102", "NDoc123", Doctor_ENUMS.SURGEON, 12),
            new StaffDoctor(3, "Dr. Luka Lomidze", "l.lomidze@hospital.com", "555-0103", "LDoc123", Doctor_ENUMS.CARDIOLOGIST, 9),
            new StaffDoctor(4, "Dr. Mariam Tsiklauri", "m.tsiklauri@hospital.com", "555-0104", "MDoc123", Doctor_ENUMS.PEDIATRICIAN, 7),
            new StaffDoctor(5, "Dr. Davit Maisuradze", "d.maisuradze@hospital.com", "555-0105", "DDoc123", Doctor_ENUMS.DERMATOLOGIST, 6),
            new StaffDoctor(6, "Dr. Tamar Gelashvili", "t.gelashvili@hospital.com", "555-0106", "TDoc123", Doctor_ENUMS.NEUROLOGIST, 10),
            new StaffDoctor(7, "Dr. Sandro Chkhaidze", "s.chkhaidze@hospital.com", "555-0107", "SDoc123", Doctor_ENUMS.DENTIST, 4)
        ];

        public async Task RunAsync()
        {
            Logger.OnEvent += Logger.WriteToFile;
            Logger.OnEvent += Logger.WriteToConsole;

            await LoadAllAsync();
            await MainMenuAsync();
        }

        private async Task LoadAllAsync()
        {
            ConsoleUI.DeferWarnings = true;

            Dictionary<string, Func<Task>> steps = new()
            {
                [T("Loading doctors...")] = () => DataService.LoadDoctorBalancesAsync(doctors),
                [T("Loading account security...")] = () => DataService.LoadStaffPasswordsAsync(admin, doctors),
                [T("Loading patient records...")] = async () => { await DataService.LoadPatientsAsync(); },
                [T("Loading appointments...")] = async () => { await DataService.LoadHistoryAsync(); }
            };

            await ConsoleUI.ShowLoadingAsync(steps);
            ConsoleUI.FlushWarnings();
        }

        private async Task MainMenuAsync()
        {
            bool running = true;

            while (running)
            {
                AnsiConsole.Write(new Rule($"[cyan]{T("Main Menu")}[/]").RuleStyle(Color.Grey));

                string choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[cyan]{T("Select a service:")}[/]")
                        .HighlightStyle(new Style(Color.Cyan1, decoration: Decoration.Bold))
                        .WrapAround()
                        .AddChoices(
                            T("Patient Registration"),
                            T("Login"),
                            T("View Doctors"),
                            T("Search Doctors"),
                            T("Exit")));

                await ConsoleUI.RunSafeAsync(async () =>
                {
                    switch (choice)
                    {
                        case var c when c == T("Patient Registration"):
                            await RegisterPatientAsync();
                            break;

                        case var c when c == T("Login"):
                            await LoginAsync();
                            break;

                        case var c when c == T("View Doctors"):
                            Tables.ShowDoctors(doctors);
                            break;

                        case var c when c == T("Search Doctors"):
                            SearchDoctors();
                            break;

                        case var c when c == T("Exit"):
                            running = false;
                            AnsiConsole.MarkupLine(
                                $"\n[grey]{Markup.Escape(T("Thank you for using Hospital Management System."))}[/]");
                            break;
                    }
                });
            }
        }

        private void SearchDoctors()
        {
            Doctor_ENUMS specialty = AnsiConsole.Prompt(
                new SelectionPrompt<Doctor_ENUMS>()
                    .Title($"[cyan]{T("Select a specialty:")}[/]")
                    .AddChoices(Enum.GetValues<Doctor_ENUMS>()));

            List<Doctor> results = Filters.Find(doctors, Filters.DoctorBySpecialty(specialty));

            if (results.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{T("No doctors found.")}[/]");
                return;
            }

            Tables.ShowDoctors(results);
        }

        private async Task RegisterPatientAsync()
        {
            AnsiConsole.Write(new Rule($"[green]{T("Patient Registration")}[/]"));

            string fullName = AnsiConsole.Ask<string>(T("Full Name:")).Trim();
            string email = AnsiConsole.Ask<string>(T("Email:")).Trim();
            string phone = AnsiConsole.Ask<string>(T("Phone Number:")).Trim();
            string password = ConsoleUI.AskSecret(T("Password:"));
            string confirmation = ConsoleUI.AskSecret(T("Confirm Password:"));
            string pin = AnsiConsole.Ask<string>(T("Create a 4-digit Payment PIN:")).Trim();

            if (string.IsNullOrWhiteSpace(fullName))
                throw new Exception_Class(T("Full name cannot be empty."));

            if (!Validator.IsValidEmail(email))
                throw new Exception_Class(T("Please enter a valid email address."));

            if (email.Equals(admin.Email, StringComparison.OrdinalIgnoreCase) ||
                doctors.Any(d => d.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                throw new Exception_Class(T("This email is reserved."));

            if (!Validator.IsValidPhone(phone))
                throw new Exception_Class(T("Please enter a valid phone number."));

            Validator.ValidateNewPassword(password, string.Empty);

            if (password != confirmation)
                throw Exception_Class.Passwords_Do_Not_Match();

            if (!Validator.IsValidPin(pin))
                throw new Exception_Class(T("PIN must be exactly 4 digits."));

            List<Patient> users = await DataService.LoadPatientsAsync();

            if (users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                throw new Exception_Class(T("A user with this email already exists."));

            int newId = users.Count == 0 ? 1 : users.Max(u => u.Id) + 1;

            if (users.Any(u => u.Id == newId))
                throw Exception_Class.Duplicate_Id(newId);

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
            await DataService.SavePatientsAsync(users);

            AnsiConsole.Write(
                new Panel($"[bold green]{T("Registration completed successfully!")}[/]")
                    .BorderColor(Color.Green)
                    .RoundedBorder());

            AnsiConsole.MarkupLine($"[grey]{T("Your Patient ID:")} [white]{newId}[/][/]");
        }

        private async Task LoginAsync()
        {
            AnsiConsole.Write(new Rule($"[yellow]{T("Login")}[/]"));

            string email = AnsiConsole.Ask<string>(T("Email:")).Trim();

            AuthService.EnsureNotLocked(email);

            string password = ConsoleUI.AskSecret(T("Password:"));
            List<Patient> patients = await DataService.LoadPatientsAsync();

            User user = AuthService.Login(email, password, admin, doctors, patients);

            switch (user)
            {
                case Admin loggedAdmin:
                    await AdminMenu.RunAsync(loggedAdmin, doctors);
                    break;

                case Doctor loggedDoctor:
                    await DoctorMenu.RunAsync(loggedDoctor, admin, doctors);
                    break;

                case Patient loggedPatient:
                    await PatientMenu.RunAsync(loggedPatient, patients, doctors);
                    break;
            }
        }
    }
}
