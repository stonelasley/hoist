using Hoist.Application.Common.Interfaces;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;

namespace Hoist.Application.ExerciseTemplates.Commands.CreateExerciseTemplate;

public record CreateExerciseTemplateCommand : IRequest<int>
{
    public string? Name { get; init; }

    public ImplementType ImplementType { get; init; }

    public ExerciseType ExerciseType { get; init; }

    public string? Model { get; init; }

    public int? LocationId { get; init; }
}

public class CreateExerciseTemplateCommandHandler : IRequestHandler<CreateExerciseTemplateCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateExerciseTemplateCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<int> Handle(CreateExerciseTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = new ExerciseTemplate
        {
            Name = request.Name!,
            ImplementType = request.ImplementType,
            ExerciseType = request.ExerciseType,
            Model = request.Model,
            UserId = _user.Id!,
            LocationId = request.LocationId
        };

        _context.ExerciseTemplates.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
