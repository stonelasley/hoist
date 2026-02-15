using Hoist.Application.Common.Interfaces;
using Hoist.Domain.Enums;

namespace Hoist.Application.ExerciseTemplates.Queries.GetExerciseTemplates;

public record GetExerciseTemplatesQuery : IRequest<List<ExerciseTemplateBriefDto>>
{
    public string? Search { get; init; }

    public ImplementType? ImplementType { get; init; }

    public ExerciseType? ExerciseType { get; init; }
}

public class GetExerciseTemplatesQueryHandler : IRequestHandler<GetExerciseTemplatesQuery, List<ExerciseTemplateBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;

    public GetExerciseTemplatesQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
    }

    public async Task<List<ExerciseTemplateBriefDto>> Handle(GetExerciseTemplatesQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        var query = _context.ExerciseTemplates
            .Where(x => x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Name.Contains(search));
        }

        if (request.ImplementType.HasValue)
        {
            query = query.Where(x => x.ImplementType == request.ImplementType.Value);
        }

        if (request.ExerciseType.HasValue)
        {
            query = query.Where(x => x.ExerciseType == request.ExerciseType.Value);
        }

        return await query
            .OrderBy(x => x.Name)
            .ProjectTo<ExerciseTemplateBriefDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
