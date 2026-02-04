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
    internal class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products");

            builder.HasKey(p => p.ProductId);

            #region  properties
            // properties of product name is required and max length 100

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            // properties of product description is optional and max length 500
            builder.Property(p => p.Description)
                .HasMaxLength(500);

            // property for product company is optional and max length 100

            builder.Property(p => p.Company)
                .HasMaxLength(100);

            // property for unit type is required and max length 50

            builder.Property(p => p.Price)
                .HasColumnType("decimal(18,2)") 
                .IsRequired();


            // property for unit type is optional and max length 50

            builder.Property(p => p.UnitType)
                .HasMaxLength(50);


            // property for discount is dafuault 0

            builder.Property(p => p.Discount)
                .HasDefaultValue(0);

            #endregion

            #region index 
            // index on final price to improve filtering performance
            builder.HasIndex(p => p.FinalPrice)
                .HasDatabaseName("Index_Product_FinalPrice");

            builder.HasIndex(p => p.IsActive)
                .HasDatabaseName("Index_Product_IsActive");


            // index on product name to improve search performance
            builder.HasIndex(p => p.Name)
                .HasDatabaseName("Index_Product_Name");


            #endregion

        }
    }
}
