using Hoist.Domain.Entities;
using Hoist.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoist.Infrastructure.Data.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.Property(l => l.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(l => l.InstagramHandle)
            .HasMaxLength(30);

        builder.Property(l => l.Latitude)
            .HasPrecision(9, 6);

        builder.Property(l => l.Longitude)
            .HasPrecision(10, 6);

        builder.Property(l => l.Notes)
            .HasMaxLength(2000);

        builder.Property(l => l.Address)
            .HasMaxLength(500);

        builder.Property(l => l.UserId)
            .IsRequired();

        builder.Property(l => l.IsDeleted)
            .HasDefaultValue(false);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(l => !l.IsDeleted);

        builder.HasIndex(l => new { l.UserId, l.IsDeleted });
    }
}
