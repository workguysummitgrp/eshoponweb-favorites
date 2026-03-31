using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Services.FavoriteServiceTests;

public class ToggleFavoriteEdgeCases
{
    private readonly IRepository<FavoriteItem> _mockFavoriteRepo = Substitute.For<IRepository<FavoriteItem>>();
    private readonly IAppLogger<FavoriteService> _mockLogger = Substitute.For<IAppLogger<FavoriteService>>();

    [Fact]
    public async Task ThrowsWhenUserIdIsEmpty()
    {
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ToggleFavoriteAsync("", 5));
    }

    [Fact]
    public async Task ThrowsWhenUserIdIsWhitespace()
    {
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ToggleFavoriteAsync("   ", 5));
    }

    [Fact]
    public async Task ThrowsWhenCatalogItemIdIsNegative()
    {
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ToggleFavoriteAsync("user-1", -1));
    }

    [Fact]
    public async Task PassesCancellationTokenToRepositoryOnAdd()
    {
        _mockFavoriteRepo.FirstOrDefaultAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns((FavoriteItem?)null);

        var cts = new CancellationTokenSource();
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await service.ToggleFavoriteAsync("user-1", 5, cts.Token);

        await _mockFavoriteRepo.Received(1).AddAsync(Arg.Any<FavoriteItem>(), cts.Token);
    }

    [Fact]
    public async Task PassesCancellationTokenToRepositoryOnDelete()
    {
        var existing = new FavoriteItem("user-1", 5);
        _mockFavoriteRepo.FirstOrDefaultAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(existing);

        var cts = new CancellationTokenSource();
        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await service.ToggleFavoriteAsync("user-1", 5, cts.Token);

        await _mockFavoriteRepo.Received(1).DeleteAsync(existing, cts.Token);
    }

    [Fact]
    public async Task AcceptsMaxIntCatalogItemId()
    {
        _mockFavoriteRepo.FirstOrDefaultAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns((FavoriteItem?)null);

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        var result = await service.ToggleFavoriteAsync("user-1", int.MaxValue);

        Assert.True(result);
    }

    [Fact]
    public async Task AddsNewFavoriteWithCorrectProperties()
    {
        FavoriteItem? capturedItem = null;
        _mockFavoriteRepo.FirstOrDefaultAsync(Arg.Any<UserFavoritesSpecification>(), Arg.Any<CancellationToken>())
            .Returns((FavoriteItem?)null);
        _mockFavoriteRepo.AddAsync(Arg.Do<FavoriteItem>(f => capturedItem = f), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<FavoriteItem>());

        var service = new FavoriteService(_mockFavoriteRepo, _mockLogger);

        await service.ToggleFavoriteAsync("user-xyz", 42);

        Assert.NotNull(capturedItem);
        Assert.Equal("user-xyz", capturedItem!.UserId);
        Assert.Equal(42, capturedItem.CatalogItemId);
    }
}
