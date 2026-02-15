using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.Locations.Commands.DeleteLocation;

public record DeleteLocationCommand(int Id) : IRequest;

public class DeleteLocationCommandHandler : IRequestHandler<DeleteLocationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public DeleteLocationCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        var entity = await _context.Locations
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == userId, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
