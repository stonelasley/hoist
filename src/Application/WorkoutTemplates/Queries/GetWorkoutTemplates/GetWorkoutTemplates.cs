using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.WorkoutTemplates.Queries.GetWorkoutTemplates;

public record GetWorkoutTemplatesQuery : IRequest<List<WorkoutTemplateBriefDto>>;

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

        return await _context.WorkoutTemplates
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.LastModified)
            .ProjectTo<WorkoutTemplateBriefDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
