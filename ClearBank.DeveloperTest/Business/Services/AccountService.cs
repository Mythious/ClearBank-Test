using ClearBank.DeveloperTest.Business.Services.Interfaces;
using ClearBank.DeveloperTest.Data.Configuration.Interfaces;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Business.Services;

/// <summary>
/// Service responsible for applying payments to accounts.
/// Ensures that the account balance is updated and changes
/// are persisted to both primary and secondary data stores if available.
/// </summary>
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