using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.WorkoutTemplates.Queries.GetWorkoutTemplates;

public record GetWorkoutTemplatesQuery : IRequest<List<WorkoutTemplateBriefDto>>
{
    public int? LocationId { get; init; }
}

public class GetWorkoutTemplatesQueryHandler : IRequestHandler<GetWorkoutTemplatesQuery, List<WorkoutTemplateBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;

    public GetWorkoutTemplatesQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
    }

    public async Task<List<WorkoutTemplateBriefDto>> Handle(GetWorkoutTemplatesQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        var query = _context.WorkoutTemplates
            .Where(x => x.UserId == userId);

        if (request.LocationId.HasValue)
        {
            query = query.Where(x => x.LocationId == request.LocationId.Value);
        }

        return await query
            .OrderByDescending(x => x.LastModified)
            .ProjectTo<WorkoutTemplateBriefDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
