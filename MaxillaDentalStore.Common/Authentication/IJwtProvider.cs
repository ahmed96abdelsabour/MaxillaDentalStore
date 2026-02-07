using MaxillaDentalStore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Common.Authentication
{
    public interface IJwtProvider
    {
        string Generate(User user);
    }
}
