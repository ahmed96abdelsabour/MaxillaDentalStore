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
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // set table name
            builder.ToTable("Orders");
            builder.HasKey(o => o.OrderId);


            // set address and phone number properties is required
         
            builder.Property(o => o.ShippingAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(o => o.phoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(o => o.Notes)
                .HasMaxLength(1000);

            // store enum as string
            // helps in readability when looking at the database directly
            // allows for easier addition of new statuses in the future without breaking existing data
            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(OrderStatus.Pending);

            // set default value for order date auromatically set to current date/time when created
            builder.Property(o => o.OrderDate)
                .HasDefaultValueSql("GETDATE()");

            // set price with precision (18,2)
            // helps in financial calculations and reporting
            builder.Property(o => o.TotalPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();


            // prevent cascade delete to avoid accidental data loss
            // when a user is deleted, their orders remain for record-keeping
            builder.HasOne(o => o.User)
                .WithMany(u => u.Orders) 
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // add indexes to improve query performance on frequently searched fields
            builder.HasIndex(o => o.OrderDate)
                .HasDatabaseName("Index_Order_Date");

            builder.HasIndex(o => o.Status)
                .HasDatabaseName("Index_Order_Status");
        }
    }
}