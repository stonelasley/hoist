using Microsoft.AspNetCore.Identity;

namespace Hoist.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public int? Age { get; set; }
}
