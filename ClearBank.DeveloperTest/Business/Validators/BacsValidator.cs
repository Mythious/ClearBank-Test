using ClearBank.DeveloperTest.Business.Validators.Interfaces;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Business.Validators;

public class BacsValidator : IPaymentValidator
{
    public bool CanValidate(PaymentScheme scheme) => scheme == PaymentScheme.Bacs;

    public MakePaymentResult Validate(Account account, MakePaymentRequest request)
    {
        var result = new MakePaymentResult { Success = true };
        
        if (account == null || !account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs))
        {
            result.Success = false;
        }
        
        return result;
    }
}