using Hoist.Domain.Entities;

namespace Hoist.Application.ExerciseTemplates.Queries.GetExerciseTemplates;

public class ExerciseTemplateBriefDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string ImplementType { get; init; } = string.Empty;

    public string ExerciseType { get; init; } = string.Empty;

    public string? ImagePath { get; init; }

    public string? Model { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ExerciseTemplate, ExerciseTemplateBriefDto>()
                .ForMember(d => d.ImplementType, opt => opt.MapFrom(s => s.ImplementType.ToString()))
                .ForMember(d => d.ExerciseType, opt => opt.MapFrom(s => s.ExerciseType.ToString()));
        }
    }
}
