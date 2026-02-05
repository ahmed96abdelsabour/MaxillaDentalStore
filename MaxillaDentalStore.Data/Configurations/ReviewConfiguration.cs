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
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            // set table name
            builder.ToTable("Reviews");
            builder.HasKey(r => r.ReviewId);

            // set review with star rating
            builder.Property(r => r.ReviewRate)
                .IsRequired();

            //set review text with max length is optional
            builder.Property(r => r.ReviewText)
                .HasMaxLength(1500);

            // set created at with default value
            builder.Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            // may rate either product or package
            builder.Property(r => r.ProductId).IsRequired(false);
            builder.Property(r => r.PackageId).IsRequired(false);


            #region relationships

            // relation with User
            builder.HasOne(r => r.User)
                .WithMany(u => u.Reviews) 
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // relation with Product
            builder.HasOne(r => r.Product)
                .WithMany(p => p.Reviews) 
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // relation with Package
            builder.HasOne(r => r.Package)
                .WithMany(pk => pk.Reviews) 
                .HasForeignKey(r => r.PackageId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion


            // index for faster retrieval of reviews by product and created at
            builder.HasIndex(r => new { r.ProductId, r.CreatedAt });

            // index for faster retrieval of reviews by package and created at
            builder.HasIndex(r => new { r.PackageId, r.CreatedAt });

            // index for faster retrieval of reviews by user
            builder.HasIndex(r => r.UserId);

        }
    }
}