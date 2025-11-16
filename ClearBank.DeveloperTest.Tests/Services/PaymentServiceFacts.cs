using System.Configuration;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Types;
using Shouldly;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Services;

public class PaymentServiceFacts
{
    public PaymentServiceFacts()
    {
        // Override the data store type for testing purposes. 
        // Todo - Remove once the test suite is complete.
        ConfigurationManager.AppSettings["DataStoreType"] = "Live";
    }
    
    [Fact]
    public void MakePayment_WhenAccountDoesNotExist_ReturnsFalse()
    {
        // Todo Temporary implementation to ensure the nuget package is working.
        // Arrange
        var service = CreateService();
        
        // Act
        var result = service.MakePayment(new MakePaymentRequest
        {
            DebtorAccountNumber = "FAKEACCOUNT",
            CreditorAccountNumber = "FAKECREDITOR",
            Amount = 1000
        });

        // Assert
        result.Success.ShouldBeFalse();
    }
    
    private IPaymentService CreateService()
    {
        return new PaymentService();
    }
}