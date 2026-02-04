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
    internal class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {

            // to set name of table and primary key
            builder.ToTable("CartItems");
            builder.HasKey(ci => ci.CartItemId);


            // set quantity is required

            builder.Property(ci => ci.Quantity)
                .IsRequired()
                .HasDefaultValue(1);


            // to handle nullable foreign key relationships for ProductId and PackageId
            // both are optional, so we set them as not required
            builder.Property(ci => ci.ProductId).IsRequired(false);
            builder.Property(ci => ci.PackageId).IsRequired(false);


            // set index on CartId for performance optimization

            builder.HasIndex(ci => ci.CartId)
                .HasDatabaseName("Index_CartItem_CartId");


            // set relation between CartItem and Cart

            builder.HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);


            // set relation between CartItem and Product

            builder.HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.SetNull);


            // set relation between CartItem and Package
            // a cart item me cotain either a product or a package both are optional
            builder.HasOne(ci => ci.Package)
               .WithMany(pk => pk.cartItems) 
               .HasForeignKey(ci => ci.PackageId)
               .OnDelete(DeleteBehavior.SetNull);

        }
    }

}