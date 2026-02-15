using AutoMapper;

namespace Hoist.Application.UserPreferences.Queries.GetUserPreferences;

public class UserPreferencesDto
{
    public string WeightUnit { get; init; } = "Lbs";

    public string DistanceUnit { get; init; } = "Miles";

    public decimal? Bodyweight { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Domain.Entities.UserPreferences, UserPreferencesDto>()
                .ForMember(d => d.WeightUnit, opt => opt.MapFrom(s => s.WeightUnit.ToString()))
                .ForMember(d => d.DistanceUnit, opt => opt.MapFrom(s => s.DistanceUnit.ToString()));
        }
    }
}
