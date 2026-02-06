using MaxillaDentalStore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MaxillaDentalStore.Repositories.Interfaces

{
        public interface ICategoryRepository
        {
           // get all categoriesa
           // using Task<IEnumerable<Category>> instead of Task<List<Category>>
           // is more flexible and allows the caller to decide how to consume the collection (e.g., as a List, an Array, etc.).

        // not using Task<List<Category>> because it forces the caller to use a List,
        // which may not always be necessary or optimal. By returning an IEnumerable<Category>,
        // we provide more flexibility and allow for better performance in scenarios where the caller may not need the full capabilities of a List.
               Task<IEnumerable<Category>> GetAllAsync();


        //  pagination for categories to improve performance and user experience
        //  when dealing with large datasets.
              Task<IEnumerable<Category>> GetPagedCategoriesAsync(int pageNumber, int pageSize);



        // gey specific category by id 
             Task<Category?> GetByIdAsync(int categoryId);



        // get category with its related products using eager loading to optimize performance and reduce the number of database calls.
             Task<Category?> GetCategoryWithProductsAsync(int categoryId);


        // add new category to the database and return the added category with its generated ID and any other database-generated values.
             Task AddAsync(Category category);



        // update existing category in the database. 
        // delete category from the database.
        // not using Task for Update and Delete
        // because these operations are typically performed synchronously and do not require asynchronous behavior.
            void Update(Category category);
            void Delete(Category category);


        // check if a category exists in the database by its ID.
           Task<bool> ExistsAsync(int categoryId);


        }


}

    

