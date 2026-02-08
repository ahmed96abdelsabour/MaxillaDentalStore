using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;
using MaxillaDentalStore.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Implementations
{


        public class CategoryService : ICategoryService
        {

        // declare the dependencies for the unit of work and the mapper, which will be injected through the constructor    
        private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;

        // constructor to initialize the dependencies
        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
            }

        // implementation  of get all categories, get category
        public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
                return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
            }

        // implementation of get category by id
        public async Task<CategoryDTO> GetByIdAsync(int id)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                return _mapper.Map<CategoryDTO>(category);
            }

        // implementation of get products by category id
        public async Task<IEnumerable<ProductResponseDto>> GetProductsByCategoryIdAsync(int categoryId)
        {
            var products = await _unitOfWork.Products.GetProductsByCategoryIdAsync(categoryId);
            return _mapper.Map<IEnumerable<ProductResponseDto>>(products);
        }

        // implementation of create category

        public async Task<CategoryDTO> CreateAsync(CreateCategoryDTO dto)
            {
                var category = _mapper.Map<Category>(dto);
                await _unitOfWork.Categories.AddAsync(category);


                await _unitOfWork.CommitAsync();

                return _mapper.Map<CategoryDTO>(category);
            }

        // implementation of update category
        public async Task<bool> UpdateAsync(int id, CreateCategoryDTO dto)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null) return false;


                _mapper.Map(dto, category);

                _unitOfWork.Categories.Update(category);
                var result = await _unitOfWork.CommitAsync();

                return result > 0;
            }


        // implementation of delete category
        public async Task<bool> DeleteAsync(int id)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null) return false;

                _unitOfWork.Categories.Delete(category);
                var result = await _unitOfWork.CommitAsync();

                return result > 0;
            }
        }
}

