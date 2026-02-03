using MaxillaDentalStore.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToShortDate ( this DateTime dateTime)
        {
            return dateTime.ToString (DateFormats.Date);
        }

        public static string ToShortDateTime ( this DateTime dateTime)
        {
            return dateTime.ToString (DateFormats.DateTime);
        }
        public static string ToFullDateTime ( this DateTime dateTime)
        {
            return dateTime.ToString (DateFormats.FullDateTime);
        }
    }
}
