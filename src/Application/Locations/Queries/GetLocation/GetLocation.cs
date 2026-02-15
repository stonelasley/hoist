using Hoist.Application.Common.Interfaces;
using Hoist.Application.Locations.Queries.GetLocations;

namespace Hoist.Application.Locations.Queries.GetLocation;

public record GetLocationQuery(int Id) : IRequest<LocationBriefDto>;

public class GetLocationQueryHandler : IRequestHandler<GetLocationQuery, LocationBriefDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;

    public GetLocationQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
    }

    public async Task<LocationBriefDto> Handle(GetLocationQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        var entity = await _context.Locations
            .Where(x => x.Id == request.Id && x.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        return _mapper.Map<LocationBriefDto>(entity);
    }
}
