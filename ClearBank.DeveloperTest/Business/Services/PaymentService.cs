using System.Collections.Generic;
using ClearBank.DeveloperTest.Types;
using System.Linq;
using ClearBank.DeveloperTest.Business.Repositories.Interfaces;
using ClearBank.DeveloperTest.Business.Services.Interfaces;
using ClearBank.DeveloperTest.Business.Validators.Interfaces;
using ClearBank.DeveloperTest.Data.Configuration.Interfaces;

namespace ClearBank.DeveloperTest.Business.Services
{
    /// <summary>
    /// Service responsible for processing payments between accounts.
    /// It validates the payment request using registered validators
    /// and applies the payment to the account(s) via <see cref="IAccountService"/>.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IBackupAccountRepository _backupAccountRepository;
        private readonly IEnumerable<IPaymentValidator> _validators;
        private readonly IDataStoreSelector _dataStoreSelector;
        private readonly IAccountService _accountService;

        public PaymentService(
            IAccountRepository accountRepository, 
            IBackupAccountRepository backupAccountRepository, 
            IEnumerable<IPaymentValidator> validators, 
            IDataStoreSelector dataStoreSelector,
            IAccountService accountService)
        {
            _accountRepository = accountRepository;
            _backupAccountRepository = backupAccountRepository;
            _validators = validators;
            _dataStoreSelector = dataStoreSelector;
            _accountService = accountService;
        }
        
        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            // Acquire the account from the primary or backup data store based on the configuration.
            var account = _dataStoreSelector.GetPrimary().GetAccount(request.DebtorAccountNumber);
            if (account == null)
            {
                // Account cannot be found in the designated primary or backup data store.
                return new MakePaymentResult { Success = false };
            }

            // Retrieve the first validator that can validate the payment scheme given only one scheme is allowed.
            var validator = _validators.FirstOrDefault(x => x.CanValidate(request.PaymentScheme));
            if (validator == null)
            {
                return new MakePaymentResult { Success = false };
            }
            
            var result = validator.Validate(account, request);
            if (result.Success)
            {
                // If the result is successful, apply the payment to the account.
                _accountService.ApplyPayment(account, request.Amount);
            }

            return result;
        }
    }
}
