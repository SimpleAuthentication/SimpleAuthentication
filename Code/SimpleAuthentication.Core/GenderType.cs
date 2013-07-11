using System;

namespace SimpleAuthentication.Core
{
    /// <summary>
    /// Human genders.
    /// </summary>
    public enum GenderType
    {
        Unknown,
        Male,
        Female
    }

    /// <summary>
    /// Some helper methods for GenderTypes.
    /// </summary>
    public static class GenderTypeHelpers
    {
        /// <summary>
        /// Converts a string to a GenderType.
        /// </summary>
        /// <param name="value">the gender type to convert.</param>
        /// <returns>The converted gender type.</returns>
        public static GenderType ToGenderType(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            switch (value.ToLowerInvariant())
            {
                case "male" : return GenderType.Male;
                case "female" : return GenderType.Female;
                default: return GenderType.Unknown;
            }
        }
    }
}