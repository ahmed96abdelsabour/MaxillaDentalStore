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
    internal class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            // to set name of table 
            builder.ToTable("OrderItems");
            builder.HasKey(oi => oi.OrderItemId);


            // set property of price at purchase is required and decimal type
            builder.Property(oi => oi.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            // set final price is required and decimal type
            builder.Property(oi => oi.TotalPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");


            // set cololr , size , material and notes are optional
            builder.Property(oi => oi.SelectedColor).HasMaxLength(50);
            builder.Property(oi => oi.SelectedSize).HasMaxLength(50);
            builder.Property(oi => oi.SelectedMaterial).HasMaxLength(100);
            builder.Property(oi => oi.ItemNotes).HasMaxLength(500);

            // set quantity is required
            builder.Property(oi => oi.Quantity)
                .IsRequired()
                .HasDefaultValue(1);


            #region relationships

            // relationship between OrderItem and Product (many-to-one, optional)
            builder.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // relationship between OrderItem and Product (many-to-one, optional)

            builder.HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            // relationship between OrderItem and Package (many-to-one, optional)
            builder.HasOne(oi => oi.Package)
                .WithMany(pk => pk.orderItems) 
                .HasForeignKey(oi => oi.PackageId)
                .OnDelete(DeleteBehavior.SetNull);

            #endregion


            // indexes for preformance optimization
            builder.HasIndex(oi => oi.OrderId)
                .HasDatabaseName("Index_OrderItem_OrderId");
        }
    }
}
