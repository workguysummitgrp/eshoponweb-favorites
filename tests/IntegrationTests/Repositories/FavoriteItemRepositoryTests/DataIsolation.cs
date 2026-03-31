using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.Infrastructure.Data;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Repositories.FavoriteItemRepositoryTests;

public class DataIsolation
{
    private readonly FavoriteService _favoriteService;
    private readonly string _userA = "isolation-user-A";
    private readonly string _userB = "isolation-user-B";

    public DataIsolation()
    {
        var dbOptions = new DbContextOptionsBuilder<CatalogContext>()
            .UseInMemoryDatabase(databaseName: $"FavoritesIsolation_{Guid.NewGuid()}")
            .Options;
        var catalogContext = new CatalogContext(dbOptions);
        var favoriteRepository = new EfRepository<FavoriteItem>(catalogContext);
        var logger = Substitute.For<IAppLogger<FavoriteService>>();
        _favoriteService = new FavoriteService(favoriteRepository, logger);
    }

    [Fact]
    public async Task UserAFavoritesNotVisibleToUserB()
    {
        await _favoriteService.ToggleFavoriteAsync(_userA, 1);
        await _favoriteService.ToggleFavoriteAsync(_userA, 2);

        var userBIds = await _favoriteService.GetUserFavoriteIdsAsync(_userB);

        Assert.Empty(userBIds);
    }

    [Fact]
    public async Task ToggleForUserADoesNotAffectUserB()
    {
        await _favoriteService.ToggleFavoriteAsync(_userA, 5);
        await _favoriteService.ToggleFavoriteAsync(_userB, 5);

        await _favoriteService.ToggleFavoriteAsync(_userA, 5);

        var userBResult = await _favoriteService.GetUserFavoritesForItemsAsync(
            _userB, new[] { 5 });
        Assert.Contains(5, userBResult);

        var userAResult = await _favoriteService.GetUserFavoritesForItemsAsync(
            _userA, new[] { 5 });
        Assert.DoesNotContain(5, userAResult);
    }

    [Fact]
    public async Task BatchFetchReturnsOnlyRequestingUserItems()
    {
        await _favoriteService.ToggleFavoriteAsync(_userA, 1);
        await _favoriteService.ToggleFavoriteAsync(_userA, 3);
        await _favoriteService.ToggleFavoriteAsync(_userB, 1);
        await _favoriteService.ToggleFavoriteAsync(_userB, 5);

        var userAResult = await _favoriteService.GetUserFavoritesForItemsAsync(
            _userA, new[] { 1, 3, 5 });

        Assert.Equal(2, userAResult.Count);
        Assert.Contains(1, userAResult);
        Assert.Contains(3, userAResult);
        Assert.DoesNotContain(5, userAResult);
    }
}
