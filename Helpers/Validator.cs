using Hospital_Project.Exceptions;
using static Hospital_Project.Helpers.Translator;

namespace Hospital_Project.Helpers
{
    internal static class Validator
    {
        public const int MinPasswordLength = 6;
        public const decimal MaxMoneyAmount = 1_000_000m;
        public const decimal MinHours = 0.25m;
        public const decimal MaxHours = 8m;

        public static void ValidateNewPassword(string? newPassword, string currentPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw Exception_Class.Password_Empty();

            if (newPassword.Length < MinPasswordLength)
                throw Exception_Class.Invalid_Quantity_Of_Characters();

            if (newPassword == currentPassword)
                throw Exception_Class.Password_Same_As_Current();
        }

        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Contains(' '))
                return false;

            int at = email.IndexOf('@');

            if (at <= 0 || at != email.LastIndexOf('@'))
                return false;

            int dot = email.LastIndexOf('.');
            return dot > at + 1 && dot < email.Length - 1;
        }

        public static bool IsValidPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            bool onlyAllowedChars = phone.All(c => char.IsDigit(c) || "+-() ".Contains(c));
            int digits = phone.Count(char.IsDigit);

            return onlyAllowedChars && digits >= 5 && digits <= 15;
        }

        public static bool IsValidPin(string? pin)
            => pin is not null && pin.Length == 4 && pin.All(char.IsDigit);

        public static string? CheckMoney(decimal amount)
        {
            if (amount <= 0)
                return T("Amount must be greater than zero.");

            if (amount > MaxMoneyAmount)
                return T("Amount is too large.");

            if (decimal.Round(amount, 2) != amount)
                return T("Amount can have at most 2 decimal places.");

            return null;
        }

        public static void ValidateMoney(decimal amount)
        {
            string? error = CheckMoney(amount);

            if (error is not null)
                throw Exception_Class.Invalid_Amount(error);
        }

        public static string? CheckHours(decimal hours)
        {
            if (hours < MinHours || hours > MaxHours)
                return $"{T("Duration must be between")} {MinHours} {T("and")} {MaxHours} {T("hours.")}";

            if (decimal.Round(hours, 2) != hours)
                return T("Amount can have at most 2 decimal places.");

            return null;
        }
    }
}
