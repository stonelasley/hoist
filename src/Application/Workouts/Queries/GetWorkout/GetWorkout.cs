using Hoist.Application.Common.Interfaces;
using Hoist.Domain.Entities;

namespace Hoist.Application.Workouts.Queries.GetWorkout;

public record GetWorkoutQuery : IRequest<WorkoutDetailDto>
{
    public int Id { get; init; }
}

public class GetWorkoutQueryHandler : IRequestHandler<GetWorkoutQuery, WorkoutDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;

    public GetWorkoutQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
    }

    public async Task<WorkoutDetailDto> Handle(GetWorkoutQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id!;

        var workout = await _context.Workouts
            .Include(w => w.Exercises.OrderBy(e => e.Position))
                .ThenInclude(e => e.Sets.OrderBy(s => s.Position))
            .Where(w => w.Id == request.Id && w.UserId == userId)
            .ProjectTo<WorkoutDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (workout == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Workout), request.Id);
        }

        return workout;
    }
}
