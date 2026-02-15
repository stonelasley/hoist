using Hoist.Domain.Entities;
using Hoist.Domain.Enums;
using Hoist.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoist.Infrastructure.Data.Configurations;

public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        builder.Property(w => w.TemplateName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(w => w.Status)
            .HasDefaultValue(WorkoutStatus.InProgress);

        builder.Property(w => w.StartedAt)
            .IsRequired();

        builder.Property(w => w.Notes)
            .HasMaxLength(2000);

        builder.Property(w => w.LocationName)
            .HasMaxLength(200);

        builder.Property(w => w.UserId)
            .IsRequired();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.WorkoutTemplate)
            .WithMany()
            .HasForeignKey(w => w.WorkoutTemplateId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(w => w.Location)
            .WithMany()
            .HasForeignKey(w => w.LocationId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(w => new { w.UserId, w.Status });
        builder.HasIndex(w => new { w.UserId, w.EndedAt });
        builder.HasIndex(w => new { w.UserId, w.Rating });
        builder.HasIndex(w => w.LocationId);
    }
}
