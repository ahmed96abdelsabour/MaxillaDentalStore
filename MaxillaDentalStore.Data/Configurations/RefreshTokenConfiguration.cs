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
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            // Primary Key
            builder.HasKey(rt => rt.RefreshTokenId);

            // Properties
            builder.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(500); // Hashed token

            builder.Property(rt => rt.ExpiresAt)
                .IsRequired();

            builder.Property(rt => rt.CreatedAt)
                .IsRequired();

            builder.Property(rt => rt.RevokedAt)
                .IsRequired(false); // Nullable

            builder.Property(rt => rt.DeviceId)
                .HasMaxLength(100)
                .IsRequired(false);

            // Indexes for performance
            builder.HasIndex(rt => rt.UserId)
                .HasDatabaseName("IX_RefreshToken_UserId");

            builder.HasIndex(rt => rt.Token)
                .IsUnique()
                .HasDatabaseName("IX_RefreshToken_Token");

            builder.HasIndex(rt => rt.ExpiresAt)
                .HasDatabaseName("IX_RefreshToken_ExpiresAt");

            // Composite index for active tokens query
            builder.HasIndex(rt => new { rt.UserId, rt.RevokedAt, rt.ExpiresAt })
                .HasDatabaseName("IX_RefreshToken_UserId_RevokedAt_ExpiresAt");

            // Relationships
            builder.HasOne(rt => rt.User)
                .WithMany() // User can have multiple refresh tokens (different devices)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Delete tokens when user deleted

            // Ignore computed properties
            builder.Ignore(rt => rt.IsExpired);
            builder.Ignore(rt => rt.IsActive);
        }
    }
}
