using ClearBank.DeveloperTest.Business.Validators.Interfaces;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Business.Validators;

public class ChapsValidator : IPaymentValidator
{
    public bool CanValidate(PaymentScheme scheme) => scheme == PaymentScheme.Chaps;

    public MakePaymentResult Validate(Account account, MakePaymentRequest request)
    {
        var result = new MakePaymentResult { Success = true };
        
        if (account == null || !account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps) || account.Status != AccountStatus.Live)
        {
            result.Success = false;
        }
        
        return result;
    }
}