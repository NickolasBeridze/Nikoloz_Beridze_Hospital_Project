using Hospital_Project.Enums;

namespace Hospital_Project.Helpers
{
    internal static class BloodTypeHelper
    {
        private static readonly Dictionary<Blood_Type_ENUMS, List<Blood_Type_ENUMS>> CanDonateToMap = new()
        {
            [Blood_Type_ENUMS.ONegative] =
            [
                Blood_Type_ENUMS.ONegative, Blood_Type_ENUMS.OPositive,
                Blood_Type_ENUMS.ANegative, Blood_Type_ENUMS.APositive,
                Blood_Type_ENUMS.BNegative, Blood_Type_ENUMS.BPositive,
                Blood_Type_ENUMS.ABNegative, Blood_Type_ENUMS.ABPositive
            ],
            [Blood_Type_ENUMS.OPositive] =
            [
                Blood_Type_ENUMS.OPositive, Blood_Type_ENUMS.APositive,
                Blood_Type_ENUMS.BPositive, Blood_Type_ENUMS.ABPositive
            ],
            [Blood_Type_ENUMS.ANegative] =
            [
                Blood_Type_ENUMS.ANegative, Blood_Type_ENUMS.APositive,
                Blood_Type_ENUMS.ABNegative, Blood_Type_ENUMS.ABPositive
            ],
            [Blood_Type_ENUMS.APositive] =
            [
                Blood_Type_ENUMS.APositive, Blood_Type_ENUMS.ABPositive
            ],
            [Blood_Type_ENUMS.BNegative] =
            [
                Blood_Type_ENUMS.BNegative, Blood_Type_ENUMS.BPositive,
                Blood_Type_ENUMS.ABNegative, Blood_Type_ENUMS.ABPositive
            ],
            [Blood_Type_ENUMS.BPositive] =
            [
                Blood_Type_ENUMS.BPositive, Blood_Type_ENUMS.ABPositive
            ],
            [Blood_Type_ENUMS.ABNegative] =
            [
                Blood_Type_ENUMS.ABNegative, Blood_Type_ENUMS.ABPositive
            ],
            [Blood_Type_ENUMS.ABPositive] =
            [
                Blood_Type_ENUMS.ABPositive
            ]
        };

        public static string ToText(Blood_Type_ENUMS type) => type switch
        {
            Blood_Type_ENUMS.APositive => "A+",
            Blood_Type_ENUMS.ANegative => "A-",
            Blood_Type_ENUMS.BPositive => "B+",
            Blood_Type_ENUMS.BNegative => "B-",
            Blood_Type_ENUMS.ABPositive => "AB+",
            Blood_Type_ENUMS.ABNegative => "AB-",
            Blood_Type_ENUMS.OPositive => "O+",
            Blood_Type_ENUMS.ONegative => "O-",
            _ => type.ToString()
        };

        public static bool CanDonate(Blood_Type_ENUMS donor, Blood_Type_ENUMS recipient)
            => CanDonateToMap.TryGetValue(donor, out List<Blood_Type_ENUMS>? recipients)
               && recipients.Contains(recipient);

        public static List<Blood_Type_ENUMS> GetRecipients(Blood_Type_ENUMS donor)
            => CanDonateToMap.TryGetValue(donor, out List<Blood_Type_ENUMS>? recipients)
                ? recipients
                : new List<Blood_Type_ENUMS>();

        public static List<Blood_Type_ENUMS> GetDonors(Blood_Type_ENUMS recipient)
            => CanDonateToMap
                .Where(pair => pair.Value.Contains(recipient))
                .Select(pair => pair.Key)
                .ToList();

        public static string JoinText(IEnumerable<Blood_Type_ENUMS> types)
            => string.Join(", ", types.Select(ToText));
    }
}
