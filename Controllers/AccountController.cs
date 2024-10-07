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

    // GET: /Account
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userResult = await _userService.GetUserWithAccountsAsync(User);

        if (!userResult.WasSuccess)
            throw new Exception("couldnt load user");

        return View(userResult.Data);
    }

    // GET: /Account/Delete?accountId={accountId}
    // Redirects to GET: /Account
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

    // GET: /Account/Add
    // Redirects to GET: /Account
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

    // GET: /Account/Details?accountId={accountId}
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

    // GET: /Account/Transfer
    [Authorize]
    public async Task<IActionResult> Transfer()
    {
        var userResult = await _userService.GetUserWithAccountsAsync(User);

        if (!userResult.WasSuccess)
            throw new Exception("couldnt load user");
        
        var user  = userResult.Data;

        return View(user.Accounts.ToList());
    }

    // POST: /Account/Transfer
    // Redirects to GET: /Account
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

    // POST: /Account/Deposit
    // Redirects to GET: /Account/Details?accountId={accountId}
    // Debug action so it doesnt use a service
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Deposit(int accountId, decimal amount)
    {
        var userResult = await _userService.GetUserWithAccountsAsync(User);

        if (!userResult.WasSuccess)
            throw new Exception("couldnt load user");
        
        var user  = userResult.Data;

        //For debug purposes
        user.Accounts
            .Where(acc => accountId == acc.Id)
            .Single()
            .Balance += amount;

        _dbContext.SaveChanges();

        return RedirectToAction("Details", new {accountId = accountId});
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
