using MaxillaDentalStore.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Interfaces
{
    public interface ICategoryService
    {
        // get all categories
        Task<IEnumerable<CategoryDTO>> GetAllAsync();

        // get a category by ID
        Task<CategoryDTO> GetByIdAsync(int id);

        // get products belonging to a category
        Task<IEnumerable<ProductResponseDto>> GetProductsByCategoryIdAsync(int categoryId);

        // create a new category
        Task<CategoryDTO> CreateAsync(CreateCategoryDTO dto);

        // update an existing category
        Task<bool> UpdateAsync(int id, CreateCategoryDTO dto);

        // delete a category
        Task<bool> DeleteAsync(int id);
    }
}
