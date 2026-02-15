using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.Locations.Commands.UpdateLocation;

public record UpdateLocationCommand : IRequest
{
    public int Id { get; init; }

    public string? Name { get; init; }

    public string? InstagramHandle { get; init; }

    public decimal? Latitude { get; init; }

    public decimal? Longitude { get; init; }

    public string? Notes { get; init; }

    public string? Address { get; init; }
}

public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateLocationCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        var entity = await _context.Locations
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == userId, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        var instagramHandle = request.InstagramHandle;
        if (!string.IsNullOrWhiteSpace(instagramHandle) && instagramHandle.StartsWith('@'))
        {
            instagramHandle = instagramHandle.Substring(1);
        }

        entity.Name = request.Name!;
        entity.InstagramHandle = instagramHandle;
        entity.Latitude = request.Latitude;
        entity.Longitude = request.Longitude;
        entity.Notes = request.Notes;
        entity.Address = request.Address;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
