using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Hospital_Project.Enums;
using Hospital_Project.Exceptions;
using Hospital_Project.Models;
using Spectre.Console;

namespace Hospital_Project;

internal class Program
{
    private const string PatientDataFilePath =
        @"C:\Users\User\source\repos\Hospital_Project\Text_Files\Patient_Data.txt";

    private const string DoctorBalanceFilePath =
        @"C:\Users\User\source\repos\Hospital_Project\Text_Files\Doctor_Balances.txt";

    private const string AppointmentFilePath =
        @"C:\Users\User\source\repos\Hospital_Project\Text_Files\Appointment.txt";

    private static readonly JsonSerializerOptions CachedJsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new FlexibleBooleanConverter() }
    };

    private const string AdminEmail = "admin@hospital.com";
    private const string AdminPassword = "admin123";

    private static string CurrentLanguage = "English";

    private static readonly Dictionary<string, string> GeorgianTranslations = new()
    {
        ["Select Language / აირჩიეთ ენა"] = "აირჩიეთ ენა / Select Language",
        ["English"] = "ინგლისური",
        ["Georgian"] = "ქართული",

        ["HOSPITAL"] = "საავადმყოფო",
        ["Hospital Management System"] = "საავადმყოფოს მართვის სისტემა",
        ["Patient • Doctor • Administration"] = "პაციენტი • ექიმი • ადმინისტრაცია",
        ["Made By Nikoloz Beridze"] = "შექმნილია ნიკოლოზ ბერიძის მიერ",

        ["Main Menu"] = "მთავარი მენიუ",
        ["Select a service:"] = "აირჩიეთ სერვისი:",
        ["Patient Registration"] = "პაციენტის რეგისტრაცია",
        ["Login"] = "შესვლა",
        ["View Doctors"] = "ექიმების ნახვა",
        ["Exit"] = "გასვლა",
        ["Thank you for using Hospital Management System."] = "გმადლობთ საავადმყოფოს მართვის სისტემის გამოყენებისთვის.",

        ["Full Name:"] = "სრული სახელი:",
        ["Email:"] = "ელ. ფოსტა:",
        ["Phone Number:"] = "ტელეფონის ნომერი:",
        ["Password:"] = "პაროლი:",
        ["Create a 4-digit Payment PIN:"] = "შექმენით გადახდის 4-ნიშნა PIN:",
        ["Registration completed successfully!"] = "რეგისტრაცია წარმატებით დასრულდა!",
        ["Your Patient ID:"] = "თქვენი პაციენტის ID:",

        ["Administrator Access"] = "ადმინისტრატორის წვდომა",
        ["Admin Menu"] = "ადმინისტრატორის მენიუ",
        ["View All Patients"] = "ყველა პაციენტის ნახვა",
        ["Add Balance to Patient"] = "პაციენტის ბალანსის შევსება",
        ["View All Doctors"] = "ყველა ექიმის ნახვა",
        ["Add Balance to Doctor"] = "ექიმის ბალანსის შევსება",
        ["Logout"] = "გამოსვლა",
        ["Administrator logged out."] = "ადმინისტრატორი გამოვიდა სისტემიდან.",

        ["Hospital Doctors"] = "საავადმყოფოს ექიმები",
        ["Doctor"] = "ექიმი",
        ["Specialty"] = "სპეციალობა",
        ["Experience"] = "გამოცდილება",
        ["Fee / Hour"] = "საათობრივი საფასური",
        ["Doctor Directory"] = "ექიმების სია",
        ["Name"] = "სახელი",
        ["Email"] = "ელ. ფოსტა",
        ["Phone"] = "ტელეფონი",
        ["Disease"] = "დაავადება",
        ["Insurance"] = "დაზღვევა",
        ["Blood Type"] = "სისხლის ჯგუფი",
        ["Balance"] = "ბალანსი",
        ["Pending Bill"] = "გადასახდელი თანხა",

        ["Doctor Menu"] = "ექიმის მენიუ",
        ["View Doctor Profile"] = "ექიმის პროფილის ნახვა",
        ["View Appointments"] = "ვიზიტების ნახვა",
        ["Doctor Profile"] = "ექიმის პროფილი",
        ["Field"] = "ველი",
        ["Information"] = "ინფორმაცია",
        ["Consultation Fee"] = "კონსულტაციის საფასური",
        ["Account Balance"] = "ანგარიშის ბალანსი",
        ["Doctor logged out."] = "ექიმი გამოვიდა სისტემიდან.",

        ["Patient Menu"] = "პაციენტის მენიუ",
        ["View Profile"] = "პროფილის ნახვა",
        ["Update Profile"] = "პროფილის განახლება",
        ["Make an Appointment"] = "ვიზიტის დაჯავშნა",
        ["View Balance"] = "ბალანსის ნახვა",
        ["Pay Bills"] = "გადასახადების გადახდა",
        ["View My Appointments"] = "ჩემი ვიზიტების ნახვა",
        ["View Appointment History"] = "ვიზიტების ისტორიის ნახვა",
        ["Patient Profile"] = "პაციენტის პროფილი",
        ["Patient ID"] = "პაციენტის ID",
        ["Full Name"] = "სრული სახელი",
        ["None recorded"] = "არ არის მითითებული",
        ["Patient Portal"] = "პაციენტის პორტალი",
        ["Welcome back"] = "კეთილი იყოს თქვენი დაბრუნება",
        ["Patient logged out."] = "პაციენტი გამოვიდა სისტემიდან.",

        ["New Full Name:"] = "ახალი სრული სახელი:",
        ["New Email:"] = "ახალი ელ. ფოსტა:",
        ["New Phone Number:"] = "ახალი ტელეფონის ნომერი:",
        ["Save profile changes?"] = "გსურთ ცვლილებების შენახვა?",
        ["Profile updated successfully."] = "პროფილი წარმატებით განახლდა.",

        ["Select a doctor:"] = "აირჩიეთ ექიმი:",
        ["Enter your disease or symptoms:"] = "მიუთითეთ დაავადება ან სიმპტომები:",
        ["Consultation duration in hours:"] = "კონსულტაციის ხანგრძლივობა საათებში:",
        ["Confirm appointment?"] = "ადასტურებთ ვიზიტს?",
        ["Estimated Cost:"] = "სავარაუდო ღირებულება:",
        ["Duration:"] = "ხანგრძლივობა:",
        ["Appointment confirmed."] = "ვიზიტი წარმატებით დაიჯავშნა.",
        ["My Appointments"] = "ჩემი ვიზიტები",
        ["Hours"] = "საათები",
        ["Cost"] = "ღირებულება",
        ["Status"] = "სტატუსი",
        ["Paid"] = "გადახდილი",
        ["Unpaid"] = "გადაუხდელი",

        ["Available Balance"] = "ხელმისაწვდომი ბალანსი",
        ["Amount Due:"] = "გადასახდელი თანხა:",
        ["Available Balance:"] = "ხელმისაწვდომი ბალანსი:",
        ["Continue with payment?"] = "გსურთ გადახდის გაგრძელება?",
        ["Enter your 4-digit payment PIN:"] = "შეიყვანეთ თქვენი 4-ნიშნა გადახდის PIN:",
        ["Payment Successful!"] = "გადახდა წარმატებით შესრულდა!",
        ["Remaining Balance:"] = "დარჩენილი ბალანსი:",
        ["Appointments moved to history:"] = "ისტორიაში გადატანილი ვიზიტები:",

        ["My Appointment History"] = "ჩემი ვიზიტების ისტორია",
        ["Payment"] = "გადახდა",
        ["Date & Time"] = "თარიღი და დრო",
        ["Total paid appointments:"] = "გადახდილი ვიზიტების რაოდენობა:",
        ["Total amount paid:"] = "გადახდილი თანხის ჯამი:",

        ["No registered patients found."] = "რეგისტრირებული პაციენტები არ მოიძებნა.",
        ["No doctors found."] = "ექიმები არ მოიძებნა.",
        ["No appointments found."] = "ვიზიტები არ მოიძებნა.",
        ["No appointment history found."] = "ვიზიტების ისტორია არ მოიძებნა.",
        ["Your appointment history is empty."] = "ვიზიტების ისტორია ცარიელია.",
        ["You have no appointments."] = "ვიზიტები არ გაქვთ.",
        ["You have no paid appointment history yet."] = "გადახდილი ვიზიტების ისტორია ჯერ არ გაქვთ.",
        ["You have no unpaid bills."] = "გადასახდელი თანხა არ გაქვთ.",

        ["Full name cannot be empty."] = "სრული სახელი ცარიელი ვერ იქნება.",
        ["Please enter a valid email address."] = "შეიყვანეთ ელ. ფოსტის სწორი მისამართი.",
        ["This email is reserved."] = "ეს ელ. ფოსტა დაკავებულია.",
        ["Password cannot be empty."] = "პაროლი ცარიელი ვერ იქნება.",
        ["PIN must be exactly 4 digits."] = "PIN უნდა შედგებოდეს ზუსტად 4 ციფრისგან.",
        ["A user with this email already exists."] = "მომხმარებელი ამ ელ. ფოსტით უკვე არსებობს.",
        ["Invalid email or password."] = "ელ. ფოსტა ან პაროლი არასწორია.",
        ["Patient not found."] = "პაციენტი ვერ მოიძებნა.",
        ["Amount must be greater than zero."] = "თანხა ნულზე მეტი უნდა იყოს.",
        ["Name cannot be empty."] = "სახელი ცარიელი ვერ იქნება.",
        ["Please enter a valid email."] = "შეიყვანეთ სწორი ელ. ფოსტა.",
        ["This email is already in use."] = "ეს ელ. ფოსტა უკვე გამოიყენება.",
        ["Disease or symptoms cannot be empty."] = "დაავადება ან სიმპტომები ცარიელი ვერ იქნება.",
        ["Duration must be greater than zero."] = "ხანგრძლივობა ნულზე მეტი უნდა იყოს.",
        ["Insufficient balance."] = "ბალანსი არასაკმარისია.",
        ["Incorrect PIN. Payment cancelled."] = "PIN არასწორია. გადახდა გაუქმდა.",
        ["No unpaid appointments were found."] = "გადაუხდელი ვიზიტები ვერ მოიძებნა.",
        ["The pending bill does not match the total of unpaid appointments. Please contact the administrator."] =
            "გადასახდელი თანხა არ ემთხვევა ვიზიტების ჯამს. დაუკავშირდით ადმინისტრატორს."
    };

    private static string T(string text)
    {
        if (CurrentLanguage == "English")
            return text;

        return GeorgianTranslations.TryGetValue(text, out string? translated)
            ? translated
            : text;
    }

    private static string MenuChoice(string text) => T(text);

    static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        string language = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(T("Select Language / აირჩიეთ ენა"))
                .AddChoices("English", "ქართული"));

        CurrentLanguage = language == "ქართული" ? "Georgian" : "English";

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
            new FigletText(T("HOSPITAL"))
                .Centered()
                .Color(Color.Cyan1));

        AnsiConsole.Write(
            new Panel(
                Align.Center(
                    new Markup(
                        $"[bold white]{Markup.Escape(T("Hospital Management System"))}[/]\n" +
                        $"[grey]{Markup.Escape(T("Patient • Doctor • Administration"))}[/]\n" +
                        $"[white]{Markup.Escape(T("Made By Nikoloz Beridze"))}[/]"),
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
                        T("Exit")));

            try
            {
                switch (choice)
                {
                    case var c when c == T("Patient Registration"):
                        await RegisterPatientAsync(doctors);
                        break;

                    case var c when c == T("Login"):
                        await LoginAsync(doctors);
                        break;

                    case var c when c == T("View Doctors"):
                        ShowDoctors(doctors);
                        break;

                    case var c when c == T("Exit"):
                        running = false;
                        AnsiConsole.MarkupLine(
                            $"\n[grey]{Markup.Escape(T("Thank you for using Hospital Management System."))}[/]");
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
        AnsiConsole.Write(new Rule($"[green]{T("Patient Registration")}[/]"));

        string fullName = AnsiConsole.Ask<string>(T("Full Name:")).Trim();
        string email = AnsiConsole.Ask<string>(T("Email:")).Trim();
        string phone = AnsiConsole.Ask<string>(T("Phone Number:")).Trim();
        string password = AnsiConsole.Ask<string>(T("Password:"));
        string pin = AnsiConsole.Ask<string>(T("Create a 4-digit Payment PIN:")).Trim();

        if (string.IsNullOrWhiteSpace(fullName))
            throw new Exception_Class(T("Full name cannot be empty."));

        if (!email.Contains('@') || !email.Contains('.'))
            throw new Exception_Class(T("Please enter a valid email address."));

        if (email.Equals(AdminEmail, StringComparison.OrdinalIgnoreCase) ||
            doctors.Any(d => d.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            throw new Exception_Class(T("This email is reserved."));

        if (string.IsNullOrWhiteSpace(password))
            throw new Exception_Class(T("Password cannot be empty."));

        if (pin.Length != 4 || !pin.All(char.IsDigit))
            throw new Exception_Class(T("PIN must be exactly 4 digits."));

        List<Patient> users = await LoadUsersAsync();

        if (users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            throw new Exception_Class(T("A user with this email already exists."));

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
            new Panel($"[bold green]{T("Registration completed successfully!")}[/]")
                .BorderColor(Color.Green)
                .RoundedBorder());

        AnsiConsole.MarkupLine($"[grey]{T("Your Patient ID:")} [white]{newId}[/][/]");
    }

    private static async Task LoginAsync(List<Doctor> doctors)
    {
        AnsiConsole.Write(new Rule($"[yellow]{T("Login")}[/]"));

        string email = AnsiConsole.Ask<string>(T("Email:")).Trim();
        string password = AnsiConsole.Ask<string>(T("Password:"));

        if (email.Equals(AdminEmail, StringComparison.OrdinalIgnoreCase) &&
            password == AdminPassword)
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
            throw new Exception_Class(T("Invalid email or password."));

        await HandlePatientMenuAsync(loggedInUser, users, doctors);
    }

    private static void ShowDoctors(List<Doctor> doctors)
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
            new Panel($"[bold yellow]{T("Administrator Access")}[/]")
                .BorderColor(Color.Yellow)
                .RoundedBorder());

        bool running = true;

        while (running)
        {
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]{T("Admin Menu")}[/]")
                    .PageSize(8)
                    .WrapAround()
                    .AddChoices(
                        T("View All Patients"),
                        T("Add Balance to Patient"),
                        T("View All Doctors"),
                        T("Add Balance to Doctor"),
                        T("Logout")));

            try
            {
                List<Patient> patients = await LoadUsersAsync();

                switch (choice)
                {
                    case var c when c == T("View All Patients"):
                        ShowPatients(patients);
                        break;

                    case var c when c == T("Add Balance to Patient"):
                        await AddPatientBalanceAsync(patients);
                        break;

                    case var c when c == T("View All Doctors"):
                        ShowDoctorsDetailed(doctors);
                        break;

                    case var c when c == T("Add Balance to Doctor"):
                        await AddDoctorBalanceAsync(doctors);
                        break;

                    case var c when c == T("Logout"):
                        running = false;
                        ShowSuccess(T("Administrator logged out."));
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
            AnsiConsole.MarkupLine($"[yellow]{T("No registered patients found.")}[/]");
            return;
        }

        string identifier = AnsiConsole.Ask<string>(
            "[cyan]Enter Patient Email or ID:[/]").Trim();

        Patient? targetPatient = int.TryParse(identifier, out int patientId)
            ? patients.FirstOrDefault(p => p.Id == patientId)
            : patients.FirstOrDefault(p =>
                p.Email.Equals(identifier, StringComparison.OrdinalIgnoreCase));

        if (targetPatient is null)
            throw new Exception_Class(T("Patient not found."));

        decimal amount = AnsiConsole.Ask<decimal>(
            $"[cyan]Amount to add to {Markup.Escape(targetPatient.FullName)} ($):[/]");

        if (amount <= 0)
            throw new Exception_Class(T("Amount must be greater than zero."));

        if (!AnsiConsole.Confirm(
            $"Add [green]${amount:F2}[/] to {Markup.Escape(targetPatient.FullName)}?"))
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
                    .Title($"[cyan]{T("Doctor Menu")}[/]")
                    .WrapAround()
                    .AddChoices(
                        T("View Doctor Profile"),
                        T("View Appointments"),
                        T("Logout")));

            switch (choice)
            {
                case var c when c == T("View Doctor Profile"):
                    ShowDoctorProfile(doctor);
                    break;

                case var c when c == T("View Appointments"):
                    await ShowDoctorAppointmentsAsync(doctor);
                    break;

                case var c when c == T("Logout"):
                    running = false;
                    ShowSuccess(T("Doctor logged out."));
                    break;
            }
        }
    }

    private static void ShowDoctorProfile(Doctor doctor)
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
        table.AddRow(T("Account Balance"), $"[green]${doctor.Balance:F2}[/]");

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
            AnsiConsole.MarkupLine($"[yellow]{T("No appointments found.")}[/]");
            return;
        }

        Table table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .Title($"[bold blue]{T("View Appointments")}[/]")
            .Expand();

        table.AddColumn(T("Patient"));
        table.AddColumn(T("Disease"));
        table.AddColumn(T("Duration"));
        table.AddColumn(T("Cost"));
        table.AddColumn(T("Payment"));

        foreach (var item in appointments)
        {
            table.AddRow(
                Markup.Escape(item.Patient.FullName),
                Markup.Escape(item.Appointment.Disease),
                $"{item.Appointment.Hours} hrs",
                $"[green]${item.Appointment.Cost:F2}[/]",
                item.Appointment.IsPaid
                    ? $"[green]{T("Paid")}[/]"
                    : $"[red]{T("Unpaid")}[/]");
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
                    .WrapAround()
                    .AddChoices(
                        T("View Profile"),
                        T("Update Profile"),
                        T("Make an Appointment"),
                        T("View Balance"),
                        T("Pay Bills"),
                        T("View My Appointments"),
                        T("View Appointment History"),
                        T("Logout")));

            try
            {
                switch (choice)
                {
                    case var c when c == T("View Profile"):
                        ShowPatientProfile(loggedInUser);
                        break;

                    case var c when c == T("Update Profile"):
                        await UpdatePatientProfileAsync(loggedInUser, registeredUsers);
                        break;

                    case var c when c == T("Make an Appointment"):
                        await MakeAppointmentAsync(loggedInUser, registeredUsers, doctors);
                        break;

                    case var c when c == T("View Balance"):
                        ShowPatientBalance(loggedInUser);
                        break;

                    case var c when c == T("Pay Bills"):
                        await PayPatientBillsAsync(loggedInUser, registeredUsers, doctors);
                        break;

                    case var c when c == T("View My Appointments"):
                        ShowPatientAppointments(loggedInUser);
                        break;

                    case var c when c == T("View Appointment History"):
                        await ShowAppointmentHistoryAsync(loggedInUser);
                        break;

                    case var c when c == T("Logout"):
                        running = false;
                        ShowSuccess(T("Patient logged out."));
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
            .Title($"[bold green]{T("Patient Profile")}[/]");

        table.AddColumn(T("Field"));
        table.AddColumn(T("Information"));

        table.AddRow(T("Patient ID"), patient.Id.ToString());
        table.AddRow(T("Full Name"), Markup.Escape(patient.FullName));
        table.AddRow(T("Email"), Markup.Escape(patient.Email));
        table.AddRow(T("Phone"), Markup.Escape(patient.PN));
        table.AddRow(T("Disease"), Markup.Escape(
            string.IsNullOrWhiteSpace(patient.Disease) ? T("None recorded") : patient.Disease));
        table.AddRow(T("Balance"), $"[green]${patient.Balance:F2}[/]");
        table.AddRow(T("Pending Bill"),
            patient.PendingBill > 0
                ? $"[red]${patient.PendingBill:F2}[/]"
                : "[green]$0.00[/]");

        AnsiConsole.Write(table);
    }

    private static async Task UpdatePatientProfileAsync(
        Patient patient,
        List<Patient> registeredUsers)
    {
        AnsiConsole.Write(new Rule($"[yellow]{T("Update Profile")}[/]"));

        string newName = AnsiConsole.Ask<string>(T("New Full Name:")).Trim();
        string newEmail = AnsiConsole.Ask<string>(T("New Email:")).Trim();
        string newPhone = AnsiConsole.Ask<string>(T("New Phone Number:")).Trim();

        if (string.IsNullOrWhiteSpace(newName))
            throw new Exception_Class(T("Name cannot be empty."));

        if (!newEmail.Contains('@') || !newEmail.Contains('.'))
            throw new Exception_Class(T("Please enter a valid email."));

        bool emailUsed = registeredUsers.Any(p =>
            p.Id != patient.Id &&
            p.Email.Equals(newEmail, StringComparison.OrdinalIgnoreCase));

        if (emailUsed)
            throw new Exception_Class(T("This email is already in use."));

        if (!AnsiConsole.Confirm(T("Save profile changes?")))
            return;

        patient.FullName = newName;
        patient.Email = newEmail;
        patient.PN = newPhone;

        await SaveUsersAsync(registeredUsers);
        ShowSuccess(T("Profile updated successfully."));
    }

    private static async Task MakeAppointmentAsync(
        Patient patient,
        List<Patient> registeredUsers,
        List<Doctor> doctors)
    {
        ShowDoctors(doctors);

        Doctor chosenDoctor = AnsiConsole.Prompt(
            new SelectionPrompt<Doctor>()
                .Title($"[cyan]{T("Select a doctor:")}[/]")
                .PageSize(8)
                .UseConverter(d => $"{d.FullName} | {d.Specialize} | ${d.ConsultationFee:F2}/hr")
                .AddChoices(doctors));

        string disease = AnsiConsole.Ask<string>(T("Enter your disease or symptoms:")).Trim();

        if (string.IsNullOrWhiteSpace(disease))
            throw new Exception_Class(T("Disease or symptoms cannot be empty."));

        decimal duration = AnsiConsole.Ask<decimal>(T("Consultation duration in hours:"));

        if (duration <= 0)
            throw new Exception_Class(T("Duration must be greater than zero."));

        patient.Disease = disease;
        patient.Consultation_Time = duration;

        decimal price = chosenDoctor.CalculatePrice(patient);

        AnsiConsole.Write(
            new Panel(
                $"[bold]{T("Doctor")}:[/] {Markup.Escape(chosenDoctor.FullName)}\n" +
                $"[bold]{T("Specialty")}:[/] {Markup.Escape(chosenDoctor.Specialize.ToString())}\n" +
                $"[bold]{T("Duration")}[/] {duration} {T("hours")}\n" +
                $"[bold]{T("Estimated Cost:")}[/] [green]${price:F2}[/]")
                .BorderColor(Color.Yellow)
                .RoundedBorder());

        if (!AnsiConsole.Confirm(T("Confirm appointment?")))
            return;

        patient.PendingBill += price;

        patient.Appointments ??= new List<AppointmentInfo>(); // Null-safe
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

        ShowSuccess(T("Appointment confirmed."));
        AnsiConsole.MarkupLine(
            $"[grey]{T("Pending Bill")}: [red]${patient.PendingBill:F2}[/][/]");
    }

    private static void ShowPatientBalance(Patient patient)
    {
        Table table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Yellow)
            .Title($"[bold yellow]{T("Account Balance")}[/]");

        table.AddColumn(T("Account"));
        table.AddColumn(T("Amount"));

        table.AddRow(T("Available Balance"), $"[green]${patient.Balance:F2}[/]");
        table.AddRow(T("Pending Bill"),
            patient.PendingBill > 0
                ? $"[red]${patient.PendingBill:F2}[/]"
                : "[green]$0.00[/]");

        AnsiConsole.Write(table);
    }

    private static async Task PayPatientBillsAsync(
        Patient patient,
        List<Patient> registeredUsers,
        List<Doctor> doctors)
    {
        if (patient.PendingBill <= 0)
        {
            AnsiConsole.MarkupLine($"[green]{T("You have no unpaid bills.")}[/]");
            return;
        }

        patient.Appointments ??= new List<AppointmentInfo>(); // Null-safe

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

        List<Appointment> appointmentHistory = appointmentsToPay.Select(a => new Appointment
        {
            PatientId = patient.Id,
            PatientName = patient.FullName,
            DoctorId = a.DoctorId,
            DoctorName = a.DoctorName,
            Disease = a.Disease,
            Hours = a.Hours,
            Payment = a.Cost,
            IsPaid = true,
            History = DateTime.Now
        }).ToList();

        await SaveAppointmentsAsync(appointmentHistory);

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

        await SavePatientsAsync(registeredUsers);
        await SaveDoctorBalancesAsync(doctors);

        AnsiConsole.Write(
            new Panel(
                $"[bold green]{T("Payment Successful!")}[/]\n" +
                $"[bold]{T("Paid")}:[/] ${amountToPay:F2}\n" +
                $"[bold]{T("Remaining Balance:")}[/] ${patient.Balance:F2}\n" +
                $"[bold]{T("Appointments moved to history:")}[/] {appointmentHistory.Count}")
                .BorderColor(Color.Green)
                .RoundedBorder());
    }

    private static void ShowPatientAppointments(Patient patient)
    {
        if (patient.Appointments is not { Count: > 0 })
        {
            AnsiConsole.MarkupLine($"[yellow]{T("You have no appointments.")}[/]");
            return;
        }

        Table table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .Title($"[bold blue]{T("My Appointments")}[/]")
            .Expand();

        table.AddColumn(T("Doctor"));
        table.AddColumn(T("Disease"));
        table.AddColumn(T("Hours"));
        table.AddColumn(T("Cost"));
        table.AddColumn(T("Status"));

        foreach (AppointmentInfo appointment in patient.Appointments)
        {
            table.AddRow(
                Markup.Escape(appointment.DoctorName),
                Markup.Escape(appointment.Disease),
                $"{appointment.Hours}",
                $"${appointment.Cost:F2}",
                appointment.IsPaid
                    ? $"[green]{T("Paid")}[/]"
                    : $"[red]{T("Unpaid")}[/]");
        }

        AnsiConsole.Write(table);
    }

    private static async Task ShowAppointmentHistoryAsync(Patient patient)
    {
        if (!File.Exists(AppointmentFilePath))
        {
            AnsiConsole.MarkupLine($"[yellow]{T("No appointment history found.")}[/]");
            return;
        }

        string content = await File.ReadAllTextAsync(AppointmentFilePath);

        if (string.IsNullOrWhiteSpace(content))
        {
            AnsiConsole.MarkupLine($"[yellow]{T("Your appointment history is empty.")}[/]");
            return;
        }

        List<Appointment> allAppointments = DeserializeJson<List<Appointment>>(content) ?? [];

        List<Appointment> patientHistory = allAppointments
            .Where(a => a.PatientId == patient.Id)
            .OrderByDescending(a => a.History)
            .ToList();

        if (patientHistory.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]{T("You have no paid appointment history yet.")}[/]");
            return;
        }

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
        table.AddColumn(T("Date & Time"));

        foreach (Appointment appointment in patientHistory)
        {
            table.AddRow(
                Markup.Escape(appointment.DoctorName),
                Markup.Escape(appointment.Disease),
                $"{appointment.Hours:F2}",
                $"[green]${appointment.Payment:F2}[/]",
                appointment.IsPaid
                    ? $"[green]{T("Paid")}[/]"
                    : $"[red]{T("Unpaid")}[/]",
                appointment.History.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        AnsiConsole.Write(table);

        decimal totalPaid = patientHistory
            .Where(a => a.IsPaid)
            .Sum(a => a.Payment);

        AnsiConsole.MarkupLine(
            $"\n[bold]{T("Total paid appointments:")}[/] {patientHistory.Count(a => a.IsPaid)}");

        AnsiConsole.MarkupLine(
            $"[bold]{T("Total amount paid:")}[/] [green]${totalPaid:F2}[/]");
    }

    // New helper methods for robust JSON handling
    private static T? DeserializeJson<T>(string content)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(content, CachedJsonOptions);
        }
        catch (JsonException)
        {
            string repaired = FixInvalidBooleanTokens(content);
            return JsonSerializer.Deserialize<T>(repaired, CachedJsonOptions);
        }
    }

    private static string FixInvalidBooleanTokens(string json)
    {
        // Replace unquoted T/F with true/false
        json = System.Text.RegularExpressions.Regex.Replace(
            json,
            @"(""IsPaid""\s*:\s*)T\b",
            "$1true");

        json = System.Text.RegularExpressions.Regex.Replace(
            json,
            @"(""IsPaid""\s*:\s*)F\b",
            "$1false");

        // Replace quoted "T"/"F" with true/false
        json = System.Text.RegularExpressions.Regex.Replace(
            json,
            @"(""IsPaid""\s*:\s*)""T""",
            "$1true");

        json = System.Text.RegularExpressions.Regex.Replace(
            json,
            @"(""IsPaid""\s*:\s*)""F""",
            "$1false");

        return json;
    }

    private static async Task<List<Patient>> LoadUsersAsync()
    {
        if (!File.Exists(PatientDataFilePath))
            return [];

        string content = await File.ReadAllTextAsync(PatientDataFilePath);

        if (string.IsNullOrWhiteSpace(content))
            return [];

        return DeserializeJson<List<Patient>>(content) ?? [];
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

    private static async Task SaveAppointmentsAsync(List<Appointment> newAppointments)
    {
        List<Appointment> history = [];

        if (File.Exists(AppointmentFilePath))
        {
            string content = await File.ReadAllTextAsync(AppointmentFilePath);

            if (!string.IsNullOrWhiteSpace(content))
                history = DeserializeJson<List<Appointment>>(content) ?? [];
        }

        history.AddRange(newAppointments);

        string? directory = Path.GetDirectoryName(AppointmentFilePath);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        string json = JsonSerializer.Serialize(history, CachedJsonOptions);
        await File.WriteAllTextAsync(AppointmentFilePath, json);
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
            AnsiConsole.MarkupLine($"[yellow]{T("No doctors found.")}[/]");
            return;
        }

        Doctor doctor = AnsiConsole.Prompt(
            new SelectionPrompt<Doctor>()
                .Title($"[cyan]{T("Select a doctor:")}[/]")
                .PageSize(8)
                .WrapAround()
                .UseConverter(d => $"{d.FullName} | {d.Specialize} | Balance: ${d.Balance:F2}")
                .AddChoices(doctors));

        decimal amount = AnsiConsole.Ask<decimal>("[cyan]Amount to add ($):[/]");

        if (amount <= 0)
            throw new Exception_Class(T("Amount must be greater than zero."));

        if (!AnsiConsole.Confirm(
            $"Add [green]${amount:F2}[/] to {Markup.Escape(doctor.FullName)}?"))
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

// Custom converter to accept flexible boolean representations
public class FlexibleBooleanConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.String:
                string? str = reader.GetString();
                if (string.Equals(str, "true", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(str, "t", StringComparison.OrdinalIgnoreCase) ||
                    str == "1")
                    return true;
                if (string.Equals(str, "false", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(str, "f", StringComparison.OrdinalIgnoreCase) ||
                    str == "0")
                    return false;
                throw new JsonException($"Cannot convert string '{str}' to boolean.");
            case JsonTokenType.Number:
                if (reader.TryGetInt32(out int num))
                    return num != 0;
                throw new JsonException("Invalid number for boolean.");
            default:
                throw new JsonException("Unexpected token for boolean.");
        }
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
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
    : Doctor(id, fullName, email, pn, password, specialize, 0, expertise)
{
}
