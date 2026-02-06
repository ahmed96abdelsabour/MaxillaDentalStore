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
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        // get all with better performance by using AsNoTracking and ordering in the database
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .AsNoTracking()    // prevent ef from tracking changes to the entities, improving read performance
                .OrderBy(c => c.Name) 
                .ToListAsync();
        }


        // get by id with better performance by using FindAsync
        // which first checks the local cache before querying the database
        public async Task<Category?> GetByIdAsync(int categoryId)
        {
  
            return await _context.Categories.FindAsync(categoryId);
        }



        // eager loading to get category with its products in one query,
        // improving performance by reducing round-trips to the database
        public async Task<Category?> GetCategoryWithProductsAsync(int categoryId)
        {
            return await _context.Categories
                .AsNoTracking()
                .Include(c => c.productCategories) 
                    .ThenInclude(pc => pc.Product) 
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }


        // implement pagination to efficiently handle large datasets
        // using skip and take to fetch only the required subset of data
        public async Task<IEnumerable<Category>> GetPagedCategoriesAsync(int pageNumber, int pageSize)
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize) 
                .Take(pageSize)                   
                .ToListAsync();
        }

        //  check if a category exists by its ID using AnyAsync, which is more efficient than retrieving the entire entity
        //  when we only need to check for existence.
        public async Task<bool> ExistsAsync(int categoryId)
        {
            
            return await _context.Categories.AnyAsync(c => c.CategoryId == categoryId);
        }

        // add new category to the database,
        // the actual saving will be done in the UnitOfWork's SaveChangesAsync method to allow for transaction management and batching of multiple operations.
        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        // update an existing category,
        // the actual saving will be done in the UnitOfWork's SaveChangesAsync method to allow for transaction management and batching of multiple operations.
        public void Update(Category category)
        {
            _context.Categories.Update(category);
        }

        // delete a category from the database,
        public void Delete(Category category)
        {
            _context.Categories.Remove(category);
        }
    }
}
