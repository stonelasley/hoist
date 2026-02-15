using Hoist.Application.Common.Interfaces;
using Hoist.Application.Workouts.Queries.GetWorkout;
using Hoist.Domain.Enums;

namespace Hoist.Application.Workouts.Queries.GetInProgressWorkout;

public record GetInProgressWorkoutQuery : IRequest<WorkoutDetailDto?>
{
}

public class GetInProgressWorkoutQueryHandler : IRequestHandler<GetInProgressWorkoutQuery, WorkoutDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;

    public GetInProgressWorkoutQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
    }

    public async Task<WorkoutDetailDto?> Handle(GetInProgressWorkoutQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id!;

        return await _context.Workouts
            .Include(w => w.Exercises.OrderBy(e => e.Position))
                .ThenInclude(e => e.Sets.OrderBy(s => s.Position))
            .Where(w => w.UserId == userId && w.Status == WorkoutStatus.InProgress)
            .ProjectTo<WorkoutDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
