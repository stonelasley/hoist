using Hoist.Application.Common.Interfaces;
using Hoist.Domain.Enums;

namespace Hoist.Application.ExerciseTemplates.Commands.UpdateExerciseTemplate;

public record UpdateExerciseTemplateCommand : IRequest
{
    public int Id { get; init; }

    public string? Name { get; init; }

    public ImplementType ImplementType { get; init; }

    public ExerciseType ExerciseType { get; init; }

    public string? Model { get; init; }
}

public class UpdateExerciseTemplateCommandHandler : IRequestHandler<UpdateExerciseTemplateCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateExerciseTemplateCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateExerciseTemplateCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        var entity = await _context.ExerciseTemplates
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == userId, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name!;
        entity.ImplementType = request.ImplementType;
        entity.ExerciseType = request.ExerciseType;
        entity.Model = request.Model;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
