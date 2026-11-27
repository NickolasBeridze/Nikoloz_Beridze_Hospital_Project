using Hospital_Project.Exceptions;
using Hospital_Project.Helpers;
using Hospital_Project.Models;
using static Hospital_Project.Helpers.Translator;

namespace Hospital_Project.Services
{
    internal static class AuthService
    {
        public const int MaxFailedAttempts = 3;
        private static readonly TimeSpan LockDuration = TimeSpan.FromSeconds(30);

        private static readonly Dictionary<string, int> FailedAttempts = new();
        private static readonly Dictionary<string, DateTime> LockedUntil = new();

        private static string Key(string email) => email.Trim().ToLowerInvariant();

        public static void EnsureNotLocked(string email)
        {
            string key = Key(email);

            if (!LockedUntil.TryGetValue(key, out DateTime until))
                return;

            if (DateTime.Now < until)
            {
                int seconds = (int)Math.Ceiling((until - DateTime.Now).TotalSeconds);
                throw Exception_Class.Account_Locked(seconds);
            }

            LockedUntil.Remove(key);
            FailedAttempts.Remove(key);
        }

        public static Exception_Class RegisterFailure(string email)
        {
            string key = Key(email);

            FailedAttempts[key] = FailedAttempts.GetValueOrDefault(key) + 1;
            int attempts = FailedAttempts[key];

            if (attempts >= MaxFailedAttempts)
            {
                LockedUntil[key] = DateTime.Now.Add(LockDuration);
                FailedAttempts.Remove(key);

                Logger.Log($"Account locked after {MaxFailedAttempts} failed attempts: {key}");
                return Exception_Class.Account_Locked((int)LockDuration.TotalSeconds);
            }

            return Exception_Class.Invalid_Credentials(MaxFailedAttempts - attempts);
        }

        public static void ResetFailures(string email)
        {
            FailedAttempts.Remove(Key(email));
            LockedUntil.Remove(Key(email));
        }

        public static User Login(
            string email,
            string password,
            Admin admin,
            List<Doctor> doctors,
            List<Patient> patients)
        {
            EnsureNotLocked(email);

            List<User> everyone = [admin];
            everyone.AddRange(doctors);
            everyone.AddRange(patients);

            User? found = everyone.FirstOrDefault(u =>
                u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase)
                && u.PasswordMatches(password));

            if (found is null)
                throw RegisterFailure(email);

            ResetFailures(email);
            return found;
        }

        public static async Task ChangePasswordAsync(User user, Func<Task> saveChanges)
        {
            EnsureNotLocked(user.Email);

            string current = ConsoleUI.AskSecret(T("Current Password:"));

            if (!user.PasswordMatches(current))
                throw RegisterFailure(user.Email);

            ResetFailures(user.Email);

            string newPassword = ConsoleUI.AskSecret(T("New Password:"));
            string confirmation = ConsoleUI.AskSecret(T("Confirm New Password:"));

            string oldPassword = user.Password;

            user.ChangePassword(newPassword, confirmation);

            try
            {
                await saveChanges();
            }
            catch
            {
                user.Password = oldPassword;
                throw;
            }

            Logger.Log($"Password changed: {user.GetRole()} {user.Email}");
            ConsoleUI.ShowSuccess(T("Password changed successfully."));
        }
    }
}
