using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Web.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.Web.Services;

public class CatalogViewModelServiceGetCatalogItems
{
    private readonly IRepository<CatalogItem> _mockItemRepo = Substitute.For<IRepository<CatalogItem>>();
    private readonly IRepository<CatalogBrand> _mockBrandRepo = Substitute.For<IRepository<CatalogBrand>>();
    private readonly IRepository<CatalogType> _mockTypeRepo = Substitute.For<IRepository<CatalogType>>();
    private readonly IUriComposer _mockUriComposer = Substitute.For<IUriComposer>();
    private readonly IFavoriteService _mockFavoriteService = Substitute.For<IFavoriteService>();
    private readonly ILoggerFactory _mockLoggerFactory = Substitute.For<ILoggerFactory>();

    private readonly string _testUserId = "vm-test-user";

    public CatalogViewModelServiceGetCatalogItems()
    {
        _mockLoggerFactory.CreateLogger(Arg.Any<string>())
            .Returns(Substitute.For<ILogger>());
        _mockUriComposer.ComposePicUri(Arg.Any<string>())
            .Returns(callInfo => callInfo.Arg<string>());
    }

    private void SetupItemRepo(List<CatalogItem> items)
    {
        _mockItemRepo.ListAsync(Arg.Any<ISpecification<CatalogItem>>(), Arg.Any<CancellationToken>())
            .Returns(items);
        _mockItemRepo.CountAsync(Arg.Any<ISpecification<CatalogItem>>(), Arg.Any<CancellationToken>())
            .Returns(items.Count);
        _mockBrandRepo.ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CatalogBrand>());
        _mockTypeRepo.ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CatalogType>());
    }

    [Fact]
    public async Task PopulatesIsFavoritedWhenUserIdProvided()
    {
        var items = new List<CatalogItem>
        {
            new CatalogItem(1, 1, "desc1", "Item1", 10m, "pic1.png"),
            new CatalogItem(1, 1, "desc2", "Item2", 20m, "pic2.png")
        };
        typeof(BaseEntity).GetProperty("Id")!.SetValue(items[0], 10);
        typeof(BaseEntity).GetProperty("Id")!.SetValue(items[1], 20);
        SetupItemRepo(items);

        _mockFavoriteService.GetUserFavoritesForItemsAsync(
                _testUserId,
                Arg.Any<IEnumerable<int>>(),
                Arg.Any<CancellationToken>())
            .Returns(new HashSet<int> { 10 } as IReadOnlySet<int>);

        var service = new CatalogViewModelService(
            _mockLoggerFactory, _mockItemRepo, _mockBrandRepo, _mockTypeRepo, _mockUriComposer, _mockFavoriteService);

        var result = await service.GetCatalogItems(0, 10, null, null, _testUserId);

        Assert.True(result.CatalogItems[0].IsFavorited);
        Assert.False(result.CatalogItems[1].IsFavorited);
    }

    [Fact]
    public async Task LeavesIsFavoritedFalseWhenNoUserId()
    {
        var items = new List<CatalogItem>
        {
            new CatalogItem(1, 1, "desc1", "Item1", 10m, "pic1.png")
        };
        typeof(BaseEntity).GetProperty("Id")!.SetValue(items[0], 10);
        SetupItemRepo(items);

        var service = new CatalogViewModelService(
            _mockLoggerFactory, _mockItemRepo, _mockBrandRepo, _mockTypeRepo, _mockUriComposer, _mockFavoriteService);

        var result = await service.GetCatalogItems(0, 10, null, null, null);

        Assert.False(result.CatalogItems[0].IsFavorited);
        await _mockFavoriteService.DidNotReceive()
            .GetUserFavoritesForItemsAsync(Arg.Any<string>(), Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BatchFetchCalledWithCorrectItemIds()
    {
        var items = new List<CatalogItem>
        {
            new CatalogItem(1, 1, "desc1", "Item1", 10m, "pic1.png"),
            new CatalogItem(1, 1, "desc2", "Item2", 20m, "pic2.png"),
            new CatalogItem(1, 1, "desc3", "Item3", 30m, "pic3.png")
        };
        typeof(BaseEntity).GetProperty("Id")!.SetValue(items[0], 5);
        typeof(BaseEntity).GetProperty("Id")!.SetValue(items[1], 15);
        typeof(BaseEntity).GetProperty("Id")!.SetValue(items[2], 25);
        SetupItemRepo(items);

        _mockFavoriteService.GetUserFavoritesForItemsAsync(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<int>>(),
                Arg.Any<CancellationToken>())
            .Returns(new HashSet<int>() as IReadOnlySet<int>);

        var service = new CatalogViewModelService(
            _mockLoggerFactory, _mockItemRepo, _mockBrandRepo, _mockTypeRepo, _mockUriComposer, _mockFavoriteService);

        await service.GetCatalogItems(0, 10, null, null, _testUserId);

        await _mockFavoriteService.Received(1).GetUserFavoritesForItemsAsync(
            _testUserId,
            Arg.Is<IEnumerable<int>>(ids => ids.Count() == 3),
            Arg.Any<CancellationToken>());
    }
}
