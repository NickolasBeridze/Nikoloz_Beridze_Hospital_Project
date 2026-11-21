namespace Hospital_Project.Models;

public class Appointment
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = "";

    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = "";

    public string Disease { get; set; } = "";
    public decimal Hours { get; set; }

    public decimal Payment { get; set; }
    public bool IsPaid { get; set; }

    public DateTime History { get; set; }
}
