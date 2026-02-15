using Hoist.Domain.Entities;
using Hoist.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoist.Infrastructure.Data.Configurations;

public class ExerciseTemplateConfiguration : IEntityTypeConfiguration<ExerciseTemplate>
{
    public void Configure(EntityTypeBuilder<ExerciseTemplate> builder)
    {
        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.ImplementType)
            .IsRequired();

        builder.Property(e => e.ExerciseType)
            .IsRequired();

        builder.Property(e => e.ImagePath)
            .HasMaxLength(500);

        builder.Property(e => e.Model)
            .HasMaxLength(500);

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Location)
            .WithMany()
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.LocationId);

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasIndex(e => new { e.UserId, e.Name })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(e => new { e.UserId, e.IsDeleted });
    }
}
