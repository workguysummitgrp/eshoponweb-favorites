using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Services.FavoriteServiceTests;

public class GetUserFavoritesForItemsEdgeCases
{
    private readonly IRepository<FavoriteItem> _mockFavoriteRepo = Substitute.For<IRepository<FavoriteItem>>();
    private readonly IAppLogger<FavoriteService> _mockLogger = Substitute.For<IAppLogger<FavoriteService>>();

    [Fact]
    public async Task ThrowsWhenUserIdIsEmpty()
    {
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetUserFavoritesForItemsAsync("", new[] { 1 }));
    }

    [Fact]
    public async Task ThrowsWhenUserIdIsWhitespace()
    {
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetUserFavoritesForItemsAsync("   ", new[] { 1 }));
    }

    [Fact]
    public async Task ReturnsSingleItemWhenOnlyOneMatches()
    {
        _mockFavoriteRepo.ListAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<FavoriteItem> { new FavoriteItem("user-1", 7) });

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        var result = await service.GetUserFavoritesForItemsAsync("user-1", new[] { 7 });

        Assert.Single(result);
        Assert.Contains(7, result);
    }

    [Fact]
    public async Task ReturnsAllWhenAllItemsAreFavorited()
    {
        var favorites = new List<FavoriteItem>
        {
            new FavoriteItem("user-1", 1),
            new FavoriteItem("user-1", 2),
            new FavoriteItem("user-1", 3)
        };
        _mockFavoriteRepo.ListAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(favorites);

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        var result = await service.GetUserFavoritesForItemsAsync("user-1", new[] { 1, 2, 3 });

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task PassesCancellationTokenToRepository()
    {
        _mockFavoriteRepo.ListAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<FavoriteItem>());

        var cts = new CancellationTokenSource();
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await service.GetUserFavoritesForItemsAsync("user-1", new[] { 1 }, cts.Token);

        await _mockFavoriteRepo.Received(1).ListAsync(Arg.Any<UserFavoritesSpecification>(), cts.Token);
    }

    [Fact]
    public async Task SkipsRepositoryCallWhenEmptyItemIds()
    {
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        var result = await service.GetUserFavoritesForItemsAsync("user-1", Array.Empty<int>());

        Assert.Empty(result);
        await _mockFavoriteRepo.DidNotReceive().ListAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReturnsReadOnlySet()
    {
        _mockFavoriteRepo.ListAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<FavoriteItem> { new FavoriteItem("user-1", 5) });

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        var result = await service.GetUserFavoritesForItemsAsync("user-1", new[] { 5 });

        Assert.IsAssignableFrom<IReadOnlySet<int>>(result);
    }
}
