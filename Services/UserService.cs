using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MvcWithIdentityAndEFCore.Data;
using MvcWithIdentityAndEFCore.Models;
namespace MvcWithIdentityAndEFCore.Services;
public interface IUserService
{
    public Task<ServiceResult<ApplicationUser>> 
        GetUserWithAccountsAsync(System.Security.Claims.ClaimsPrincipal claim);
        
    bool IsOwner(string user, int accountId);
}

public class UserService : IUserService
{
    ApplicationDbContext _applicationDbContext;

    public UserService(ApplicationDbContext context)
    {
        _applicationDbContext = context;
    }

    public async Task<ServiceResult<ApplicationUser>> 
        GetUserWithAccountsAsync(ClaimsPrincipal claim)
    {
        var userId = claim.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var user = await _applicationDbContext.Users
            .Where(u => u.Id == userId)
            .Include(u => u.Accounts.Where(a => !a.Closed))
            .SingleOrDefaultAsync();

        if( user is null )
            return ServiceResult<ApplicationUser>.Failed();
        
        return ServiceResult<ApplicationUser>.Succeeded(user);
    }

    public bool IsOwner(string userId, int accountId) =>
        _applicationDbContext.Users.Where(u => userId == u.Id)
            .Single()
            .Accounts.Any(acc => acc.Id == accountId);
}