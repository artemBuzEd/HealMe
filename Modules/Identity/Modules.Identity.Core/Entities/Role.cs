using Microsoft.AspNetCore.Identity;

namespace Modules.Identity.Core.Entities;

public class Role : IdentityRole
{
    public string Description {get;set;} = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}