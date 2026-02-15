using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.WorkoutTemplates.Commands.UpdateWorkoutTemplate;

public record UpdateWorkoutTemplateCommand : IRequest
{
    public int Id { get; init; }

    public string? Name { get; init; }

    public string? Notes { get; init; }

    public string? Location { get; init; }
}

public class UpdateWorkoutTemplateCommandHandler : IRequestHandler<UpdateWorkoutTemplateCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateWorkoutTemplateCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateWorkoutTemplateCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        var entity = await _context.WorkoutTemplates
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == userId, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name!;
        entity.Notes = request.Notes;
        entity.Location = request.Location;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
