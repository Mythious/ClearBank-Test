using ClearBank.DeveloperTest.Business.Validators;
using ClearBank.DeveloperTest.Business.Validators.Interfaces;
using ClearBank.DeveloperTest.Types;
using Shouldly;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Validators;

public class FasterPaymentsValidatorFacts
{
    [Theory]
    [InlineData(PaymentScheme.Bacs)]
    [InlineData(PaymentScheme.Chaps)]
    [InlineData(PaymentScheme.FasterPayments)]
    
    public void FpValidator_IfPaymentSchemeIsFp_ReturnsTrue(PaymentScheme scheme)
    {
        // Arrange
        var validator = CreateValidator();
        
        // Act
        var schemeResult = validator.CanValidate(scheme);

        // Assert
        if (scheme == PaymentScheme.FasterPayments)
        {
            schemeResult.ShouldBeTrue();
        }
        else
        {
            schemeResult.ShouldBeFalse();
        }
    }

    [Fact]
    public void FpValidator_IfAllowedPaymentSchemeIsNotFp_ReturnsFalse()
    {
        // Arrange
        var testAccount = new Account { AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs };
        var testRequest = new MakePaymentRequest { PaymentScheme = PaymentScheme.FasterPayments };
        var validator = CreateValidator();
        
        // Act
        var schemeResult = validator.Validate(testAccount, testRequest);
        
        // Assert
        schemeResult.Success.ShouldBeFalse();
    }
    
    [Fact]
    public void FpValidator_IfAccountIsNull_ReturnsFalse()
    {
        // Arrange
        var testRequest = new MakePaymentRequest { PaymentScheme = PaymentScheme.FasterPayments };
        var validator = CreateValidator();
        
        // Act
        var schemeResult = validator.Validate(null, testRequest);
        
        // Assert
        schemeResult.Success.ShouldBeFalse();
    }

    [Fact]
    public void FpValidator_IfAccountIsNotNullAndAllowedPaymentSchemeIsFp_ReturnsTrue()
    {
        // Arrange
        var testAccount = new Account { AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments };
        var testRequest = new MakePaymentRequest { PaymentScheme = PaymentScheme.FasterPayments };
        var validator = CreateValidator();
        
        // Act
        var schemeResult = validator.Validate(testAccount, testRequest);
        
        // Assert
        schemeResult.Success.ShouldBeTrue();
    }

    private IPaymentValidator CreateValidator()
    {
        return new FasterPaymentsValidator();
    }
}