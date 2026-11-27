namespace Hospital_Project.Models
{
    internal class Admin : User
    {
        public const string DefaultEmail = "admin@hospital.com";
        public const string DefaultPassword = "admin123";

        public Admin(string email, string password)
        {
            Id = 0;
            FullName = "Administrator";
            Email = email;
            Password = password;
        }

        public override string GetRole() => "Administrator";
        public override string GetSummary() => $"{FullName} ({Email})";
    }
}
