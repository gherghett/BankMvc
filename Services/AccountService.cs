using MvcWithIdentityAndEFCore.Data;
using MvcWithIdentityAndEFCore.Models;
using MvcWithIdentityAndEFCore.Services;

public interface IAccountService
{
    ServiceResult<Transaction> Transfer(string userId ,int fromAccId, int toAccId, decimal amount);
    ServiceResult<Account> Close(string userId, int accId);
}

public class AccountService : IAccountService
{
    private ApplicationDbContext _dbContext;
    private IUserService _userService;
    public AccountService(ApplicationDbContext context, IUserService userService)
    {
        _dbContext = context;
        _userService = userService;
        //add logger
    }

    public ServiceResult<Account> Close(string userId, int accId)
    {
        if(!_userService.IsOwner(userId, accId))
        {
            return ServiceResult<Account>.Failed("not owner");
        }

        Account account = GetAccountById(accId);

        if(account.Balance > 0)
        {
            return ServiceResult<Account>.Failed("Can't close account with money in it.");
        }

        account.Closed = true;
        _dbContext.SaveChanges();

        return ServiceResult<Account>.Succeeded(account, "The Account was closed");

    }

    public ServiceResult<Transaction> Transfer(string userId, int fromAccId, int toAccId, decimal amount)
    {
        if(!_userService.IsOwner(userId, fromAccId))
        {
            return ServiceResult<Transaction>.Failed("sender is not owner");
        }
        Account fromAccount = GetAccountById(fromAccId);

        if(fromAccount.Balance < amount)
        {
            return ServiceResult<Transaction>.Failed("amount is larger than balance");
        }

        //Do the transfer
        Account toAccount = GetAccountById(toAccId);
        Transaction transaction = new Transaction{
            FromAcc = fromAccount,
            ToAcc = toAccount,
            Time = DateTime.Now,
            Amount = amount
        };
        fromAccount.Balance -= amount;
        toAccount.Balance += amount;
        _dbContext.Add(transaction);
        _dbContext.SaveChanges();


        return ServiceResult<Transaction>.Succeeded(new Transaction());
    }
    private Account GetAccountById(int accountId) =>
        _dbContext.Accounts
            .Where(acc => acc.Id == accountId)
            .SingleOrDefault() ?? throw new Exception("Account does not exist");
}