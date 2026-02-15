using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.UserPreferences.Queries.GetUserPreferences;

public record GetUserPreferencesQuery : IRequest<UserPreferencesDto>;

public class GetUserPreferencesQueryHandler : IRequestHandler<GetUserPreferencesQuery, UserPreferencesDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;

    public GetUserPreferencesQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
    }

    public async Task<UserPreferencesDto> Handle(GetUserPreferencesQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        var preferences = await _context.UserPreferences
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (preferences == null)
        {
            // Return default preferences without creating a DB record
            return new UserPreferencesDto
            {
                WeightUnit = "Lbs",
                DistanceUnit = "Miles",
                Bodyweight = null
            };
        }

        return _mapper.Map<UserPreferencesDto>(preferences);
    }
}
