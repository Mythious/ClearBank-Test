using ClearBank.DeveloperTest.Business.Repositories.Interfaces;
using ClearBank.DeveloperTest.Data.Configuration;
using ClearBank.DeveloperTest.Data.Configuration.Enums;
using ClearBank.DeveloperTest.Data.Configuration.Interfaces;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Configuration;

public class DataStoreSelectorFacts
{
    private readonly Mock<IAccountRepository> _mockAccountRepository = new();
    private readonly Mock<IBackupAccountRepository> _mockBackupAccountRepository = new();
    
    [Fact]
    public void GetPrimary_ReturnsLive_WhenDataStoreTypeIsLive()
    {
        // Arrange
        var selector = CreateSelector(DataStoreType.Live);

        // Act
        var primary = selector.GetPrimary();
        var secondary = selector.GetSecondary();

        // Assert
        primary.ShouldBe(_mockAccountRepository.Object);
        secondary.ShouldBe(_mockBackupAccountRepository.Object);
    }

    [Fact]
    public void GetPrimary_ReturnsBackup_WhenDataStoreTypeIsBackup()
    {
        // Arrange
        var selector = CreateSelector(DataStoreType.Backup);

        // Act
        var primary = selector.GetPrimary();
        var secondary = selector.GetSecondary();

        // Assert
        primary.ShouldBe(_mockBackupAccountRepository.Object);
        secondary.ShouldBeNull();
    }
    
    private IDataStoreSelector CreateSelector(DataStoreType type)
    {
        return new DataStoreSelector(_mockAccountRepository.Object, _mockBackupAccountRepository.Object, Options.Create(new DataStoreOptions { DataStoreType = type}));
    }
}