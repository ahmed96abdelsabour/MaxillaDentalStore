using MaxillaDentalStore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Repositories.Interfaces
{
    public interface IProductRepository
    {
        // 🔹 Base
        Task<Product?> GetByIdAsync(int productId);
        Task<IEnumerable<Product>> GetAllAsync();

        // 🔹 Queries 
        Task<IEnumerable<Product>> GetAvailableAsync();
        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> SearchAsync(string keyword);

        // 🔹 Details (Use-case based)
        Task<Product?> GetWithImagesAsync(int productId);
        Task<Product?> GetWithCategoriesAsync(int productId);

        // 🔹 Admin
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int productId);
    }
}
