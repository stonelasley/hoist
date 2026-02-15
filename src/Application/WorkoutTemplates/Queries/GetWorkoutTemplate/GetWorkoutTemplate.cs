using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.WorkoutTemplates.Queries.GetWorkoutTemplate;

public record GetWorkoutTemplateQuery(int Id) : IRequest<WorkoutTemplateDetailDto>;

public class GetWorkoutTemplateQueryHandler : IRequestHandler<GetWorkoutTemplateQuery, WorkoutTemplateDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;

    public GetWorkoutTemplateQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
    }

    public async Task<WorkoutTemplateDetailDto> Handle(GetWorkoutTemplateQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        var entity = await _context.WorkoutTemplates
            .IgnoreQueryFilters()
            .Where(x => x.Id == request.Id && x.UserId == userId)
            .Include(x => x.Exercises.OrderBy(e => e.Position))
                .ThenInclude(e => e.ExerciseTemplate)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        return _mapper.Map<WorkoutTemplateDetailDto>(entity);
    }
}
