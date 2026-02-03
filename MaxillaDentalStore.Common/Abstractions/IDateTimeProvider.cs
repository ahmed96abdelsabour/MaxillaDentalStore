using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Common.Abstractions
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
        DateTime LocalNow { get; }
    }
}
