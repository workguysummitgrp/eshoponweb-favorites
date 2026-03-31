using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Services.FavoriteServiceTests;

public class GetUserFavoriteIdsEdgeCases
{
    private readonly IRepository<FavoriteItem> _mockFavoriteRepo = Substitute.For<IRepository<FavoriteItem>>();
    private readonly IAppLogger<FavoriteService> _mockLogger = Substitute.For<IAppLogger<FavoriteService>>();

    [Fact]
    public async Task ThrowsWhenUserIdIsEmpty()
    {
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetUserFavoriteIdsAsync(""));
    }

    [Fact]
    public async Task ThrowsWhenUserIdIsWhitespace()
    {
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetUserFavoriteIdsAsync("   "));
    }

    [Fact]
    public async Task ReturnsSingleItemList()
    {
        _mockFavoriteRepo.ListAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<FavoriteItem> { new FavoriteItem("user-1", 42) });

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        var result = await service.GetUserFavoriteIdsAsync("user-1");

        Assert.Single(result);
        Assert.Equal(42, result[0]);
    }

    [Fact]
    public async Task ReturnsReadOnlyList()
    {
        _mockFavoriteRepo.ListAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<FavoriteItem>());

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        var result = await service.GetUserFavoriteIdsAsync("user-1");

        Assert.IsAssignableFrom<IReadOnlyList<int>>(result);
    }

    [Fact]
    public async Task PassesCancellationTokenToRepository()
    {
        _mockFavoriteRepo.ListAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<FavoriteItem>());

        var cts = new CancellationTokenSource();
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await service.GetUserFavoriteIdsAsync("user-1", cts.Token);

        await _mockFavoriteRepo.Received(1).ListAsync(Arg.Any<UserFavoritesSpecification>(), cts.Token);
    }

    [Fact]
    public async Task PreservesOrderFromRepository()
    {
        var favorites = new List<FavoriteItem>
        {
            new FavoriteItem("user-1", 10),
            new FavoriteItem("user-1", 3),
            new FavoriteItem("user-1", 7)
        };
        _mockFavoriteRepo.ListAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(favorites);

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        var result = await service.GetUserFavoriteIdsAsync("user-1");

        Assert.Equal(new[] { 10, 3, 7 }, result);
    }
}
