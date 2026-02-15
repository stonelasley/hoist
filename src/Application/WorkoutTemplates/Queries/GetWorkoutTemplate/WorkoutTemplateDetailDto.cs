using Hoist.Domain.Entities;

namespace Hoist.Application.WorkoutTemplates.Queries.GetWorkoutTemplate;

public class WorkoutTemplateDetailDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Notes { get; init; }

    public string? Location { get; init; }

    public DateTimeOffset Created { get; init; }

    public DateTimeOffset LastModified { get; init; }

    public List<WorkoutTemplateExerciseDto> Exercises { get; init; } = new();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<WorkoutTemplate, WorkoutTemplateDetailDto>();
        }
    }
}

public class WorkoutTemplateExerciseDto
{
    public int Id { get; init; }

    public int ExerciseTemplateId { get; init; }

    public string ExerciseName { get; init; } = string.Empty;

    public string ImplementType { get; init; } = string.Empty;

    public string ExerciseType { get; init; } = string.Empty;

    public int Position { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<WorkoutTemplateExercise, WorkoutTemplateExerciseDto>()
                .ForMember(d => d.ExerciseName, opt => opt.MapFrom(s => s.ExerciseTemplate.Name))
                .ForMember(d => d.ImplementType, opt => opt.MapFrom(s => s.ExerciseTemplate.ImplementType.ToString()))
                .ForMember(d => d.ExerciseType, opt => opt.MapFrom(s => s.ExerciseTemplate.ExerciseType.ToString()));
        }
    }
}
