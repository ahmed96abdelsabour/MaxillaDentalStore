using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        public Task AddAsync(Product product)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Product>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Product>> GetAvailableAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
        {
            throw new NotImplementedException();
        }

        public Task<Product?> GetByIdAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<Product?> GetWithCategoriesAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<Product?> GetWithImagesAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Product>> SearchAsync(string keyword)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Product product)
        {
            throw new NotImplementedException();
        }
    }
}
