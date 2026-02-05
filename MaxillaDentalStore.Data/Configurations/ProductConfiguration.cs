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

            // property for unit type is required and max length 50 with precision 18 and scale 2

            builder.Property(p => p.Price)
                .HasPrecision(18, 2) 
                .IsRequired();


            // property for unit type is optional and max length 50

            builder.Property(p => p.UnitType)
                .HasMaxLength(50);


            // property for discount is dafuault 0

            builder.Property(p => p.Discount)
                .HasDefaultValue(0);

            #endregion


            #region index  

            // index on is active to improve performance of filtering active products
            builder.HasIndex(p => new {p.IsActive , p.Price})
                .HasDatabaseName("Index_Product_IsActive");


            // index on product name to improve search performance
            builder.HasIndex(p => p.Name)
                .HasDatabaseName("Index_Product_Name");


            #endregion

            #region relationships

            // product between order items one to many
            builder.HasMany(p => p.OrderItems)
                .WithOne(oi => oi.Product)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            // product between cart items one to many
            builder.HasMany(p => p.CartItems)
                .WithOne(ci => ci.Product)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // product between product images one to many

            builder.HasMany(p => p.productImages)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            // product between product categories many to many
            builder.HasMany(p => p.productCategories)
                .WithOne(pc => pc.Product)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            // product between package items one to many

            builder.HasMany(p => p.packageItems)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            // product between reviews one to many
            builder.HasMany(p => p.Reviews)
                .WithOne(r => r.Product)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

        }
    }
}
