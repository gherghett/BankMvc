using MvcWithIdentityAndEFCore.Data;
using MvcWithIdentityAndEFCore.Models;
using MvcWithIdentityAndEFCore.Services;

public interface IAccountService
{
    ServiceResult<Transaction> Transfer(string userId ,int fromAccId, int toAccId, decimal amount);
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