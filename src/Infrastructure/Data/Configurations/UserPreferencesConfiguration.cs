using Hoist.Domain.Entities;
using Hoist.Domain.Enums;
using Hoist.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoist.Infrastructure.Data.Configurations;

public class UserPreferencesConfiguration : IEntityTypeConfiguration<UserPreferences>
{
    public void Configure(EntityTypeBuilder<UserPreferences> builder)
    {
        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.WeightUnit)
            .HasDefaultValue(WeightUnit.Lbs);

        builder.Property(p => p.DistanceUnit)
            .HasDefaultValue(DistanceUnit.Miles);

        builder.Property(p => p.Bodyweight)
            .HasPrecision(6, 2);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(p => p.UserId)
            .IsUnique();
    }
}
