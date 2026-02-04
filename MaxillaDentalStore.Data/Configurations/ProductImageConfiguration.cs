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
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            // set table name
            builder.ToTable("ProductImages");
            builder.HasKey(pi => pi.ProductImageId);

            //  image url settings
            builder.Property(pi => pi.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            // set index on product id
            // helpful for faster lookups when querying images by product
            builder.HasIndex(pi => pi.ProductId)
                .HasDatabaseName("Index_ProductImage_ProductId");

            
        }
    }
}


