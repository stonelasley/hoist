using Hoist.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoist.Infrastructure.Data.Configurations;

public class WorkoutSetConfiguration : IEntityTypeConfiguration<WorkoutSet>
{
    public void Configure(EntityTypeBuilder<WorkoutSet> builder)
    {
        builder.Property(s => s.Weight)
            .HasPrecision(8, 2);

        builder.Property(s => s.Distance)
            .HasPrecision(10, 4);

        builder.Property(s => s.Bodyweight)
            .HasPrecision(6, 2);

        builder.HasOne(s => s.WorkoutExercise)
            .WithMany(e => e.Sets)
            .HasForeignKey(s => s.WorkoutExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.WorkoutExerciseId, s.Position });
    }
}
