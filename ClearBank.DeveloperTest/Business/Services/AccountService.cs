using ClearBank.DeveloperTest.Data.Configuration.Interfaces;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services;

public class AccountService : IAccountService
{
    private readonly IDataStoreSelector _dataStoreSelector;

    public AccountService(IDataStoreSelector dataStoreSelector)
    {
        _dataStoreSelector = dataStoreSelector;
    }

    public void ApplyPayment(Account account, decimal amount)
    {
        // Adjust balance amount
        account.Balance -= amount;
        
        // Update account in both databases if they exist
        _dataStoreSelector.GetPrimary()?.UpdateAccount(account);
        _dataStoreSelector.GetSecondary()?.UpdateAccount(account);
    }
}