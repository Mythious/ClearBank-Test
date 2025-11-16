using ClearBank.DeveloperTest.Data.Configuration.Enums;

namespace ClearBank.DeveloperTest.Data.Configuration;

public class DataStoreOptions
{
    // Presumes Live data store is always available and is set to default
    public DataStoreType DataStoreType { get; set; } = DataStoreType.Live;
}