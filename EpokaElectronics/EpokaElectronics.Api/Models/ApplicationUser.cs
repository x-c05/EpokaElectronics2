using Microsoft.AspNetCore.Identity;

namespace EpokaElectronics.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = "";
}
