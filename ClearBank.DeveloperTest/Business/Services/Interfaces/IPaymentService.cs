using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Business.Services.Interfaces
{
    /// <summary>
    /// Provides interface for the payment service.
    /// </summary>
    public interface IPaymentService
    {
        MakePaymentResult MakePayment(MakePaymentRequest request);
    }
}
