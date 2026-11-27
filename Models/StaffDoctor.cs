using Hospital_Project.Enums;

namespace Hospital_Project.Models
{
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
}
