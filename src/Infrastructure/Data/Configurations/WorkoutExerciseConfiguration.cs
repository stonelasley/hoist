using Hoist.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoist.Infrastructure.Data.Configurations;

public class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutExercise> builder)
    {
        builder.Property(e => e.ExerciseName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasOne(e => e.Workout)
            .WithMany(w => w.Exercises)
            .HasForeignKey(e => e.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ExerciseTemplate)
            .WithMany()
            .HasForeignKey(e => e.ExerciseTemplateId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(e => new { e.WorkoutId, e.Position });
    }
}
