using System.Collections.Generic;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Types;
using System.Configuration;
using System.Linq;
using ClearBank.DeveloperTest.Business.Repositories.Interfaces;
using ClearBank.DeveloperTest.Business.Validators.Interfaces;

namespace ClearBank.DeveloperTest.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IBackupAccountRepository _backupAccountRepository;
        private readonly IEnumerable<IPaymentValidator> _validators;

        public PaymentService(IAccountRepository accountRepository, IBackupAccountRepository backupAccountRepository, IEnumerable<IPaymentValidator> validators)
        {
            _accountRepository = accountRepository;
            _backupAccountRepository = backupAccountRepository;
            _validators = validators;
        }
        
        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            var dataStoreType = ConfigurationManager.AppSettings["DataStoreType"];

            Account account = null;

            if (dataStoreType == "Backup")
            {
                account = _backupAccountRepository.GetAccount(request.DebtorAccountNumber);
            }
            else
            {
                account = _accountRepository.GetAccount(request.DebtorAccountNumber);
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

                if (dataStoreType == "Backup")
                {
                    _backupAccountRepository.UpdateAccount(account);
                }
                else
                {
                    _accountRepository.UpdateAccount(account);
                }
            }

            return result;
        }
    }
}
