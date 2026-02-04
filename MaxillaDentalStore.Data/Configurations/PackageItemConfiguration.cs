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
    public class PackageItemConfiguration : IEntityTypeConfiguration<PackageItem>
    {
        public void Configure(EntityTypeBuilder<PackageItem> builder)
        {
            // set table name
            builder.ToTable("PackageItems");
            builder.HasKey(pi => pi.PackageItemId);

            // set quantity with default value 1
            builder.Property(pi => pi.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            // set uique constraint on PackageId + ProductId
            // prevent adding new line it increase quantity instead
            builder.HasIndex(pi => new { pi.PackageId, pi.ProductId })
                .IsUnique()
                .HasDatabaseName("Index_Unique_Package_Product");


            #region relations

            // relation with Package
            builder.HasOne(pi => pi.Package)
                .WithMany(p => p.PackageItems)
                .HasForeignKey(pi => pi.PackageId)
                .OnDelete(DeleteBehavior.Cascade);

            //  relation with Product
            // with restrict on delete to prevent deleting product if used in package
            builder.HasOne(pi => pi.Product)
                .WithMany(p => p.packageItems)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

        }
    }
}
