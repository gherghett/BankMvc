using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankMvcEf.Models;
using BankMvcEf.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BankMvcEf.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace BankMvcEf.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IUserService _userService;
    private readonly IAccountService _accountService;

    public AccountController
    (
        ILogger<AccountController> logger,
        ApplicationDbContext dbContext,
        IUserService userService,
        IAccountService accountService
    )
    {
        _dbContext = dbContext;
        _logger = logger;
        _userService = userService;
        _accountService = accountService;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userResult = await _userService.GetUserWithAccountsAsync(User);

        if (!userResult.WasSuccess)
            throw new Exception("couldnt load user");

        return View(userResult.Data);
    }

    [Authorize]
    public async Task<IActionResult> Delete(int accountId)
    {
        var userResult = await _userService.GetUserWithAccountsAsync(User);
        if (!userResult.WasSuccess)
                throw new Exception("couldnt load user");
        var user  = userResult.Data;

        var result = _accountService.Close(user.Id, accountId);

        TempData["Alert"] = result.Message;
        return RedirectToAction("Index");
    }

    [Authorize]
    public async Task<IActionResult> Add()
    {
        var userResult = await _userService.GetUserWithAccountsAsync(User);

        if (!userResult.WasSuccess)
            throw new Exception("couldnt load user");
        
        var user  = userResult.Data;

        var result = _accountService.New(user.Id);
        
        TempData["Alert"] = result.Message;
        return RedirectToAction("Index");
    }

    [Authorize]
    public async Task<IActionResult> Details(int accountId)
    {
        var userResult = await _userService.GetUserWithAccountsAsync(User);

        if (!userResult.WasSuccess)
            throw new Exception("couldnt load user");
        
        var user  = userResult.Data;

        if (!_userService.IsOwner(user.Id, accountId))
        {
            return RedirectToAction("Index");
        }

        var result = _accountService.GetAccountWithHistory(accountId);
        if(!result.WasSuccess)
        {
            TempData["Alert"] = result.Message;
            return RedirectToAction("Index");
        }

        return View(result.Data);
    }

    [Authorize]
    public async Task<IActionResult> Transfer()
    {
        var userResult = await _userService.GetUserWithAccountsAsync(User);

        if (!userResult.WasSuccess)
            throw new Exception("couldnt load user");
        
        var user  = userResult.Data;

        return View(user.Accounts.ToList());
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Transfer(int fromAccount, int toAccount,
        string otherAccountNumber, decimal amount)
    {
        var userResult = await _userService.GetUserWithAccountsAsync(User);

        if (!userResult.WasSuccess)
            throw new Exception("couldnt load user");
        
        var user  = userResult.Data;

        // this value being 0 means the account number was manually filled in
        // so we read the manual field
        if(toAccount == 0)
        {
            if(!int.TryParse(otherAccountNumber, out toAccount))
            {   
                TempData["Alert"] = "Invalid account Id";
                return RedirectToAction("Index");
            }
        }

        _accountService.Transfer(user.Id, fromAccount, toAccount, amount);
        
        return RedirectToAction("Index");
    }
    private Account GetAccountById(int accountId) =>
        _dbContext.Accounts
            .Where(acc => acc.Id == accountId)
            .SingleOrDefault() ?? throw new Exception("Account cant be found");
    private (string id, ApplicationUser user) GetIdAndUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new Exception("Could not log in! cant find user");
        var user = _dbContext.Users.Where(u => userId == u.Id).Include(u => u.Accounts).SingleOrDefault()
            ?? throw new Exception("Could not log in! Cant load user!");
        return (userId, user);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
