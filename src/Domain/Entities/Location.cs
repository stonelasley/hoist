namespace Hoist.Domain.Entities;

public class Location : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string? InstagramHandle { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? Notes { get; set; }

    public string? Address { get; set; }

    public string UserId { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
}
