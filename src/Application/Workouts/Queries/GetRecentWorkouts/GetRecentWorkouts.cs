using Hoist.Application.Common.Interfaces;
using Hoist.Domain.Enums;

namespace Hoist.Application.Workouts.Queries.GetRecentWorkouts;

public record GetRecentWorkoutsQuery : IRequest<List<WorkoutBriefDto>> { }

public class GetRecentWorkoutsQueryHandler : IRequestHandler<GetRecentWorkoutsQuery, List<WorkoutBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMapper _mapper;

    public GetRecentWorkoutsQueryHandler(IApplicationDbContext context, IUser user, IMapper mapper)
    {
        _context = context;
        _user = user;
        _mapper = mapper;
    }

    public async Task<List<WorkoutBriefDto>> Handle(GetRecentWorkoutsQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id!;

        var workouts = await _context.Workouts
            .Where(w => w.UserId == userId && w.Status == WorkoutStatus.Completed)
            .OrderByDescending(w => w.EndedAt)
            .Take(3)
            .ProjectTo<WorkoutBriefDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return workouts;
    }
}
