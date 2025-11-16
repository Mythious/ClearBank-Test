using ClearBank.DeveloperTest.Business.Validators.Interfaces;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Business.Validators;

public class FasterPaymentsValidator : IPaymentValidator
{
    public bool CanValidate(PaymentScheme scheme) => scheme == PaymentScheme.FasterPayments;

    public MakePaymentResult Validate(Account account, MakePaymentRequest request)
    {
        var result = new MakePaymentResult { Success = true };
        
        if (account == null || !account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments) || account.Balance < request.Amount)
        {
            result.Success = false;
        }
        
        return result;
    }
}