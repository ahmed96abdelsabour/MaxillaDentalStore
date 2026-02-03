using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Common.Helpers
{
    public static class PriceHelper
    {
        // we can use it when we have to apply discount on price on product or service 
        // as it is a common operation in e-commerce applications
        // we can use it in various parts of the application wherever price calculation is needed dynamically
        public static decimal ApplyDiscount(decimal price, decimal discountPercentage)
        {
            if (discountPercentage < 0 || discountPercentage > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(discountPercentage), "Discount percentage must be between 0 and 100.");
            }
            var discountAmount = (price * discountPercentage) / 100;
            return price - discountAmount;
        }
    }
}
