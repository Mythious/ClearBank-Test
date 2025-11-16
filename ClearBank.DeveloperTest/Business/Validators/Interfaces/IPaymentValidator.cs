using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Business.Validators.Interfaces;

public interface IPaymentValidator
{
    bool CanValidate(PaymentScheme scheme);
    MakePaymentResult Validate(Account? account, MakePaymentRequest request);
}