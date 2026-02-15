using Hoist.Application.Common.Interfaces;
using Hoist.Domain.Entities;

namespace Hoist.Application.WorkoutTemplates.Commands.CreateWorkoutTemplate;

public record CreateWorkoutTemplateCommand : IRequest<int>
{
    public string? Name { get; init; }

    public string? Notes { get; init; }

    public int? LocationId { get; init; }
}

public class CreateWorkoutTemplateCommandHandler : IRequestHandler<CreateWorkoutTemplateCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateWorkoutTemplateCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<int> Handle(CreateWorkoutTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = new WorkoutTemplate
        {
            Name = request.Name!,
            Notes = request.Notes,
            LocationId = request.LocationId,
            UserId = _user.Id!
        };

        _context.WorkoutTemplates.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
