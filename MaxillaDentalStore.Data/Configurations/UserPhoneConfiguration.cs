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
    internal class UserPhoneConfiguration : IEntityTypeConfiguration<UserPhone>
    {
        public void Configure(EntityTypeBuilder<UserPhone> builder)
        {
            // Table name
            builder.ToTable("UserPhones");
            builder.HasKey(up => up.UserPhoneId);


            // phone reuired  with max length 20

            builder.Property(up => up.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);


            // prevent duplicate phone numbers for the same user
            // set a unique index on UserId and PhoneNumber
            builder.HasIndex(up => new { up.UserId, up.PhoneNumber })
                .IsUnique()
                .HasDatabaseName("Index_User_Phone_Unique");


            // relationship with User
            // if a user is deleted, delete their phone numbers as well
            builder.HasOne(up => up.User)
                .WithMany(u => u.UserPhones) 
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);


        }

    }
}
