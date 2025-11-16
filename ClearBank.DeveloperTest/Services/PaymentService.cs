using System.Collections.Generic;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Types;
using System.Configuration;
using System.Linq;
using ClearBank.DeveloperTest.Business.Repositories.Interfaces;
using ClearBank.DeveloperTest.Business.Validators.Interfaces;
using ClearBank.DeveloperTest.Data.Configuration;
using ClearBank.DeveloperTest.Data.Configuration.Interfaces;

namespace ClearBank.DeveloperTest.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IBackupAccountRepository _backupAccountRepository;
        private readonly IEnumerable<IPaymentValidator> _validators;
        private readonly IDataStoreSelector _dataStoreSelector;

        public PaymentService(IAccountRepository accountRepository, IBackupAccountRepository backupAccountRepository, IEnumerable<IPaymentValidator> validators, IDataStoreSelector dataStoreSelector)
        {
            _accountRepository = accountRepository;
            _backupAccountRepository = backupAccountRepository;
            _validators = validators;
            _dataStoreSelector = dataStoreSelector;
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

            var result = new MakePaymentResult();

            result.Success = true;
            
            // Retrieve the first validator that can validate the payment scheme given only one scheme is allowed.
            var validator = _validators.FirstOrDefault(x => x.CanValidate(request.PaymentScheme));
            if (validator != null)
            {
                // If a valid validator is found, validate the payment.
                result = validator.Validate(account, request);
            }
            else
            {
                return new MakePaymentResult { Success = false };
            }

            if (result.Success)
            {
                account.Balance -= request.Amount;
                
                // Update primary repository
                _dataStoreSelector.GetPrimary().UpdateAccount(account);

                // Also optionally update backup repository to keep in sync
                var backup = _dataStoreSelector.GetSecondary();
                if (backup != null)
                {
                    backup.UpdateAccount(account);
                }
            }

            return result;
        }
    }
}
