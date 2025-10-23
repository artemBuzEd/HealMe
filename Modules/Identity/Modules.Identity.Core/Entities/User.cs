using Microsoft.AspNetCore.Identity;
using Modules.Identity.Core.Entities.Enums;

namespace Modules.Identity.Core.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}