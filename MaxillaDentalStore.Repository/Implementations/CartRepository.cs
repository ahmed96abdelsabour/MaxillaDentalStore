using MaxillaDentalStore.Data;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Repositories.Implementations
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        // get cart by user id with items and product details
        public async Task<Cart?> GetCartByUserIdAsync(int userId)
        {
            return await _context.Carts
                   .Include(c => c.CartItems)            
                   .ThenInclude(ci => ci.Product)
                       .ThenInclude(p => p.productImages)
                   .Include(c => c.CartItems)
                   .ThenInclude(ci => ci.Package)
                   .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart?> GetActiveCartDetailsAsync(int userId)
        {
            return await _context.Carts
                .AsNoTracking()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.productImages)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Package)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        // add new cart for user
        public async Task AddAsync(Cart cart)
        {
            await _context.Carts.AddAsync(cart);
        }


        // update cart (e.g., add/remove items, change quantities)
        public void Update(Cart cart)
        {
            _context.Carts.Update(cart);
        }

        public void ClearCart(Cart cart)
        {
            
            _context.Carts.Remove(cart);
        }
    }
}
