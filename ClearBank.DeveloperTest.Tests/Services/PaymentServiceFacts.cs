using System;
using ClearBank.DeveloperTest.Business.Repositories.Interfaces;
using ClearBank.DeveloperTest.Business.Repositories.Interfaces.Base;
using ClearBank.DeveloperTest.Business.Services;
using ClearBank.DeveloperTest.Business.Services.Interfaces;
using ClearBank.DeveloperTest.Business.Validators;
using ClearBank.DeveloperTest.Business.Validators.Interfaces;
using ClearBank.DeveloperTest.Data.Configuration.Interfaces;
using ClearBank.DeveloperTest.Types;
using Moq;
using Shouldly;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Services;

public class PaymentServiceFacts
{
    private readonly Mock<IAccountRepository> _mockAccountRepository = new();
    private readonly Mock<IBackupAccountRepository> _mockBackupAccountRepository = new();
    private readonly Mock<IDataStoreSelector> _mockDataStoreSelector = new();
    private readonly Mock<IAccountService> _mockAccountService = new();
    
    private readonly DateTime _testPaymentDate = DateTime.FromOADate(45735.0);
    
    [Theory]
    [InlineData(PaymentScheme.Bacs)]
    [InlineData(PaymentScheme.FasterPayments)]
    [InlineData(PaymentScheme.Chaps)]
    public void MakePayment_WhenAccountDoesNotExist_ReturnsFalse(PaymentScheme paymentScheme)
    {
        // Arrange
        SeedPrimaryAccount("TestAccount", 1000, AccountStatus.Live, AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.Chaps);
        var service = CreateService();
        
        // Act
        var result = service.MakePayment(new MakePaymentRequest
        {
            DebtorAccountNumber = "FakeAccount",
            CreditorAccountNumber = "FakeCreditor",
            Amount = 1000,
            PaymentScheme = paymentScheme,
            PaymentDate = _testPaymentDate
        });

        // Assert
        result.Success.ShouldBeFalse();
    }

    [Theory]
    [InlineData(PaymentScheme.Bacs)]
    [InlineData(PaymentScheme.FasterPayments)]
    [InlineData(PaymentScheme.Chaps)]
    public void MakePayment_WhenAccountIsLiveAndPaymentIsSuccessful_ReturnsTrue(PaymentScheme paymentScheme)
    {
        // Arrange
        const string accountNumber = "TestAccount";
        const int initialBalance = 1000;
        const int paymentAmount = 100;
        SeedPrimaryAndSecondaryAccounts(accountNumber, initialBalance, AccountStatus.Live, AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps);
        
        var service = CreateService();
        var request = new MakePaymentRequest
        {
            DebtorAccountNumber = accountNumber,
            CreditorAccountNumber = "CreditorAccount",
            Amount = paymentAmount,
            PaymentScheme = paymentScheme,
            PaymentDate = _testPaymentDate
        };

        // Act
        var result = service.MakePayment(request);

        // Assert
        result.Success.ShouldBeTrue();
        
        // Separate verification for backup and live accounts.
        _mockDataStoreSelector.Verify(x => x.GetPrimary().GetAccount(It.Is<string>(x => x == request.DebtorAccountNumber)), Times.Once);
        _mockAccountService.Verify(x => x.ApplyPayment(It.Is<Account>(a => a.AccountNumber == accountNumber), paymentAmount), Times.Once);
    }
    
    [Fact]
    public void MakePayment_UsesBackupRepository_WhenDataStoreTypeIsBackup()
    {
        // Arrange
        const string accountNumber = "BackupAccount";
        SeedPrimaryAccount(accountNumber, 500, AccountStatus.Live, AllowedPaymentSchemes.FasterPayments, true);

        var service = CreateService(true);
        var request = new MakePaymentRequest
        {
            DebtorAccountNumber = accountNumber,
            CreditorAccountNumber = "TestCreditorNumber",
            Amount = 100,
            PaymentScheme = PaymentScheme.FasterPayments,
            PaymentDate = _testPaymentDate
        };

        // Act
        var result = service.MakePayment(request);

        // Assert
        result.Success.ShouldBeTrue();
        
        _mockDataStoreSelector.Verify(x => x.GetPrimary().GetAccount(It.Is<string>(x => x == request.DebtorAccountNumber)), Times.Once);
        _mockAccountService.Verify(x => x.ApplyPayment(It.Is<Account>(a => a.AccountNumber == accountNumber), request.Amount), Times.Once);
    }

    [Theory]
    [InlineData(PaymentScheme.FasterPayments)]
    [InlineData(PaymentScheme.Bacs)]
    [InlineData(PaymentScheme.Chaps)]
    public void MakePayment_IfPaymentIsNotAllowed_ReturnsFalse(PaymentScheme paymentScheme)
    {
        // Arrange
        const string accountNumber = "TestAccount";
        const int initialBalance = 1000;
        const int paymentAmount = 100;
        
        // Using 0 Flag to indicate that the account is not allowed to make payments of the specified scheme.
        SeedPrimaryAndSecondaryAccounts(accountNumber, initialBalance, AccountStatus.Live, 0);
        
        var service = CreateService();
        var request = new MakePaymentRequest
        {
            DebtorAccountNumber = accountNumber,
            CreditorAccountNumber = "CreditorAccount",
            Amount = paymentAmount,
            PaymentScheme = paymentScheme,
            PaymentDate = _testPaymentDate
        };

        // Act
        var result = service.MakePayment(request);

        // Assert
        result.Success.ShouldBeFalse();
        _mockDataStoreSelector.Verify(x => x.GetPrimary().GetAccount(It.Is<string>(x => x == request.DebtorAccountNumber)), Times.Once);
    }
    
    private void SeedPrimaryAndSecondaryAccounts(string accountNumber, decimal balance, AccountStatus status, AllowedPaymentSchemes allowedPaymentSchemes, bool isBackup = false)
    {
        SeedPrimaryAccount(accountNumber, balance, status, allowedPaymentSchemes, isBackup);
        SeedSecondaryAccount(accountNumber, balance, status, allowedPaymentSchemes, isBackup);
    }

    private void SeedPrimaryAccount(string accountNumber, decimal balance, AccountStatus status,
        AllowedPaymentSchemes allowedPaymentSchemes, bool isBackup = false)
    {
        if (!isBackup)
        {
            _mockAccountRepository
                .Setup(x => x.GetAccount(It.Is<string>(s => s == accountNumber)))
                .Returns(new Account
                {
                    AccountNumber = accountNumber,
                    Balance = balance,
                    Status = status,
                    AllowedPaymentSchemes = allowedPaymentSchemes
                });
        }
        else
        {
            _mockBackupAccountRepository
                .Setup(x => x.GetAccount(It.Is<string>(s => s == accountNumber)))
                .Returns(new Account
                {
                    AccountNumber = accountNumber,
                    Balance = balance,
                    Status = status,
                    AllowedPaymentSchemes = allowedPaymentSchemes
                });
        }
    }

    private void SeedSecondaryAccount(string accountNumber, decimal balance, AccountStatus status,
        AllowedPaymentSchemes allowedPaymentSchemes, bool isBackup = false)
    {
        if (!isBackup)
        {
            _mockBackupAccountRepository
                .Setup(x => x.GetAccount(It.Is<string>(s => s == accountNumber)))
                .Returns(new Account
                {
                    AccountNumber = accountNumber,
                    Balance = balance,
                    Status = status,
                    AllowedPaymentSchemes = allowedPaymentSchemes
                });
        }
    }
    
    private IPaymentService CreateService(bool useBackupDataStore = false)
    {
        var validators = new IPaymentValidator[]
        {
            new BacsValidator(),
            new FasterPaymentsValidator(),
            new ChapsValidator()
        };

        if (useBackupDataStore)
        {
            // Replicates if the main data store is backup.
            _mockDataStoreSelector.Setup(s => s.GetPrimary()).Returns(_mockBackupAccountRepository.Object);
            _mockDataStoreSelector.Setup(s => s.GetSecondary()).Returns((IBaseAccountRepository)null);
        }
        else
        {
            // Replicates if the main data store is live.
            _mockDataStoreSelector.Setup(s => s.GetPrimary()).Returns(_mockAccountRepository.Object);
            _mockDataStoreSelector.Setup(s => s.GetSecondary()).Returns(_mockBackupAccountRepository.Object);
        }

        
        return new PaymentService(
            _mockAccountRepository.Object, 
            _mockBackupAccountRepository.Object,
            validators,
            _mockDataStoreSelector.Object,
            _mockAccountService.Object);
    }
}