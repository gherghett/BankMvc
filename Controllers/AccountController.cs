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
            if (user is null)
            {
                throw new Exception("Couldnt find user");
            }
            return View(user.Accounts.ToList());
        }
        else
            return RedirectToAction("Login", "Account");
    }

    [Authorize]
    public async Task<IActionResult> Delete(int accountId)
    {
        if (User?.Identity?.IsAuthenticated ?? false)
        {
            // Get the user
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = _dbContext.Users.Where(u => userId == u.Id).Include(u => u.Accounts).SingleOrDefault();

            if (user is null)
            {
                throw new Exception("Couldnt find user");
            }

            // if the user the owner
            if (OwnsAccount(user, accountId))
            {
                _dbContext
                .Remove(
                    user
                    .Accounts
                        .Where(acc => acc.Id == accountId)
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
            var user = _dbContext.Users.Where(u => userId == u.Id).Include(u => u.Accounts).SingleOrDefault();

            if (user is null || userId is null)
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
                .Where(acc => acc.ApplicationUser.Id == userId).ToList();
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

    [Authorize]
    public IActionResult Details(int accountId)
    {
        if (User?.Identity?.IsAuthenticated ?? false)
        {
            (var userId, var user) = GetUser();

            if (user is null || userId is null)
            {
                throw new Exception("Couldnt find user");
            }
            if (OwnsAccount(user, accountId))
            {

                return View(GetAccountById(accountId));

            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        else
        {
            return RedirectToAction("Login", "Account");
        }
    }

    [Authorize]
    [HttpPost]
    public IActionResult Transfer(int fromAccount, int toAccount,
        string otherAccountNumber, decimal amount)
    {
        if (User?.Identity?.IsAuthenticated ?? false)
        {
            //needs verification
            int to = 0;
            (var userId, var user) = GetUser();
            if (!OwnsAccount(user, fromAccount))
            {
                throw new Exception("User cant send from others account");
            }
            if (toAccount == 0)
            {
                //sendin to external account
                if (int.TryParse(otherAccountNumber, out int toOtherAccount))
                {
                    to = toOtherAccount;
                }
                else
                {
                    RedirectToAction("Index"); //not valid number
                }
            }
            else
            {
                to = toAccount;
            }

            //Do the transfer
            Account fromAccountObject = GetAccountById(fromAccount);
            Account toAccountObject = GetAccountById(to);
            fromAccountObject.Balance -= amount;
            toAccountObject.Balance += amount;
            _dbContext.SaveChanges();
        }
        return RedirectToAction("Index");
    }
    private bool OwnsAccount(ApplicationUser user, int accountID) =>
        _dbContext.Accounts
            .Where(acc => acc.Id == accountID && acc.ApplicationUserId == user.Id)
            .Any();
    private Account GetAccountById(int accountId) =>
        _dbContext.Accounts
            .Where(acc => acc.Id == accountId)
            .SingleOrDefault() ?? throw new Exception("Account cant be found");
    private (string id, ApplicationUser user) GetUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new Exception("Could not log in! cant find user");
        var user = _dbContext.Users.Where(u => userId == u.Id).Include(u => u.Accounts).SingleOrDefault()
            ?? throw new Exception("Could not log in! Cant load user!");
        return (userId, user);
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
