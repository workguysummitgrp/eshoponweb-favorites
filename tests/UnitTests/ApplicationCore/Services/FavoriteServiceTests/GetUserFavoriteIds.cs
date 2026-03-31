using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Services.FavoriteServiceTests;

public class GetUserFavoriteIds
{
    private readonly string _userId = "test-user-id";
    private readonly IRepository<FavoriteItem> _mockFavoriteRepo = Substitute.For<IRepository<FavoriteItem>>();
    private readonly IAppLogger<FavoriteService> _mockLogger = Substitute.For<IAppLogger<FavoriteService>>();

    [Fact]
    public async Task ReturnsAllFavoriteCatalogItemIds()
    {
        var favorites = new List<FavoriteItem>
        {
            new FavoriteItem(_userId, 1),
            new FavoriteItem(_userId, 5),
            new FavoriteItem(_userId, 10)
        };

        _mockFavoriteRepo.ListAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(favorites);

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        var result = await service.GetUserFavoriteIdsAsync(_userId);

        Assert.Equal(3, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(5, result);
        Assert.Contains(10, result);
    }

    [Fact]
    public async Task ReturnsEmptyListWhenNoFavorites()
    {
        _mockFavoriteRepo.ListAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<FavoriteItem>());

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        var result = await service.GetUserFavoriteIdsAsync(_userId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ThrowsWhenUserIdIsNull()
    {
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetUserFavoriteIdsAsync(null!));
    }

    [Fact]
    public async Task InvokesRepositoryListAsyncOnce()
    {
        _mockFavoriteRepo.ListAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<FavoriteItem>());

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await service.GetUserFavoriteIdsAsync(_userId);

        await _mockFavoriteRepo.Received(1).ListAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>());
    }
}
