using Hospital_Project.Enums;
using Hospital_Project.Exceptions;
using Hospital_Project.Helpers;
using Hospital_Project.Models;
using static Hospital_Project.Helpers.Translator;

namespace Hospital_Project.Services
{
    internal record AppointmentRow(Patient Patient, AppointmentInfo Appointment);

    internal static class AppointmentService
    {
        private static readonly Dictionary<AppointmentStatus, List<AppointmentStatus>> AllowedTransitions = new()
        {
            [AppointmentStatus.Scheduled] = [AppointmentStatus.Confirmed, AppointmentStatus.Cancelled],
            [AppointmentStatus.Confirmed] = [AppointmentStatus.Completed, AppointmentStatus.Cancelled],
            [AppointmentStatus.Completed] = [],
            [AppointmentStatus.Cancelled] = []
        };

        public static List<AppointmentStatus> GetAllowedNext(AppointmentStatus current)
            => AllowedTransitions.TryGetValue(current, out List<AppointmentStatus>? next)
                ? next
                : new List<AppointmentStatus>();

        public static void ChangeStatus(AppointmentInfo appointment, AppointmentStatus newStatus)
        {
            if (!GetAllowedNext(appointment.Status).Contains(newStatus))
                throw Exception_Class.Invalid_Status_Transition(appointment.Status, newStatus);

            appointment.Status = newStatus;
        }

        public static string? CheckDate(DateTime date)
        {
            if (date <= DateTime.Now)
                return T("The appointment date must be in the future.");

            if (date > DateTime.Now.AddYears(1))
                return T("The appointment cannot be more than one year ahead.");

            return null;
        }

        public static void ValidateDate(DateTime date)
        {
            string? error = CheckDate(date);

            if (error is not null)
                throw Exception_Class.Invalid_Date(error);
        }

        public static string FormatTime(DateTime time)
            => time == default ? "-" : time.ToString(ConsoleUI.DateFormat);

        private static bool Overlaps(DateTime startA, decimal hoursA, DateTime startB, decimal hoursB)
        {
            DateTime endA = startA.AddHours((double)hoursA);
            DateTime endB = startB.AddHours((double)hoursB);

            return startA < endB && startB < endA;
        }

        public static void EnsureNoConflict(
            List<Patient> patients,
            List<Appointment> history,
            int doctorId,
            int patientId,
            DateTime start,
            decimal hours,
            AppointmentInfo? ignore = null)
        {
            bool activeConflict = patients.Any(p => p.Appointments.Any(a =>
                a != ignore
                && a.Status != AppointmentStatus.Cancelled
                && a.ScheduledAt != default
                && (a.DoctorId == doctorId || p.Id == patientId)
                && Overlaps(a.ScheduledAt, a.Hours, start, hours)));

            bool historyConflict = history.Any(h =>
                h.Status != AppointmentStatus.Cancelled
                && h.ScheduledAt != default
                && (h.DoctorId == doctorId || h.PatientId == patientId)
                && Overlaps(h.ScheduledAt, h.Hours, start, hours));

            if (activeConflict || historyConflict)
                throw Exception_Class.Scheduling_Conflict(
                    T("This time overlaps with another appointment of the doctor or the patient."));
        }

        public static async Task CancelAsync(Patient owner, AppointmentInfo appointment, List<Patient> allPatients)
        {
            ChangeStatus(appointment, AppointmentStatus.Cancelled);

            owner.PendingBill = Math.Max(0, owner.PendingBill - appointment.Cost);
            owner.Appointments.Remove(appointment);

            Appointment record = ToHistory(owner, appointment, 0m, false);
            await DataService.AppendHistoryAsync([record]);
            await DataService.SavePatientsAsync(allPatients);

            Logger.Log($"Appointment cancelled: patient {owner.Id}, doctor {appointment.DoctorId}");
        }

        public static Appointment ToHistory(Patient patient, AppointmentInfo a, decimal payment, bool isPaid)
        {
            return new Appointment
            {
                PatientId = patient.Id,
                PatientName = patient.FullName,
                DoctorId = a.DoctorId,
                DoctorName = a.DoctorName,
                Disease = a.Disease,
                Hours = a.Hours,
                Payment = payment,
                IsPaid = isPaid,
                History = DateTime.Now,
                ScheduledAt = a.ScheduledAt,
                Status = a.Status
            };
        }

        public static List<AppointmentRow> GetRows(List<Patient> patients, Predicate<AppointmentInfo> match)
        {
            return patients
                .SelectMany(p => p.Appointments
                    .Where(a => match(a))
                    .Select(a => new AppointmentRow(p, a)))
                .OrderBy(r => r.Appointment.ScheduledAt)
                .ToList();
        }
    }
}
