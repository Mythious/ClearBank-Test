using ClearBank.DeveloperTest.Business.Repositories.Interfaces;
using ClearBank.DeveloperTest.Business.Repositories.Interfaces.Base;
using ClearBank.DeveloperTest.Data.Configuration.Enums;
using ClearBank.DeveloperTest.Data.Configuration.Interfaces;
using Microsoft.Extensions.Options;

namespace ClearBank.DeveloperTest.Data.Configuration;

public class DataStoreSelector : IDataStoreSelector
{
    private readonly IAccountRepository _accountRepository;
    private readonly IBackupAccountRepository _backupAccountRepository;
    private readonly DataStoreOptions _options;

    public DataStoreSelector(IAccountRepository accountRepository, IBackupAccountRepository backupAccountRepository, IOptions<DataStoreOptions> options)
    {
        _accountRepository = accountRepository;
        _backupAccountRepository = backupAccountRepository;
        _options = options.Value;
    }
    
    public IBaseAccountRepository GetPrimary()
    {
        return _options.DataStoreType == DataStoreType.Live ? _accountRepository : _backupAccountRepository;
    }

    public IBaseAccountRepository GetSecondary()
    {
        // Todo - Enable nullability here for future safety improvements.
        return _options.DataStoreType == DataStoreType.Live ? _backupAccountRepository : null;
    }
}