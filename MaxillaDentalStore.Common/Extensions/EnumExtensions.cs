using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Common.Extensions
{
    public static class EnumExtensions
    {
        public static string ToDisplayName(this Enum enumValue)
            => enumValue.ToString();

    }
}
