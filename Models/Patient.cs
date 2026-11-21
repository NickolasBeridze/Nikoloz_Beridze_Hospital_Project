using System.Collections.Generic;
using Hospital_Project.Enums;

namespace Hospital_Project.Models
{
    internal class Patient : User
    {
        public string Disease { get; set; } = string.Empty;
        public string Insurance { get; set; } = string.Empty;
        public Blood_Type_ENUMS BloodType { get; set; }
        public string Allergies { get; set; } = string.Empty;
        public decimal Consultation_Time { get; set; } = 0;
        public decimal Balance { get; set; } = 0;
        public decimal PendingBill { get; set; } = 0;
        public string Pin { get; set; } = string.Empty;

        
        public List<AppointmentInfo> Appointments { get; set; } = [];
    }

    internal class AppointmentInfo
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Disease { get; set; } = string.Empty;
        public decimal Hours { get; set; }
        public decimal Cost { get; set; }
        public bool IsPaid { get; set; }
    }
}