using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Web.Extensions;
using Microsoft.eShopWeb.Web.Services;
using Microsoft.eShopWeb.Web.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.Web.Services;

public class CachedCatalogViewModelServiceGetCatalogItems
{
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly IFavoriteService _mockFavoriteService = Substitute.For<IFavoriteService>();
    private readonly CatalogViewModelService _innerService;
    private readonly IRepository<CatalogItem> _mockItemRepo = Substitute.For<IRepository<CatalogItem>>();
    private readonly string _testUserId = "cached-test-user";

    public CachedCatalogViewModelServiceGetCatalogItems()
    {
        var mockBrandRepo = Substitute.For<IRepository<CatalogBrand>>();
        var mockTypeRepo = Substitute.For<IRepository<CatalogType>>();
        var mockUriComposer = Substitute.For<IUriComposer>();
        var innerFavoriteService = Substitute.For<IFavoriteService>();
        var mockLoggerFactory = Substitute.For<ILoggerFactory>();
        mockLoggerFactory.CreateLogger(Arg.Any<string>())
            .Returns(Substitute.For<ILogger>());
        mockUriComposer.ComposePicUri(Arg.Any<string>())
            .Returns(callInfo => callInfo.Arg<string>());

        var items = new List<CatalogItem>
        {
            new CatalogItem(1, 1, "desc1", "Item1", 10m, "pic1.png"),
            new CatalogItem(1, 1, "desc2", "Item2", 20m, "pic2.png")
        };
        typeof(BaseEntity).GetProperty("Id")!.SetValue(items[0], 100);
        typeof(BaseEntity).GetProperty("Id")!.SetValue(items[1], 200);

        _mockItemRepo.ListAsync(Arg.Any<ISpecification<CatalogItem>>(), Arg.Any<CancellationToken>())
            .Returns(items);
        _mockItemRepo.CountAsync(Arg.Any<ISpecification<CatalogItem>>(), Arg.Any<CancellationToken>())
            .Returns(items.Count);
        mockBrandRepo.ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CatalogBrand>());
        mockTypeRepo.ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CatalogType>());

        innerFavoriteService.GetUserFavoritesForItemsAsync(
                Arg.Any<string>(), Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<int>() as IReadOnlySet<int>);

        _innerService = new CatalogViewModelService(
            mockLoggerFactory, _mockItemRepo, mockBrandRepo, mockTypeRepo, mockUriComposer, innerFavoriteService);
    }

    [Fact]
    public async Task OverlaysFavoritesForAuthenticatedUser()
    {
        _mockFavoriteService.GetUserFavoritesForItemsAsync(
                _testUserId,
                Arg.Any<IEnumerable<int>>(),
                Arg.Any<CancellationToken>())
            .Returns(new HashSet<int> { 100 } as IReadOnlySet<int>);

        var service = new CachedCatalogViewModelService(_cache, _innerService, _mockFavoriteService);

        var result = await service.GetCatalogItems(0, 10, null, null, _testUserId);

        Assert.True(result.CatalogItems[0].IsFavorited);
        Assert.False(result.CatalogItems[1].IsFavorited);
    }

    [Fact]
    public async Task ReturnsAllFalseWhenNoUserId()
    {
        var service = new CachedCatalogViewModelService(_cache, _innerService, _mockFavoriteService);

        var result = await service.GetCatalogItems(0, 10, null, null, null);

        Assert.All(result.CatalogItems, item => Assert.False(item.IsFavorited));
        await _mockFavoriteService.DidNotReceive()
            .GetUserFavoritesForItemsAsync(Arg.Any<string>(), Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CachedDataDoesNotContainFavoritesState()
    {
        _mockFavoriteService.GetUserFavoritesForItemsAsync(
                _testUserId,
                Arg.Any<IEnumerable<int>>(),
                Arg.Any<CancellationToken>())
            .Returns(new HashSet<int> { 100 } as IReadOnlySet<int>);

        var service = new CachedCatalogViewModelService(_cache, _innerService, _mockFavoriteService);

        var withUser = await service.GetCatalogItems(0, 10, null, null, _testUserId);
        Assert.True(withUser.CatalogItems[0].IsFavorited);

        var withoutUser = await service.GetCatalogItems(0, 10, null, null, null);
        Assert.All(withoutUser.CatalogItems, item => Assert.False(item.IsFavorited));
    }
}
