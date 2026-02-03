using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Common.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// Trim + Lowercase (culture-invariant)
        /// Useful for comparisons & searching
        /// </summary>
        public static string Normalize(this string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? string.Empty
                : input.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Returns null if string is empty or whitespace
        /// Useful for optional DB fields
        /// </summary>
        public static string NullIfEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null!
                : value.Trim();
        }


        /// <summary>
        /// Limits string length (DB & DTO safety)
        /// </summary
        public static string Truncate(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || maxLength <= 0)
            {
                return string.Empty;
            }
            return input.Length <= maxLength ? input : input.Substring(0, maxLength);
        }

        public static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }
}
