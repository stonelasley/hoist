using Hoist.Domain.Entities;

namespace Hoist.Application.WorkoutTemplates.Queries.GetWorkoutTemplates;

public class WorkoutTemplateBriefDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Notes { get; init; }

    public int? LocationId { get; init; }

    public string? LocationName { get; init; }

    public DateTimeOffset Created { get; init; }

    public DateTimeOffset LastModified { get; init; }

    public int ExerciseCount { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<WorkoutTemplate, WorkoutTemplateBriefDto>()
                .ForMember(d => d.ExerciseCount, opt => opt.MapFrom(s => s.Exercises.Count))
                .ForMember(d => d.LocationName, opt => opt.MapFrom(s => s.Location != null ? s.Location.Name : null));
        }
    }
}
