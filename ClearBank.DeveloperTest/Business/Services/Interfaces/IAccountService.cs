using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Business.Services.Interfaces;

/// <summary>
/// Account service interface abstraction.
/// </summary>
public interface IAccountService
{
    void ApplyPayment(Account accountNumber, decimal amount);
}