using Hoist.Domain.Entities;

namespace Hoist.Application.Workouts.Queries.GetWorkout;

public class WorkoutSetDto
{
    public int Id { get; init; }

    public int Position { get; init; }

    public decimal? Weight { get; init; }

    public int? Reps { get; init; }

    public int? Duration { get; init; }

    public decimal? Distance { get; init; }

    public decimal? Bodyweight { get; init; }

    public string? BandColor { get; init; }

    public string? WeightUnit { get; init; }

    public string? DistanceUnit { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<WorkoutSet, WorkoutSetDto>()
                .ForMember(d => d.WeightUnit, opt => opt.MapFrom(s => s.WeightUnit.HasValue ? s.WeightUnit.Value.ToString() : null))
                .ForMember(d => d.DistanceUnit, opt => opt.MapFrom(s => s.DistanceUnit.HasValue ? s.DistanceUnit.Value.ToString() : null));
        }
    }
}
