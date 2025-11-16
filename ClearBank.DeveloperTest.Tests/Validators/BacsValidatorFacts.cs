using ClearBank.DeveloperTest.Business.Validators;
using ClearBank.DeveloperTest.Business.Validators.Interfaces;
using ClearBank.DeveloperTest.Types;
using Shouldly;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Validators;

public class BacsValidatorFacts
{
    [Theory]
    [InlineData(PaymentScheme.Bacs)]
    [InlineData(PaymentScheme.Chaps)]
    [InlineData(PaymentScheme.FasterPayments)]
    
    public void BacsValidator_IfPaymentSchemeIsBacs_ReturnsTrue(PaymentScheme scheme)
    {
        // Arrange
        var validator = CreateValidator();
        
        // Act
        var schemeResult = validator.CanValidate(scheme);

        // Assert
        if (scheme == PaymentScheme.Bacs)
        {
            schemeResult.ShouldBeTrue();
        }
        else
        {
            schemeResult.ShouldBeFalse();
        }
    }

    [Fact]
    public void BacsValidator_IfAllowedPaymentSchemeIsNotBacs_ReturnsFalse()
    {
        // Arrange
        var testAccount = new Account { AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps };
        var testRequest = new MakePaymentRequest { PaymentScheme = PaymentScheme.Bacs };
        var validator = CreateValidator();
        
        // Act
        var schemeResult = validator.Validate(testAccount, testRequest);
        
        // Assert
        schemeResult.Success.ShouldBeFalse();
    }
    
    [Fact]
    public void BacsValidator_IfAccountIsNull_ReturnsFalse()
    {
        // Arrange
        var testRequest = new MakePaymentRequest { PaymentScheme = PaymentScheme.Bacs };
        var validator = CreateValidator();
        
        // Act
        var schemeResult = validator.Validate(null, testRequest);
        
        // Assert
        schemeResult.Success.ShouldBeFalse();
    }

    [Fact]
    public void BacsValidator_IfAccountIsNotNullAndAllowedPaymentSchemeIsBacs_ReturnsTrue()
    {
        // Arrange
        var testAccount = new Account { AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs };
        var testRequest = new MakePaymentRequest { PaymentScheme = PaymentScheme.Bacs };
        var validator = CreateValidator();
        
        // Act
        var schemeResult = validator.Validate(testAccount, testRequest);
        
        // Assert
        schemeResult.Success.ShouldBeTrue();
    }

    private IPaymentValidator CreateValidator()
    {
        return new BacsValidator();
    }
}