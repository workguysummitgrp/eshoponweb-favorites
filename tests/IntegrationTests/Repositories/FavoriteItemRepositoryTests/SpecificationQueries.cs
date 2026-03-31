using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Infrastructure.Data;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Repositories.FavoriteItemRepositoryTests;

public class SpecificationQueries
{
    private readonly CatalogContext _catalogContext;
    private readonly EfRepository<FavoriteItem> _favoriteRepository;
    private readonly string _userId = "spec-test-user";

    public SpecificationQueries()
    {
        var dbOptions = new DbContextOptionsBuilder<CatalogContext>()
            .UseInMemoryDatabase(databaseName: $"FavoritesSpec_{Guid.NewGuid()}")
            .Options;
        _catalogContext = new CatalogContext(dbOptions);
        _favoriteRepository = new EfRepository<FavoriteItem>(_catalogContext);
    }

    private async Task SeedFavorites()
    {
        await _favoriteRepository.AddAsync(new FavoriteItem(_userId, 1), TestContext.Current.CancellationToken);
        await _favoriteRepository.AddAsync(new FavoriteItem(_userId, 2), TestContext.Current.CancellationToken);
        await _favoriteRepository.AddAsync(new FavoriteItem(_userId, 3), TestContext.Current.CancellationToken);
        await _favoriteRepository.AddAsync(new FavoriteItem("other-user", 1), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task UserIdOnlySpecReturnsAllUserFavorites()
    {
        await SeedFavorites();

        var spec = new UserFavoritesSpecification(_userId);
        var results = await _favoriteRepository.ListAsync(spec, TestContext.Current.CancellationToken);

        Assert.Equal(3, results.Count);
        Assert.All(results, f => Assert.Equal(_userId, f.UserId));
    }

    [Fact]
    public async Task SingleItemSpecReturnsSingleMatch()
    {
        await SeedFavorites();

        var spec = new UserFavoritesSpecification(_userId, 2);
        var result = await _favoriteRepository.FirstOrDefaultAsync(spec, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.CatalogItemId);
    }

    [Fact]
    public async Task BatchSpecReturnsOnlyMatchingItems()
    {
        await SeedFavorites();

        var requestedIds = new List<int> { 1, 3, 99 };
        var spec = new UserFavoritesSpecification(_userId, requestedIds);
        var results = await _favoriteRepository.ListAsync(spec, TestContext.Current.CancellationToken);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, f => f.CatalogItemId == 1);
        Assert.Contains(results, f => f.CatalogItemId == 3);
        Assert.DoesNotContain(results, f => f.CatalogItemId == 99);
    }

    [Fact]
    public async Task SpecExcludesOtherUsersFavorites()
    {
        await SeedFavorites();

        var spec = new UserFavoritesSpecification("other-user");
        var results = await _favoriteRepository.ListAsync(spec, TestContext.Current.CancellationToken);

        Assert.Single(results);
        Assert.Equal(1, results[0].CatalogItemId);
    }

    [Fact]
    public async Task BatchSpecReturnsEmptyForNoMatches()
    {
        await SeedFavorites();

        var requestedIds = new List<int> { 98, 99 };
        var spec = new UserFavoritesSpecification(_userId, requestedIds);
        var results = await _favoriteRepository.ListAsync(spec, TestContext.Current.CancellationToken);

        Assert.Empty(results);
    }
}
