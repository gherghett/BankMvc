using Microsoft.AspNetCore.Identity;
namespace MvcWithIdentityAndEFCore.Models;

public class ApplicationUser : IdentityUser
{
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
