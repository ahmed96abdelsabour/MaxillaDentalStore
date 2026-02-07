using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Mapping
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            // transfer entity catageroy into catagory DTO and reverse map to allow mapping from category DTO to category entity
            CreateMap<Category, CategoryDTO>().ReverseMap();

            // transfer from create category DTO to entity
            CreateMap<CreateCategoryDTO, Category>();
        }
    }
}
