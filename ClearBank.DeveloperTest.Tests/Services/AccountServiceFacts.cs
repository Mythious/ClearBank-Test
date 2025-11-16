using ClearBank.DeveloperTest.Business.Repositories.Interfaces;
using ClearBank.DeveloperTest.Business.Repositories.Interfaces.Base;
using ClearBank.DeveloperTest.Data.Configuration.Interfaces;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Types;
using Moq;
using Shouldly;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Services;

public class AccountServiceFacts
{
    private readonly Mock<IDataStoreSelector> _mockDataStoreSelector = new Mock<IDataStoreSelector>();
    private readonly Mock<IAccountRepository> _mockAccountRepository = new();
    private readonly Mock<IBackupAccountRepository> _mockBackupAccountRepository = new();
    
    [Fact]
    public void ApplyPayment_ReducesAccountBalance()
    {
        // Arrange
        _mockDataStoreSelector.Setup(s => s.GetSecondary()).Returns(_mockBackupAccountRepository.Object);
        var service = CreateService();
        var account = new Account
        {
            AccountNumber = "ACC123",
            Balance = 1000
        };
        var amount = 200;

        // Act
        service.ApplyPayment(account, amount);

        // Assert
        _mockDataStoreSelector.Verify(x => x.GetPrimary().UpdateAccount(It.Is<Account>(a => a.AccountNumber == "ACC123" && a.Balance == 800)), Times.Once);
        _mockDataStoreSelector.Verify(x => x.GetSecondary().UpdateAccount(It.Is<Account>(a => a.AccountNumber == "ACC123" && a.Balance == 800)), Times.Once);
    }

    [Fact]
    public void ApplyPayment_HandlesNullSecondaryRepository()
    {
        // Arrange
        _mockDataStoreSelector.Setup(s => s.GetSecondary()).Returns((IBaseAccountRepository)null);
        var service = CreateService();
        var account = new Account { AccountNumber = "ACC123", Balance = 500 };

        // Act
        service.ApplyPayment(account, 200);

        // Assert
        _mockDataStoreSelector.Verify(x => x.GetPrimary().UpdateAccount(It.Is<Account>(a => a.AccountNumber == "ACC123" && a.Balance == 300)), Times.Once);
        _mockDataStoreSelector.Verify(x => x.GetSecondary().UpdateAccount(It.Is<Account>(a => a.AccountNumber == "ACC123" && a.Balance == 300)), Times.Never);
    }
    
    private IAccountService CreateService(bool useBackupDataStore = false)
    {
        _mockDataStoreSelector.Setup(s => s.GetPrimary()).Returns(_mockAccountRepository.Object);
        
        return new AccountService(_mockDataStoreSelector.Object);
    }
}