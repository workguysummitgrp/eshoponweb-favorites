using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Infrastructure.Data;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Repositories.FavoriteItemRepositoryTests;

public class ToggleRoundtrip
{
    private readonly CatalogContext _catalogContext;
    private readonly EfRepository<FavoriteItem> _favoriteRepository;
    private readonly FavoriteService _favoriteService;
    private readonly string _userId = "roundtrip-user";

    public ToggleRoundtrip()
    {
        var dbOptions = new DbContextOptionsBuilder<CatalogContext>()
            .UseInMemoryDatabase(databaseName: $"FavoritesRoundtrip_{Guid.NewGuid()}")
            .Options;
        _catalogContext = new CatalogContext(dbOptions);
        _favoriteRepository = new EfRepository<FavoriteItem>(_catalogContext);
        var logger = Substitute.For<IAppLogger<FavoriteService>>();
        _favoriteService = new FavoriteService(_favoriteRepository, logger);
    }

    [Fact]
    public async Task ToggleAddsThenRemovesFavorite()
    {
        var added = await _favoriteService.ToggleFavoriteAsync(_userId, 5);
        Assert.True(added);

        var removed = await _favoriteService.ToggleFavoriteAsync(_userId, 5);
        Assert.False(removed);

        var spec = new UserFavoritesSpecification(_userId);
        var remaining = await _favoriteRepository.ListAsync(spec, TestContext.Current.CancellationToken);
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task GetUserFavoriteIdsReturnsAddedItems()
    {
        await _favoriteService.ToggleFavoriteAsync(_userId, 3);
        await _favoriteService.ToggleFavoriteAsync(_userId, 7);
        await _favoriteService.ToggleFavoriteAsync(_userId, 11);

        var result = await _favoriteService.GetUserFavoriteIdsAsync(_userId);

        Assert.Equal(3, result.Count);
        Assert.Contains(3, result);
        Assert.Contains(7, result);
        Assert.Contains(11, result);
    }

    [Fact]
    public async Task GetUserFavoritesForItemsReturnsBatchResults()
    {
        await _favoriteService.ToggleFavoriteAsync(_userId, 1);
        await _favoriteService.ToggleFavoriteAsync(_userId, 5);
        await _favoriteService.ToggleFavoriteAsync(_userId, 10);

        var result = await _favoriteService.GetUserFavoritesForItemsAsync(
            _userId, new[] { 1, 5, 99 });

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(5, result);
        Assert.DoesNotContain(99, result);
    }

    [Fact]
    public async Task FullLifecycleAddVerifyRemoveVerify()
    {
        var added = await _favoriteService.ToggleFavoriteAsync(_userId, 42);
        Assert.True(added);

        var allIds = await _favoriteService.GetUserFavoriteIdsAsync(_userId);
        Assert.Single(allIds);
        Assert.Equal(42, allIds[0]);

        var batchResult = await _favoriteService.GetUserFavoritesForItemsAsync(
            _userId, new[] { 42, 99 });
        Assert.Single(batchResult);
        Assert.Contains(42, batchResult);

        var removed = await _favoriteService.ToggleFavoriteAsync(_userId, 42);
        Assert.False(removed);

        var afterRemoval = await _favoriteService.GetUserFavoriteIdsAsync(_userId);
        Assert.Empty(afterRemoval);
    }
}
