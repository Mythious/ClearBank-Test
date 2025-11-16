using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Business.Repositories.Interfaces.Base;

/// <summary>
/// Base interface for all account repositories.
/// </summary>
public interface IBaseAccountRepository
{
    public Account GetAccount(string accountNumber);
    
    public void UpdateAccount(Account account);
}