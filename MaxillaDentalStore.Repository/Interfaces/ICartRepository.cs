using MaxillaDentalStore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Repositories.Interfaces
{
    public interface ICartRepository
    {
        // get cart by user id (used to retrieve the current active cart for a user)
        Task<Cart?> GetCartByUserIdAsync(int userId);

        /// <summary>
        /// Get active cart with full details (Items, Products, Packages) for Checkout
        /// </summary>
        Task<Cart?> GetActiveCartDetailsAsync(int userId);

        // add a new cart (usually when a new user registers or first add operation)
        Task AddAsync(Cart cart);

        // update the cart (modify quantities or remove items)
        void Update(Cart cart);

        // clear the entire cart (after completing the purchase)
        void ClearCart(Cart cart);
    }
}
