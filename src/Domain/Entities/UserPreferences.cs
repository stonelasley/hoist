namespace Hoist.Domain.Entities;

public class UserPreferences : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;

    public WeightUnit WeightUnit { get; set; }

    public DistanceUnit DistanceUnit { get; set; }

    public decimal? Bodyweight { get; set; }
}
