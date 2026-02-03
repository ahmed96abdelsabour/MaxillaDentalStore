using MaxillaDentalStore.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Common.Helpers
{
    public class DateTimeHelper : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime LocalNow => 
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow , TimeZoneInfo.Local);
    }
}
