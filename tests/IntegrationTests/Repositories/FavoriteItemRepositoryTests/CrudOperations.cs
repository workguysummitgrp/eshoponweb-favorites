using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Infrastructure.Data;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Repositories.FavoriteItemRepositoryTests;

public class CrudOperations
{
    private readonly CatalogContext _catalogContext;
    private readonly EfRepository<FavoriteItem> _favoriteRepository;
    private readonly string _testUserId = "crud-test-user";

    public CrudOperations()
    {
        var dbOptions = new DbContextOptionsBuilder<CatalogContext>()
            .UseInMemoryDatabase(databaseName: $"FavoritesCrud_{Guid.NewGuid()}")
            .Options;
        _catalogContext = new CatalogContext(dbOptions);
        _favoriteRepository = new EfRepository<FavoriteItem>(_catalogContext);
    }

    [Fact]
    public async Task AddsFavoriteAndAssignsId()
    {
        var favorite = new FavoriteItem(_testUserId, 1);

        await _favoriteRepository.AddAsync(favorite, TestContext.Current.CancellationToken);

        Assert.True(favorite.Id > 0);
    }

    [Fact]
    public async Task GetsFavoriteBySpecification()
    {
        var favorite = new FavoriteItem(_testUserId, 2);
        await _favoriteRepository.AddAsync(favorite, TestContext.Current.CancellationToken);

        var spec = new UserFavoritesSpecification(_testUserId, 2);
        var result = await _favoriteRepository.FirstOrDefaultAsync(spec, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(_testUserId, result.UserId);
        Assert.Equal(2, result.CatalogItemId);
    }

    [Fact]
    public async Task DeletesFavoriteSuccessfully()
    {
        var favorite = new FavoriteItem(_testUserId, 3);
        await _favoriteRepository.AddAsync(favorite, TestContext.Current.CancellationToken);

        await _favoriteRepository.DeleteAsync(favorite, TestContext.Current.CancellationToken);

        var spec = new UserFavoritesSpecification(_testUserId, 3);
        var result = await _favoriteRepository.FirstOrDefaultAsync(spec, TestContext.Current.CancellationToken);
        Assert.Null(result);
    }

    [Fact]
    public async Task ReturnsNullForNonExistentFavorite()
    {
        var spec = new UserFavoritesSpecification("nonexistent-user", 999);
        var result = await _favoriteRepository.FirstOrDefaultAsync(spec, TestContext.Current.CancellationToken);

        Assert.Null(result);
    }
}
