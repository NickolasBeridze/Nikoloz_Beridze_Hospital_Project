using Hospital_Project.Enums;
using System;

namespace Hospital_Project.Models
{
    abstract class Doctor : User
    {
        public int Expertise { get; set; } = 0;
        public Doctor_ENUMS Specialize { get; set; } = Doctor_ENUMS.GENERAL_DOCTOR;
        public decimal ConsultationFee { get; set; } = 0;

        public Doctor(int id, string fullName, string email, string pn, string password, Doctor_ENUMS specialize, decimal consultationFee, int expertise) : base()
        {
            Id = id;
            FullName = fullName;
            Email = email;
            PN = pn;
            Password = password;
            Specialize = specialize;
            Expertise = expertise;

            ConsultationFee = GetBasePayBySpecialty(specialize);
        }

        protected decimal GetBasePayBySpecialty(Doctor_ENUMS specialty) => specialty switch
        {
            Doctor_ENUMS.GENERAL_DOCTOR => 50m,
            Doctor_ENUMS.SURGEON => 200m,
            Doctor_ENUMS.PEDIATRICIAN => 75m,
            Doctor_ENUMS.DERMATOLOGIST => 110m,
            Doctor_ENUMS.CARDIOLOGIST => 150m,
            Doctor_ENUMS.NEUROLOGIST => 140m,
            Doctor_ENUMS.DENTIST => 90m,
            _ => throw new ArgumentOutOfRangeException(nameof(specialty), "Unknown specialty")
        };

        public decimal CalculatePrice(Patient patient, decimal tax = 0.18m)
        {
            decimal Base_Pay = patient.Consultation_Time * ConsultationFee;
            return Base_Pay + (Base_Pay * tax);
        }
    }
}