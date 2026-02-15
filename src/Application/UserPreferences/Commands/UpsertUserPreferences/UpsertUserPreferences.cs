using Hoist.Application.Common.Interfaces;
using Hoist.Domain.Enums;

namespace Hoist.Application.UserPreferences.Commands.UpsertUserPreferences;

public record UpsertUserPreferencesCommand : IRequest
{
    public string WeightUnit { get; init; } = string.Empty;

    public string DistanceUnit { get; init; } = string.Empty;

    public decimal? Bodyweight { get; init; }
}

public class UpsertUserPreferencesCommandHandler : IRequestHandler<UpsertUserPreferencesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpsertUserPreferencesCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpsertUserPreferencesCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id!;

        // Parse enum values
        var weightUnit = Enum.Parse<WeightUnit>(request.WeightUnit);
        var distanceUnit = Enum.Parse<DistanceUnit>(request.DistanceUnit);

        // Find existing preferences
        var preferences = await _context.UserPreferences
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (preferences != null)
        {
            // Update existing
            preferences.WeightUnit = weightUnit;
            preferences.DistanceUnit = distanceUnit;
            preferences.Bodyweight = request.Bodyweight;
        }
        else
        {
            // Create new
            preferences = new Domain.Entities.UserPreferences
            {
                UserId = userId,
                WeightUnit = weightUnit,
                DistanceUnit = distanceUnit,
                Bodyweight = request.Bodyweight
            };

            _context.UserPreferences.Add(preferences);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
