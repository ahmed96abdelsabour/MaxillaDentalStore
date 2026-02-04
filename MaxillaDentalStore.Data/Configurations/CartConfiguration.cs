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
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {

            // to set name of table and primary key
            builder.ToTable("Carts");
            builder.HasKey(c => c.CartId);



            // property to set time outmatically from server side when creating new cart
           // to avoid issues with client time settings
            builder.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            // set default value for IsActive to true
            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);


            // index for UserId and IsActive to optimize queries
            // to avoid latency when searching for active cart of a user
            builder.HasIndex(c => new { c.UserId, c.IsActive })
                .HasDatabaseName("Index_Cart_User_Active");


            // set relation 1:1 with User
            builder.HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // set relation 1:many with CartItems
            builder.HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
