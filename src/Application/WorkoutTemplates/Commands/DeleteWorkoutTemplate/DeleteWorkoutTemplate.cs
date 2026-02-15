using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.WorkoutTemplates.Commands.DeleteWorkoutTemplate;

public record DeleteWorkoutTemplateCommand(int Id) : IRequest;

public class DeleteWorkoutTemplateCommandHandler : IRequestHandler<DeleteWorkoutTemplateCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public DeleteWorkoutTemplateCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(DeleteWorkoutTemplateCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        var entity = await _context.WorkoutTemplates
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == userId, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.WorkoutTemplates.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
