using Hoist.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoist.Infrastructure.Data.Configurations;

public class WorkoutTemplateExerciseConfiguration : IEntityTypeConfiguration<WorkoutTemplateExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutTemplateExercise> builder)
    {
        builder.Property(w => w.WorkoutTemplateId)
            .IsRequired();

        builder.Property(w => w.ExerciseTemplateId)
            .IsRequired();

        builder.Property(w => w.Position)
            .IsRequired();

        builder.HasOne(w => w.WorkoutTemplate)
            .WithMany(t => t.Exercises)
            .HasForeignKey(w => w.WorkoutTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.ExerciseTemplate)
            .WithMany(e => e.WorkoutTemplateExercises)
            .HasForeignKey(w => w.ExerciseTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(w => new { w.WorkoutTemplateId, w.Position });
    }
}
