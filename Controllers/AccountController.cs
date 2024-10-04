using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcWithIdentityAndEFCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MvcWithIdentityAndEFCore.Data;


namespace MvcWithIdentityAndEFCore.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _dbContext;

    public AccountController
    (
        ILogger<HomeController> logger,
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager
    )
    {
        _dbContext = dbContext;
        _logger = logger;
    }


    [Authorize]
    public IActionResult Index()
    {
        // Check if the user is authenticated
        if (User?.Identity?.IsAuthenticated ?? false)
        {
            // Get the user ID from the ClaimsPrincipal
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = _dbContext.Users.Where(u => userId == u.Id).Include(u => u.Accounts).SingleOrDefault();
            if(user is null)
            {
                throw new Exception("Couldnt find user");
            }
            return View(user.Accounts.ToList());
        }
        else
            return RedirectToAction("Login", "Account");
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (User?.Identity?.IsAuthenticated ?? false)
        {
            // Get the user
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = _dbContext.Users.Where(u => userId == u.Id ).Include(u=>u.Accounts).SingleOrDefault();

            if(user is null)
            {
                throw new Exception("Couldnt find user");
            }
            
            // if the user the owner
            if(user.Accounts.Where(acc => acc.Id == id).Any())
            {
                _dbContext
                .Remove(
                    user
                    .Accounts
                        .Where(acc => acc.Id == id)
                        .SingleOrDefault()!);
            }
            else
            {
                // not the users account
                return RedirectToAction("Login", "Account");
            }

            // Save changes to the database
            await _dbContext.SaveChangesAsync();

            // Optionally redirect or return a view
            return RedirectToAction("Index");
        }
        else
        {
            return RedirectToAction("Login", "Account");
        }     
    }

    [Authorize]
    public async Task<IActionResult> Add()
    {
        if (User?.Identity?.IsAuthenticated ?? false)
        {
            // Get the user ID from the ClaimsPrincipal
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = _dbContext.Users.Where(u => userId == u.Id ).Include(u=>u.Accounts).SingleOrDefault();
            
            if(user is null || userId is null)
            {
                throw new Exception("Couldnt find user");
            }
            // Create a new account
            var newAccount = new Account
            {
                Balance = 0,
                ApplicationUser = user // Associate the account with the user
            };

            var myaccts = _dbContext.Accounts
                .Where(acc=>acc.ApplicationUser.Id == userId).ToList();
            // Add the account to the DbContext
            _dbContext.Accounts.Add(newAccount);

            // Save changes to the database
            await _dbContext.SaveChangesAsync();

            // Optionally redirect or return a view
            return RedirectToAction("Index");
        }
        else
        {
            return RedirectToAction("Login", "Account");
        }
    }

    // //[Authorize(Roles = "Leader")]
    // public IActionResult Account()
    // {
    //     return View();
    // }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
