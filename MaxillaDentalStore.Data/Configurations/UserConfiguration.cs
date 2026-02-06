using MaxillaDentalStore.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace MaxillaDentalStore.Data.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // to set table name
           builder.ToTable("Users");

            
            // to set primary key
            builder.HasKey(u => u.UserId);

            // to set property for name as required with max length 100
            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(100);

            // to set property for email as required with max length 150

            builder.Property (u => u.Email)
                .IsRequired()
                .HasMaxLength(150);

            // to set property for password hash as required
            builder.Property(u => u.PasswordHash)
                .IsRequired();

            // to set property for clinic name as optional with max length 200

            builder.Property(u => u.ClinicName)
                .HasMaxLength(200);

            // to set index on email to be unique
            // to ensure no two users can have the same email

            builder.HasIndex(u => u.Email)
                .IsUnique();

            // to set index on name to improve search performance when looking up users by name
            builder.HasIndex(u => u.Name)
                .HasDatabaseName("Index_User_Name");


            // save role in db as string

            builder.Property(u => u.Role)
                .HasConversion<string>();
            builder.HasIndex(u => u.Role)
                .HasDatabaseName("Index_User_Role");

            // to set property for clinic address as optional with max length 500
            builder.Property(u => u.ClinicAddress)
              .HasMaxLength(500);


            // to set user with one cart relationship
            // user is deleted, the cart is also deleted
            builder.HasOne (u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // to set user with user phones relationship
            // when user is deleted, all associated phones are also deleted
            builder.HasMany(u => u.UserPhones)
                .WithOne(up => up.User)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // to set user with orders relationship
            // when user is deleted, all associated orders are also deleted

            builder.HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // to set user with reviews relationship
            
             builder.HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // set property for user created at with default value of current date and time
            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()")
                .ValueGeneratedOnAdd();
        }
    }
}
