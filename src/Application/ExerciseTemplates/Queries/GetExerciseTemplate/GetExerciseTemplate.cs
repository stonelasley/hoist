using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.ExerciseTemplates.Queries.GetExerciseTemplate;

public record GetExerciseTemplateQuery(int Id) : IRequest<ExerciseTemplateDetailDto>;

public class GetExerciseTemplateQueryHandler : IRequestHandler<GetExerciseTemplateQuery, ExerciseTemplateDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;

    public GetExerciseTemplateQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
    }

    public async Task<ExerciseTemplateDetailDto> Handle(GetExerciseTemplateQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        var result = await _context.ExerciseTemplates
            .Where(x => x.Id == request.Id && x.UserId == userId)
            .ProjectTo<ExerciseTemplateDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, result);

        return result;
    }
}
