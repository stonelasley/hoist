using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.ExerciseTemplates.Commands.DeleteExerciseTemplate;

public record DeleteExerciseTemplateCommand(int Id) : IRequest;

public class DeleteExerciseTemplateCommandHandler : IRequestHandler<DeleteExerciseTemplateCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public DeleteExerciseTemplateCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(DeleteExerciseTemplateCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        var entity = await _context.ExerciseTemplates
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == userId, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
