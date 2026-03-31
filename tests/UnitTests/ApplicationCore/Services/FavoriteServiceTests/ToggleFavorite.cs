using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Services.FavoriteServiceTests;

public class ToggleFavorite
{
    private readonly string _userId = "test-user-id";
    private readonly int _catalogItemId = 5;
    private readonly IRepository<FavoriteItem> _mockFavoriteRepo = Substitute.For<IRepository<FavoriteItem>>();
    private readonly IAppLogger<FavoriteService> _mockLogger = Substitute.For<IAppLogger<FavoriteService>>();

    [Fact]
    public async Task ReturnsTrueWhenItemNotYetFavorited()
    {
        _mockFavoriteRepo.FirstOrDefaultAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns((FavoriteItem?)null);

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        var result = await service.ToggleFavoriteAsync(_userId, _catalogItemId);

        Assert.True(result);
        await _mockFavoriteRepo.Received(1).AddAsync(Arg.Any<FavoriteItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReturnsFalseWhenItemAlreadyFavorited()
    {
        var existing = new FavoriteItem(_userId, _catalogItemId);
        _mockFavoriteRepo.FirstOrDefaultAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(existing);

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        var result = await service.ToggleFavoriteAsync(_userId, _catalogItemId);

        Assert.False(result);
        await _mockFavoriteRepo.Received(1).DeleteAsync(existing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DoesNotAddWhenAlreadyFavorited()
    {
        var existing = new FavoriteItem(_userId, _catalogItemId);
        _mockFavoriteRepo.FirstOrDefaultAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(existing);

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await service.ToggleFavoriteAsync(_userId, _catalogItemId);

        await _mockFavoriteRepo.DidNotReceive().AddAsync(Arg.Any<FavoriteItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DoesNotDeleteWhenNotYetFavorited()
    {
        _mockFavoriteRepo.FirstOrDefaultAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns((FavoriteItem?)null);

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await service.ToggleFavoriteAsync(_userId, _catalogItemId);

        await _mockFavoriteRepo.DidNotReceive().DeleteAsync(Arg.Any<FavoriteItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ThrowsWhenUserIdIsNull()
    {
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ToggleFavoriteAsync(null!, _catalogItemId));
    }

    [Fact]
    public async Task ThrowsWhenCatalogItemIdIsZero()
    {
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ToggleFavoriteAsync(_userId, 0));
    }
}
