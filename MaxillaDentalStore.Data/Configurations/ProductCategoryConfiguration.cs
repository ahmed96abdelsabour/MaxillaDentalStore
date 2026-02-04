using MaxillaDentalStore.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Configurations
{
     public class ProductCategoryConfiguration : IEntityTypeConfiguration <ProductCategory>
    {
        public void Configure (EntityTypeBuilder<ProductCategory> builder)
        {
            builder.ToTable("ProductCategories");

            // to set relationships and composite key
            builder.HasKey(pc => new { pc.ProductId, pc.CategoryId });


            // relationship between Product with Category
            // one product can have many categories
            // if we delete a product all its categories in the join table will be deleted
            builder.HasOne(pc => pc.Product)
                .WithMany(p => p.productCategories)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // relationship between Category with Product
            // one category can have many products
            // if we delete a category all its products in the join table will be deleted
            builder.HasOne(pc => pc.Category)
                .WithMany(c => c.productCategories) 
                .HasForeignKey(pc => pc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);



            // indexes in the join table to help in searching fastly 
            // if we want to know "what are all categories for a specific product" the search will be very fast

            builder.HasIndex(pc => pc.CategoryId)
                .HasDatabaseName("Index_ProductCategory_CategoryId");
        }


    }
}
