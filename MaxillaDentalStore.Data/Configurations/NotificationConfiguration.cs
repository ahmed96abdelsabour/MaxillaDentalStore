using MaxillaDentalStore.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaxillaDentalStore.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");
            builder.HasKey(n => n.NotificationId);

            // Properties
            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(1000);

            // Store enum as string for readability
            builder.Property(n => n.Type)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(n => n.IsRead)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(n => n.CreatedAt)
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            // Relationships
            builder.HasOne(n => n.RecipientUser)
                .WithMany()
                .HasForeignKey(n => n.RecipientUserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            builder.HasOne(n => n.RelatedUser)
                .WithMany()
                .HasForeignKey(n => n.RelatedUserId)
                .OnDelete(DeleteBehavior.Restrict) // Prevent cascade delete
                .IsRequired(false);

            builder.HasOne(n => n.Order)
                .WithMany()
                .HasForeignKey(n => n.OrderId)
                .OnDelete(DeleteBehavior.Restrict) // Prevent cascade delete
                .IsRequired(false);

            builder.HasOne(n => n.Review)
                .WithMany()
                .HasForeignKey(n => n.ReviewId)
                .OnDelete(DeleteBehavior.Restrict) // Prevent cascade delete
                .IsRequired(false);

            // Indexes for performance optimization
            builder.HasIndex(n => new { n.RecipientUserId, n.IsRead, n.CreatedAt })
                .HasDatabaseName("IX_Notifications_Recipient_IsRead_CreatedAt");

            builder.HasIndex(n => n.Type)
                .HasDatabaseName("IX_Notifications_Type");

            builder.HasIndex(n => n.CreatedAt)
                .HasDatabaseName("IX_Notifications_CreatedAt");
        }
    }
}
