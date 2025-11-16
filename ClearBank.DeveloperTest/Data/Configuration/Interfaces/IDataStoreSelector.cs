using ClearBank.DeveloperTest.Business.Repositories.Interfaces.Base;

namespace ClearBank.DeveloperTest.Data.Configuration.Interfaces;

public interface IDataStoreSelector
{
    IBaseAccountRepository GetPrimary();
    IBaseAccountRepository GetSecondary(); // Optional base account repository definition for backup data
}