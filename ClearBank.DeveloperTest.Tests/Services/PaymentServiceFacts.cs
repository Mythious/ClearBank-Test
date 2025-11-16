using System;
using System.Configuration;
using ClearBank.DeveloperTest.Business.Repositories.Interfaces;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Types;
using Moq;
using Shouldly;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Services;

public class PaymentServiceFacts
{
    private readonly Mock<IAccountRepository> _mockAccountRepository = new();
    private readonly Mock<IBackupAccountRepository> _mockBackupAccountRepository = new();
    
    private readonly DateTime _testPaymentDate = DateTime.FromOADate(45735.0);
    
    public PaymentServiceFacts()
    {
        // Override the data store type for testing purposes. 
        // Todo - Remove & Refactor once the test suite is complete.
        ConfigurationManager.AppSettings["DataStoreType"] = "Live";
    }
    
    [Theory]
    [InlineData(PaymentScheme.Bacs)]
    [InlineData(PaymentScheme.FasterPayments)]
    [InlineData(PaymentScheme.Chaps)]
    public void MakePayment_WhenAccountDoesNotExist_ReturnsFalse(PaymentScheme paymentScheme)
    {
        // Arrange
        SeedLiveAccount("TestAccount", 1000, AccountStatus.Live, AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.Chaps);
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
        SeedLiveAndBackupAccounts(accountNumber, initialBalance, AccountStatus.Live, AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps);
        
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
        if (ConfigurationManager.AppSettings["DataStoreType"] == "Backup")
        {
            _mockBackupAccountRepository.Verify(x => x.UpdateAccount(It.Is<Account>(a =>
                a.AccountNumber == accountNumber &&
                a.Balance == initialBalance - paymentAmount)), Times.Once);
        }
        else
        {
            _mockAccountRepository.Verify(x => x.UpdateAccount(It.Is<Account>(a =>
                a.AccountNumber == accountNumber &&
                a.Balance == initialBalance - paymentAmount)), Times.Once);
        }
    }
    
    [Fact]
    public void MakePayment_UsesBackupRepository_WhenDataStoreTypeIsBackup()
    {
        // Arrange
        ConfigurationManager.AppSettings["DataStoreType"] = "Backup";
        const string accountNumber = "BackupAccount";
        SeedBackupAccount(accountNumber, 500, AccountStatus.Live, AllowedPaymentSchemes.FasterPayments);

        var service = CreateService();
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
        _mockBackupAccountRepository.Verify(x => x.UpdateAccount(It.Is<Account>(a => a.AccountNumber == accountNumber && a.Balance == 400)), Times.Once);
        _mockAccountRepository.Verify(x => x.UpdateAccount(It.IsAny<Account>()), Times.Never);
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
        SeedLiveAndBackupAccounts(accountNumber, initialBalance, AccountStatus.Live, 0);
        
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
        
        // Separate verification for backup and live accounts.
        if (ConfigurationManager.AppSettings["DataStoreType"] == "Backup")
        {
            _mockBackupAccountRepository.Verify(x => x.UpdateAccount(It.Is<Account>(a =>
                a.AccountNumber == accountNumber &&
                a.Balance == initialBalance - paymentAmount)), Times.Never);
        }
        else
        {
            _mockAccountRepository.Verify(x => x.UpdateAccount(It.Is<Account>(a =>
                a.AccountNumber == accountNumber &&
                a.Balance == initialBalance - paymentAmount)), Times.Never);
        }
    }
    
    private void SeedLiveAccount(string accountNumber, decimal balance, AccountStatus status, AllowedPaymentSchemes allowedPaymentSchemes)
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

    private void SeedBackupAccount(string accountNumber, decimal balance, AccountStatus status, AllowedPaymentSchemes allowedPaymentSchemes)
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

    private void SeedLiveAndBackupAccounts(string accountNumber, decimal balance, AccountStatus status, AllowedPaymentSchemes allowedPaymentSchemes)
    {
        SeedLiveAccount(accountNumber, balance, status, allowedPaymentSchemes);
        SeedBackupAccount(accountNumber, balance, status, allowedPaymentSchemes);
    }
    
    private IPaymentService CreateService()
    {
        
        return new PaymentService(_mockAccountRepository.Object, _mockBackupAccountRepository.Object);
    }
}