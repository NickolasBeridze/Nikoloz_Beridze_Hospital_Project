using Hospital_Project.Enums;
using Hospital_Project.Models;

namespace Hospital_Project.Helpers
{
    internal static class Filters
    {
        public static Predicate<Patient> PatientById(int id) => p => p.Id == id;

        public static Predicate<Patient> PatientByName(string text)
            => p => p.FullName.Contains(text, StringComparison.OrdinalIgnoreCase);

        public static Predicate<Patient> PatientByBloodType(Blood_Type_ENUMS type)
            => p => p.BloodType == type;

        public static Predicate<Doctor> DoctorBySpecialty(Doctor_ENUMS specialty)
            => d => d.Specialize == specialty;

        public static List<TItem> Find<TItem>(List<TItem> source, Predicate<TItem> match)
            => source.FindAll(match);

        public static decimal SumBy<TItem>(IEnumerable<TItem> items, Func<TItem, decimal> selector)
            => items.Sum(selector);

        public static List<string> FormatAll<TItem>(IEnumerable<TItem> items, Func<TItem, string> formatter)
            => items.Select(formatter).ToList();

        public static void ForEachItem<TItem>(IEnumerable<TItem> items, Action<TItem> action)
        {
            foreach (TItem item in items)
                action(item);
        }
    }
}
