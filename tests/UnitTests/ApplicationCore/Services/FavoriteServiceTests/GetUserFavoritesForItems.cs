using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Services.FavoriteServiceTests;

public class GetUserFavoritesForItems
{
    private readonly string _userId = "test-user-id";
    private readonly IRepository<FavoriteItem> _mockFavoriteRepo = Substitute.For<IRepository<FavoriteItem>>();
    private readonly IAppLogger<FavoriteService> _mockLogger = Substitute.For<IAppLogger<FavoriteService>>();

    [Fact]
    public async Task ReturnsMatchingCatalogItemIds()
    {
        var favorites = new List<FavoriteItem>
        {
            new FavoriteItem(_userId, 3),
            new FavoriteItem(_userId, 7)
        };

        _mockFavoriteRepo.ListAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(favorites);

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        var result = await service.GetUserFavoritesForItemsAsync(_userId, new[] { 3, 5, 7 });

        Assert.Equal(2, result.Count);
        Assert.Contains(3, result);
        Assert.Contains(7, result);
        Assert.DoesNotContain(5, result);
    }

    [Fact]
    public async Task ReturnsEmptySetWhenNoCatalogItemIdsProvided()
    {
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        var result = await service.GetUserFavoritesForItemsAsync(_userId, Array.Empty<int>());

        Assert.Empty(result);
        await _mockFavoriteRepo.DidNotReceive().ListAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReturnsEmptySetWhenNoFavoritesMatch()
    {
        _mockFavoriteRepo.ListAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<FavoriteItem>());

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        var result = await service.GetUserFavoritesForItemsAsync(_userId, new[] { 1, 2, 3 });

        Assert.Empty(result);
    }

    [Fact]
    public async Task ThrowsWhenUserIdIsNull()
    {
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetUserFavoritesForItemsAsync(null!, new[] { 1 }));
    }
}
