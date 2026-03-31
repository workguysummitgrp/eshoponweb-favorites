using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Web.Controllers.Api;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.Web.Controllers.Api;

public class FavoritesControllerToggleEdgeCases
{
    private readonly IFavoriteService _mockFavoriteService = Substitute.For<IFavoriteService>();
    private readonly string _testUserId = "edge-user-123";

    private FavoritesController CreateControllerWithUser(string? userId)
    {
        var controller = new FavoritesController(_mockFavoriteService);
        var claims = new List<Claim>();
        if (userId is not null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        return controller;
    }

    [Fact]
    public async Task ReturnsBadRequestWhenCatalogItemIdIsNegative()
    {
        var controller = CreateControllerWithUser(_testUserId);
        var request = new FavoritesController.ToggleFavoriteRequest { CatalogItemId = -10 };

        var result = await controller.Toggle(request, CancellationToken.None);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task PassesCorrectUserIdToService()
    {
        _mockFavoriteService.ToggleFavoriteAsync(_testUserId, 42, Arg.Any<CancellationToken>())
            .Returns(true);
        var controller = CreateControllerWithUser(_testUserId);
        var request = new FavoritesController.ToggleFavoriteRequest { CatalogItemId = 42 };

        await controller.Toggle(request, CancellationToken.None);

        await _mockFavoriteService.Received(1).ToggleFavoriteAsync(
            _testUserId, 42, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PropagatesServiceException()
    {
        _mockFavoriteService.ToggleFavoriteAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("DB failure"));
        var controller = CreateControllerWithUser(_testUserId);
        var request = new FavoritesController.ToggleFavoriteRequest { CatalogItemId = 5 };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            controller.Toggle(request, CancellationToken.None));
    }

    [Fact]
    public async Task ReturnsBadRequestForMinValueCatalogItemId()
    {
        var controller = CreateControllerWithUser(_testUserId);
        var request = new FavoritesController.ToggleFavoriteRequest { CatalogItemId = int.MinValue };

        var result = await controller.Toggle(request, CancellationToken.None);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task DoesNotCallServiceWhenCatalogItemIdInvalid()
    {
        var controller = CreateControllerWithUser(_testUserId);
        var request = new FavoritesController.ToggleFavoriteRequest { CatalogItemId = -1 };

        await controller.Toggle(request, CancellationToken.None);

        await _mockFavoriteService.DidNotReceive()
            .ToggleFavoriteAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DoesNotCallServiceWhenUnauthorized()
    {
        var controller = CreateControllerWithUser(null);
        var request = new FavoritesController.ToggleFavoriteRequest { CatalogItemId = 5 };

        await controller.Toggle(request, CancellationToken.None);

        await _mockFavoriteService.DidNotReceive()
            .ToggleFavoriteAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
