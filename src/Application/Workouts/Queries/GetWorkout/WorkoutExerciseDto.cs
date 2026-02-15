using Hoist.Domain.Entities;

namespace Hoist.Application.Workouts.Queries.GetWorkout;

public class WorkoutExerciseDto
{
    public int Id { get; init; }

    public int? ExerciseTemplateId { get; init; }

    public string ExerciseName { get; init; } = string.Empty;

    public string ImplementType { get; init; } = string.Empty;

    public string ExerciseType { get; init; } = string.Empty;

    public int Position { get; init; }

    public List<WorkoutSetDto> Sets { get; init; } = new();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<WorkoutExercise, WorkoutExerciseDto>()
                .ForMember(d => d.ImplementType, opt => opt.MapFrom(s => s.ImplementType.ToString()))
                .ForMember(d => d.ExerciseType, opt => opt.MapFrom(s => s.ExerciseType.ToString()))
                .ForMember(d => d.Sets, opt => opt.MapFrom(s => s.Sets.OrderBy(set => set.Position)));
        }
    }
}
