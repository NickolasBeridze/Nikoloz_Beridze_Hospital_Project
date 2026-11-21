using Hospital_Project.Exceptions;

namespace Hospital_Project.Models
{
    internal class User
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
    }
}