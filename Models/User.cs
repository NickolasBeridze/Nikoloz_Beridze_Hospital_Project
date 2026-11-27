using Hospital_Project.Exceptions;
using Hospital_Project.Helpers;

namespace Hospital_Project.Models
{
    internal abstract class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PN { get; set; } = string.Empty;

        public decimal Balance { get; set; }
        public string Password { get; set; } = string.Empty;

        public User() { }

        public User(string fullName, string email, string pn, string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                throw Exception_Class.Invalid_Quantity_Of_Characters();
            }

            FullName = fullName;
            Email = email;
            PN = pn;
            Password = password;
        }

        public abstract string GetRole();
        public abstract string GetSummary();

        public bool PasswordMatches(string input) => Password == input;

        public void ChangePassword(string newPassword, string confirmPassword)
        {
            Validator.ValidateNewPassword(newPassword, Password);

            if (newPassword != confirmPassword)
                throw Exception_Class.Passwords_Do_Not_Match();

            Password = newPassword;
        }
    }
}
