using Hoist.Domain.Entities;

namespace Hoist.Application.Locations.Queries.GetLocations;

public class LocationBriefDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? InstagramHandle { get; init; }

    public decimal? Latitude { get; init; }

    public decimal? Longitude { get; init; }

    public string? Notes { get; init; }

    public string? Address { get; init; }

    public DateTimeOffset Created { get; init; }

    public DateTimeOffset LastModified { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Location, LocationBriefDto>();
        }
    }
}
