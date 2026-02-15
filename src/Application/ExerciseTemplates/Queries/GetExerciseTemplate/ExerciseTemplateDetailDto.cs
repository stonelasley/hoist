using Hoist.Domain.Entities;

namespace Hoist.Application.ExerciseTemplates.Queries.GetExerciseTemplate;

public class ExerciseTemplateDetailDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string ImplementType { get; init; } = string.Empty;

    public string ExerciseType { get; init; } = string.Empty;

    public string? ImagePath { get; init; }

    public string? Model { get; init; }

    public DateTimeOffset Created { get; init; }

    public DateTimeOffset LastModified { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ExerciseTemplate, ExerciseTemplateDetailDto>()
                .ForMember(d => d.ImplementType, opt => opt.MapFrom(s => s.ImplementType.ToString()))
                .ForMember(d => d.ExerciseType, opt => opt.MapFrom(s => s.ExerciseType.ToString()));
        }
    }
}
