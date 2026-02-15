using Hoist.Application.Common.Interfaces;
using Hoist.Domain.Entities;

namespace Hoist.Application.Locations.Commands.CreateLocation;

public record CreateLocationCommand : IRequest<int>
{
    public string? Name { get; init; }

    public string? InstagramHandle { get; init; }

    public decimal? Latitude { get; init; }

    public decimal? Longitude { get; init; }

    public string? Notes { get; init; }

    public string? Address { get; init; }
}

public class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateLocationCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<int> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        var instagramHandle = request.InstagramHandle;
        if (!string.IsNullOrWhiteSpace(instagramHandle) && instagramHandle.StartsWith('@'))
        {
            instagramHandle = instagramHandle.Substring(1);
        }

        var entity = new Location
        {
            Name = request.Name!,
            InstagramHandle = instagramHandle,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Notes = request.Notes,
            Address = request.Address,
            UserId = _user.Id!
        };

        _context.Locations.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
