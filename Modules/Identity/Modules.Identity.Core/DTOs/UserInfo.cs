using Modules.Identity.Core.Entities.Enums;

namespace Modules.Identity.Core.DTOs;

public class UserInfo
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public Gender Gender { get; set; }
    public IList<string> Roles { get; set; }
}