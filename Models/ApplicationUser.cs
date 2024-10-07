using Microsoft.AspNetCore.Identity;
namespace BankMvcEf.Models;

public class ApplicationUser : IdentityUser
{
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
