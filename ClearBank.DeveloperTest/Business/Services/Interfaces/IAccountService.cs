using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services;

public interface IAccountService
{
    void ApplyPayment(Account accountNumber, decimal amount);
}