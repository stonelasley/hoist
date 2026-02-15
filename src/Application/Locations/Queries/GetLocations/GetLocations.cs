using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.Locations.Queries.GetLocations;

public record GetLocationsQuery : IRequest<List<LocationBriefDto>>;

public class GetLocationsQueryHandler : IRequestHandler<GetLocationsQuery, List<LocationBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;

    public GetLocationsQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
    }

    public async Task<List<LocationBriefDto>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        return await _context.Locations
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ProjectTo<LocationBriefDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
