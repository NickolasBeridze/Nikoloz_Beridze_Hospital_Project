using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hospital_Project
{
    internal class Program 

    {

        private static readonly string FilePath = @"C:\Users\User\source\repos\Hospital_Project\Text_Files\User_Data.txt";
        private static readonly JsonSerializerOptions CachedJsonOptions = new() { WriteIndented = true };

        private const string AdminEmail = "admin@hospital.com";
        private const string AdminPassword = "admin123";

        static async Task Main()
        {
            await Greet();
            await Config();
        }


        public static async Task Greet()
        {
            Console.WriteLine("====================================");
            Console.WriteLine("Welcome to the Hospital Management System!");
            Console.WriteLine("====================================");
            await Task.CompletedTask;
        }

        public static async Task Config()
        {
            bool running = true;

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

            while (running)
            {
                Console.WriteLine("\nWhat service would you like to access?");
                await Task.Delay(500);
                Console.WriteLine("1. Patient Registration");
                Console.WriteLine("2. Login (Patient / Doctor / Admin)");
                Console.WriteLine("3. Exit");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Please enter a valid number (1-3).");
                    continue;
                }

                try
                {
                    switch (choice)
                    {
                        case 1:
                            Console.WriteLine("Enter your details for registration:");
                            await Task.Delay(500);

                            Console.Write("Full Name: ");
                            string input_fullName = Console.ReadLine() ?? string.Empty;

                            Console.Write("Email: ");
                            string input_email = Console.ReadLine() ?? string.Empty;

                            if (input_email.Equals(AdminEmail, StringComparison.OrdinalIgnoreCase) ||
                                doctors.Any(d => d.Email.Equals(input_email, StringComparison.OrdinalIgnoreCase)))
                            {
                                throw new Exception_Class("This email is reserved.");
                            }

                            Console.Write("Phone Number: ");
                            string input_pn = Console.ReadLine() ?? string.Empty;

                            Console.Write("Password: ");
                            string input_password = Console.ReadLine() ?? string.Empty;

                            Console.Write("Create a 4-digit Payment PIN: ");
                            string input_pin = Console.ReadLine()?.Trim() ?? string.Empty;

                            if (input_pin.Length != 4 || !input_pin.All(char.IsDigit))
                            {
                                throw new Exception_Class("PIN must be exactly 4 digits.");
                            }

                            List<Patient> users = await LoadUsersAsync();

                            if (users.Any(u => u.Email.Equals(input_email, StringComparison.OrdinalIgnoreCase)))
                            {
                                throw new Exception_Class("A user with this email already exists.");
                            }

                            var user = new Patient
                            {
                                Id = users.Count + 1,
                                FullName = input_fullName,
                                Email = input_email,
                                PN = input_pn,
                                Password = input_password,
                                Pin = input_pin
                            };

                            users.Add(user);
                            await SaveUsersAsync(users);

                            Console.WriteLine("Registration successful!");
                            break;

                        case 2:
                            Console.Write("Email: ");
                            string loginEmail = Console.ReadLine()?.Trim() ?? string.Empty;

                            Console.Write("Password: ");
                            string loginPassword = Console.ReadLine() ?? string.Empty;

                            if (loginEmail.Equals(AdminEmail, StringComparison.OrdinalIgnoreCase) && loginPassword == AdminPassword)
                            {
                                await HandleAdminMenuAsync();
                                break;
                            }

                            Doctor? loggedInDoctor = doctors.FirstOrDefault(d =>
                                d.Email.Equals(loginEmail, StringComparison.OrdinalIgnoreCase) && d.Password == loginPassword);

                            if (loggedInDoctor is not null)
                            {
                                await HandleDoctorMenuAsync(loggedInDoctor);
                                break;
                            }

                            List<Patient> registeredUsers = await LoadUsersAsync();
                            Patient? loggedInUser = registeredUsers.FirstOrDefault(u =>
                                u.Email.Equals(loginEmail, StringComparison.OrdinalIgnoreCase) && u.Password == loginPassword);

                            if (loggedInUser is null)
                            {
                                throw new Exception_Class("Invalid email or password.");
                            }

                            await HandlePatientMenuAsync(loggedInUser, registeredUsers, doctors);
                            break;

                        case 3:
                            running = false;
                            Console.WriteLine("Exiting system. Goodbye!");
                            break;

                        default:
                            Console.WriteLine("Invalid choice.");
                            break;
                    }
                }
                catch (Exception_Class ex)
                {
                    Console.WriteLine($"\nError: {ex.Message}");
                }
            }
        }

        private static async Task HandleAdminMenuAsync()
        {
            Console.WriteLine("\nWelcome, System Administrator!");
            bool adminLoggedIn = true;

            while (adminLoggedIn)
            {
                await Task.Delay(300);
                Console.WriteLine("\n--- Admin Menu ---");
                Console.WriteLine("1. View All Patients");
                Console.WriteLine("2. Add Balance to Patient");
                Console.WriteLine("3. Logout");

                if (!int.TryParse(Console.ReadLine(), out int adminChoice))
                {
                    Console.WriteLine("Please enter a valid number (1-3).");
                    continue;
                }

                List<Patient> patients = await LoadUsersAsync();

                switch (adminChoice)
                {
                    case 1:
                        if (patients.Count == 0)
                        {
                            Console.WriteLine("No registered patients found.");
                            break;
                        }

                        Console.WriteLine("\nRegistered Patients:");
                        foreach (var p in patients)
                        {
                            Console.WriteLine($"ID: {p.Id} | {p.FullName} ({p.Email}) | Balance: ${p.Balance:F2} | Pending Bill: ${p.PendingBill:F2}");
                        }
                        break;

                    case 2:
                        if (patients.Count == 0)
                        {
                            Console.WriteLine("No registered patients found.");
                            break;
                        }

                        Console.Write("Enter Patient Email or ID: ");
                        string identifier = Console.ReadLine()?.Trim() ?? string.Empty;

                        Patient? targetPatient = int.TryParse(identifier, out int patientId)
                            ? patients.FirstOrDefault(p => p.Id == patientId)
                            : patients.FirstOrDefault(p => p.Email.Equals(identifier, StringComparison.OrdinalIgnoreCase));

                        if (targetPatient is null)
                        {
                            throw new Exception_Class("Patient not found.");
                        }

                        Console.Write($"Enter amount to add to {targetPatient.FullName}'s balance: $");
                        if (!decimal.TryParse(Console.ReadLine(), out decimal amount) || amount <= 0)
                        {
                            throw new Exception_Class("Please enter a valid positive amount.");
                        }

                        targetPatient.Balance += amount;
                        await SaveUsersAsync(patients);
                        Console.WriteLine($"Successfully added ${amount:F2} to {targetPatient.FullName}. New Balance: ${targetPatient.Balance:F2}");
                        break;

                    case 3:
                        Console.WriteLine("Logging out from Admin...");
                        adminLoggedIn = false;
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        private static async Task HandleDoctorMenuAsync(Doctor doctor)
        {
            Console.WriteLine($"\nWelcome, {doctor.FullName} ({doctor.Specialize})!");
            bool doctorLoggedIn = true;

            while (doctorLoggedIn)
            {
                await Task.Delay(300);
                Console.WriteLine("\n--- Doctor Menu ---");
                Console.WriteLine("1. View Doctor Profile");
                Console.WriteLine("2. View My Appointments & Patient Diseases");
                Console.WriteLine("3. Logout");

                if (!int.TryParse(Console.ReadLine(), out int docChoice))
                {
                    Console.WriteLine("Please enter a valid number (1-3).");
                    continue;
                }

                switch (docChoice)
                {
                    case 1:
                        Console.WriteLine("\nDoctor Profile:");
                        Console.WriteLine($"Name: {doctor.FullName}");
                        Console.WriteLine($"Specialty: {doctor.Specialize}");
                        Console.WriteLine($"Expertise: {doctor.Expertise} years");
                        Console.WriteLine($"Consultation Rate: ${doctor.ConsultationFee}/hr");
                        break;

                    case 2:
                        List<Patient> patients = await LoadUsersAsync();
                        var myAppointments = patients
                            .SelectMany(p => p.Appointments
                                .Where(a => a.DoctorId == doctor.Id)
                                .Select(a => new { Patient = p, Appointment = a }))
                            .ToList();

                        if (myAppointments.Count == 0)
                        {
                            Console.WriteLine("You have no scheduled appointments yet.");
                            break;
                        }

                        Console.WriteLine("\nYour Appointments:");
                        foreach (var item in myAppointments)
                        {
                            string status = item.Appointment.IsPaid ? "Paid" : "Unpaid";
                            Console.WriteLine($"Patient: {item.Patient.FullName} | Disease: {item.Appointment.Disease} | Duration: {item.Appointment.Hours} hrs | Fee: ${item.Appointment.Cost:F2} ({status})");
                        }
                        break;

                    case 3:
                        Console.WriteLine("Logging out...");
                        doctorLoggedIn = false;
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        private static async Task HandlePatientMenuAsync(Patient loggedInUser, List<Patient> registeredUsers, List<Doctor> doctors)
        {
            Console.WriteLine($"\nWelcome back, {loggedInUser.FullName}!");
            bool loggedIn = true;

            while (loggedIn)
            {
                await Task.Delay(500);
                Console.WriteLine("\nWhat would you like to do?");
                Console.WriteLine("1. View Profile");
                Console.WriteLine("2. Update Profile");
                Console.WriteLine("3. Make an Appointment");
                Console.WriteLine("4. View Balance");
                Console.WriteLine("5. Pay Bills");
                Console.WriteLine("6. Logout");

                if (!int.TryParse(Console.ReadLine(), out int Choice_2))
                {
                    Console.WriteLine("Please enter a valid number (1-6).");
                    continue;
                }

                switch (Choice_2)
                {
                    case 1:
                        Console.WriteLine("\nProfile Information:");
                        Console.WriteLine($"Name: {loggedInUser.FullName}");
                        Console.WriteLine($"Email: {loggedInUser.Email}");
                        Console.WriteLine($"Phone Number: {loggedInUser.PN}");
                        Console.WriteLine($"Last Recorded Disease: {(string.IsNullOrWhiteSpace(loggedInUser.Disease) ? "None" : loggedInUser.Disease)}");
                        Console.WriteLine($"Balance: ${loggedInUser.Balance:F2}");
                        Console.WriteLine($"Pending Bill: ${loggedInUser.PendingBill:F2}");
                        break;

                    case 2:
                        Console.Write("New Full Name: ");
                        string newFullName = Console.ReadLine() ?? string.Empty;
                        Console.Write("New Email: ");
                        string newEmail = Console.ReadLine() ?? string.Empty;
                        Console.Write("New Phone Number: ");
                        string newPN = Console.ReadLine() ?? string.Empty;

                        loggedInUser.FullName = newFullName;
                        loggedInUser.Email = newEmail;
                        loggedInUser.PN = newPN;

                        await SaveUsersAsync(registeredUsers);
                        Console.WriteLine("Profile updated successfully!");
                        break;

                    case 3:
                        Console.WriteLine("\nAvailable Doctors:");
                        for (int i = 0; i < doctors.Count; i++)
                        {
                            Console.WriteLine($"{i + 1}. {doctors[i].FullName} - {doctors[i].Specialize} (${doctors[i].ConsultationFee}/hr)");
                        }

                        Console.Write("Select a doctor (1-7): ");
                        if (!int.TryParse(Console.ReadLine(), out int docIndex) || docIndex < 1 || docIndex > doctors.Count)
                        {
                            throw new Exception_Class("Invalid doctor selection.");
                        }

                        Doctor chosenDoctor = doctors[docIndex - 1];

                        Console.Write("Enter your disease / symptoms: ");
                        string diseaseInput = Console.ReadLine()?.Trim() ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(diseaseInput))
                        {
                            throw new Exception_Class("Disease cannot be empty.");
                        }

                        Console.Write("Enter consultation duration in hours: ");
                        if (!decimal.TryParse(Console.ReadLine(), out decimal duration) || duration <= 0)
                        {
                            throw new Exception_Class("Please enter a valid number of hours.");
                        }

                        loggedInUser.Disease = diseaseInput;
                        loggedInUser.Consultation_Time = duration;
                        decimal price = chosenDoctor.CalculatePrice(loggedInUser);
                        loggedInUser.PendingBill += price;

                        loggedInUser.Appointments.Add(new AppointmentInfo
                        {
                            DoctorId = chosenDoctor.Id,
                            DoctorName = chosenDoctor.FullName,
                            Disease = diseaseInput,
                            Hours = duration,
                            Cost = price,
                            IsPaid = false
                        });

                        await SaveUsersAsync(registeredUsers);
                        Console.WriteLine($"Appointment confirmed with {chosenDoctor.FullName} ({chosenDoctor.Specialize}) for '{diseaseInput}'.");
                        Console.WriteLine($"Consultation Fee (with 18% tax): ${price:F2}");
                        Console.WriteLine($"Total Pending Bill: ${loggedInUser.PendingBill:F2}");
                        break;

                    case 4:
                        Console.WriteLine($"\nCurrent Balance: ${loggedInUser.Balance:F2}");
                        Console.WriteLine($"Pending Bill: ${loggedInUser.PendingBill:F2}");
                        break;

                    case 5:
                        if (loggedInUser.PendingBill <= 0)
                        {
                            Console.WriteLine("You have no unpaid bills.");
                            break;
                        }

                        Console.WriteLine($"\nTotal Amount Due: ${loggedInUser.PendingBill:F2}");
                        Console.WriteLine($"Your Current Balance: ${loggedInUser.Balance:F2}");

                        if (loggedInUser.Balance < loggedInUser.PendingBill)
                        {
                            throw new Exception_Class($"Insufficient balance. Ask an Admin to add at least ${(loggedInUser.PendingBill - loggedInUser.Balance):F2}.");
                        }

                        Console.Write("Enter your 4-digit PIN to confirm payment: ");
                        string enteredPin = Console.ReadLine()?.Trim() ?? string.Empty;

                        if (enteredPin != loggedInUser.Pin)
                        {
                            throw new Exception_Class("Incorrect PIN. Payment cancelled.");
                        }

                        loggedInUser.Balance -= loggedInUser.PendingBill;
                        loggedInUser.PendingBill = 0;
                        loggedInUser.Consultation_Time = 0;

                        foreach (var app in loggedInUser.Appointments)
                        {
                            app.IsPaid = true;
                        }

                        await SaveUsersAsync(registeredUsers);
                        Console.WriteLine($"Payment completed! Remaining Balance: ${loggedInUser.Balance:F2}");
                        break;

                    case 6:
                        Console.WriteLine("Logging out...");
                        loggedIn = false;
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        private static async Task<List<Patient>> LoadUsersAsync()
        {
            if (!File.Exists(FilePath))
            {
                return [];
            }

            string json = await File.ReadAllTextAsync(FilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            return JsonSerializer.Deserialize<List<Patient>>(json) ?? [];
        }

        private static async Task SaveUsersAsync(List<Patient> users)
        {
            string? directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(users, CachedJsonOptions);
            await File.WriteAllTextAsync(FilePath, json);
        }
    }

    internal class StaffDoctor : Doctor
    {
        public StaffDoctor(int id, string fullName, string email, string pn, string password, Doctor_ENUMS specialize, int expertise)
            : base(id, fullName, email, pn, password, specialize, 0, expertise)
        {
        }
    }
}