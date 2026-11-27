using System.Text;

namespace Hospital_Project.Services
{
    internal static class FileService
    {
        public static readonly string DataFolder = Path.Combine(AppContext.BaseDirectory, "Text_Files");

        public static string PatientDataPath => Path.Combine(DataFolder, "Patient_Data.txt");
        public static string AppointmentPath => Path.Combine(DataFolder, "Appointment.txt");
        public static string DoctorBalancePath => Path.Combine(DataFolder, "Doctor_Balances.txt");
        public static string StaffPasswordPath => Path.Combine(DataFolder, "Staff_Passwords.txt");
        public static string LogPath => Path.Combine(DataFolder, "Activity_Log.txt");
        public static string BackupFolder => Path.Combine(AppContext.BaseDirectory, "Backups");

        private static readonly UTF8Encoding Utf8NoBom = new(false);

        public static async Task<string> ReadAllTextAsync(string path)
        {
            if (!File.Exists(path))
                return string.Empty;

            using StreamReader reader = new(path, Utf8NoBom);
            return await reader.ReadToEndAsync();
        }

        public static async Task WriteAllTextSafeAsync(string path, string content)
        {
            string? directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            string tempPath = path + ".tmp";

            await using (StreamWriter writer = new(tempPath, false, Utf8NoBom))
            {
                await writer.WriteAsync(content);
            }

            File.Move(tempPath, path, true);
        }

        public static void PreserveCorruptCopy(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Copy(path, $"{path}.corrupt_{DateTime.Now:yyyyMMdd_HHmmss}", true);
            }
            catch (IOException)
            {
            }
        }
    }
}
