using System.Text.Json;
using Hospital_Project.Enums;
using Hospital_Project.Helpers;
using Hospital_Project.Models;
using static Hospital_Project.Helpers.Translator;

namespace Hospital_Project.Services
{
    internal static class DataService
    {
        public static async Task<List<Patient>> LoadPatientsAsync()
        {
            List<Patient?> loaded = await LoadListAsync<Patient>(FileService.PatientDataPath);
            return CleanPatients(loaded);
        }

        public static async Task SavePatientsAsync(List<Patient> patients)
        {
            await FileService.WriteAllTextSafeAsync(FileService.PatientDataPath, JsonHelper.Serialize(patients));
        }

        private static List<Patient> CleanPatients(List<Patient?> loaded)
        {
            List<Patient> clean = [];
            int problems = 0;

            foreach (Patient? patient in loaded)
            {
                if (patient is null
                    || patient.Id <= 0
                    || string.IsNullOrWhiteSpace(patient.Email)
                    || string.IsNullOrWhiteSpace(patient.Password)
                    || clean.Any(c => c.Id == patient.Id
                                   || c.Email.Equals(patient.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    problems++;
                    continue;
                }

                patient.Appointments ??= [];
                patient.Appointments.RemoveAll(a => a is null);

                if (!Enum.IsDefined(patient.BloodType))
                {
                    patient.BloodType = default;
                    problems++;
                }

                if (patient.Balance < 0 || patient.PendingBill < 0)
                {
                    patient.Balance = Math.Max(0, patient.Balance);
                    patient.PendingBill = Math.Max(0, patient.PendingBill);
                    problems++;
                }

                foreach (AppointmentInfo appointment in patient.Appointments)
                {
                    if (!Enum.IsDefined(appointment.Status))
                    {
                        appointment.Status = AppointmentStatus.Scheduled;
                        problems++;
                    }
                }

                clean.Add(patient);
            }

            if (problems > 0)
            {
                FileService.PreserveCorruptCopy(FileService.PatientDataPath);
                ConsoleUI.ShowWarning($"{T("Some patient records were invalid and were skipped or fixed:")} {problems}");
            }

            return clean;
        }

        public static async Task<List<Appointment>> LoadHistoryAsync()
        {
            List<Appointment?> loaded = await LoadListAsync<Appointment>(FileService.AppointmentPath);
            return loaded.Where(a => a is not null).Select(a => a!).ToList();
        }

        public static async Task AppendHistoryAsync(List<Appointment> newAppointments)
        {
            List<Appointment> history = await LoadHistoryAsync();
            history.AddRange(newAppointments);

            await FileService.WriteAllTextSafeAsync(FileService.AppointmentPath, JsonHelper.Serialize(history));
        }

        public static async Task LoadDoctorBalancesAsync(List<Doctor> doctors)
        {
            string content = await FileService.ReadAllTextAsync(FileService.DoctorBalancePath);

            if (string.IsNullOrWhiteSpace(content))
                return;

            Dictionary<int, decimal> balances;

            try
            {
                balances = JsonSerializer.Deserialize<Dictionary<int, decimal>>(content) ?? new Dictionary<int, decimal>();
            }
            catch (JsonException)
            {
                FileService.PreserveCorruptCopy(FileService.DoctorBalancePath);
                ConsoleUI.ShowWarning(T("A data file was damaged. A copy was saved and the program continues with empty data."));
                return;
            }

            foreach (Doctor doctor in doctors)
            {
                if (balances.TryGetValue(doctor.Id, out decimal balance) && balance >= 0)
                    doctor.Balance = balance;
            }
        }

        public static async Task SaveDoctorBalancesAsync(List<Doctor> doctors)
        {
            Dictionary<int, decimal> balances = doctors.ToDictionary(
                doctor => doctor.Id,
                doctor => doctor.Balance);

            await FileService.WriteAllTextSafeAsync(FileService.DoctorBalancePath, JsonHelper.Serialize(balances));
        }

        public static async Task LoadStaffPasswordsAsync(Admin admin, List<Doctor> doctors)
        {
            string content = await FileService.ReadAllTextAsync(FileService.StaffPasswordPath);

            if (string.IsNullOrWhiteSpace(content))
                return;

            Dictionary<string, string> saved;

            try
            {
                saved = JsonSerializer.Deserialize<Dictionary<string, string>>(content) ?? new Dictionary<string, string>();
            }
            catch (JsonException)
            {
                FileService.PreserveCorruptCopy(FileService.StaffPasswordPath);
                ConsoleUI.ShowWarning(T("A data file was damaged. A copy was saved and the program continues with empty data."));
                return;
            }

            List<User> staff = [admin];
            staff.AddRange(doctors);

            foreach (User user in staff)
            {
                if (saved.TryGetValue(user.Email.ToLowerInvariant(), out string? password)
                    && !string.IsNullOrWhiteSpace(password)
                    && password.Length >= Validator.MinPasswordLength)
                {
                    user.Password = password;
                }
            }
        }

        public static async Task SaveStaffPasswordsAsync(Admin admin, List<Doctor> doctors)
        {
            List<User> staff = [admin];
            staff.AddRange(doctors);

            Dictionary<string, string> passwords = staff.ToDictionary(
                user => user.Email.ToLowerInvariant(),
                user => user.Password);

            await FileService.WriteAllTextSafeAsync(FileService.StaffPasswordPath, JsonHelper.Serialize(passwords));
        }

        public static async Task<string> BackupAsync()
        {
            string folder = Path.Combine(FileService.BackupFolder, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(folder);

            string[] files =
            [
                FileService.PatientDataPath,
                FileService.AppointmentPath,
                FileService.DoctorBalancePath,
                FileService.StaffPasswordPath
            ];

            foreach (string file in files)
            {
                if (!File.Exists(file))
                    continue;

                string text = await FileService.ReadAllTextAsync(file);
                await FileService.WriteAllTextSafeAsync(Path.Combine(folder, Path.GetFileName(file)), text);
            }

            return folder;
        }

        private static async Task<List<TItem?>> LoadListAsync<TItem>(string path)
        {
            string content = await FileService.ReadAllTextAsync(path);

            if (string.IsNullOrWhiteSpace(content))
                return [];

            try
            {
                return JsonHelper.Deserialize<List<TItem?>>(content) ?? [];
            }
            catch (JsonException)
            {
                FileService.PreserveCorruptCopy(path);
                ConsoleUI.ShowWarning(T("A data file was damaged. A copy was saved and the program continues with empty data."));
                return [];
            }
        }
    }
}
