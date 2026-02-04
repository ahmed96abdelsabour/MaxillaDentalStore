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
    public class PackageConfiguration : IEntityTypeConfiguration<Package>
    {
        public void Configure(EntityTypeBuilder<Package> builder)
        {
            // set table name
            builder.ToTable("Packages");
            builder.HasKey(p => p.PackageId);

            //  property for package name and description
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            // set price with precision for package price and make it required
            builder.Property(p => p.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // set image url with max length for package image and make it optional
            builder.Property(p => p.ImageUrl)
                .HasMaxLength(500);

            //  set default value for created at to current date and time
            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            // set if package is available with default value true
            builder.Property(p => p.IsAvilable)
                .HasDefaultValue(true);


            // indexes for preformance optimization
            builder.HasIndex(p => new { p.IsAvilable, p.CreatedAt })
                .HasDatabaseName("Index_Package_Available_Date");


               #region Relationships

            // relations 1:M with PackageItems
            builder.HasMany(p => p.PackageItems)
                .WithOne(pi => pi.Package)
                .HasForeignKey(pi => pi.PackageId)
                .OnDelete(DeleteBehavior.Cascade);


            // relation M:N with Items through PackageItems
            builder.HasMany(p => p.cartItems) 
                .WithOne(ci => ci.Package)
                .HasForeignKey(ci => ci.PackageId)
                .OnDelete(DeleteBehavior.SetNull);

            //  relation M:N with Orders through OrderItems
            builder.HasMany(p => p.orderItems) 
                .WithOne(oi => oi.Package)
                .HasForeignKey(oi => oi.PackageId)
                .OnDelete(DeleteBehavior.SetNull);

            #endregion
        }
    }
}