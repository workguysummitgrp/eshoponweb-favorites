using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Web.Controllers.Api;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.Web.Controllers.Api;

public class FavoritesControllerToggle
{
    private readonly IFavoriteService _mockFavoriteService = Substitute.For<IFavoriteService>();
    private readonly string _testUserId = "test-user-123";

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
    public async Task ReturnsOkWithTrueWhenFavoriteAdded()
    {
        _mockFavoriteService.ToggleFavoriteAsync(_testUserId, 5, Arg.Any<CancellationToken>())
            .Returns(true);
        var controller = CreateControllerWithUser(_testUserId);
        var request = new FavoritesController.ToggleFavoriteRequest { CatalogItemId = 5 };

        var result = await controller.Toggle(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<FavoritesController.ToggleFavoriteResponse>(okResult.Value);
        Assert.True(response.IsFavorited);
    }

    [Fact]
    public async Task ReturnsOkWithFalseWhenFavoriteRemoved()
    {
        _mockFavoriteService.ToggleFavoriteAsync(_testUserId, 5, Arg.Any<CancellationToken>())
            .Returns(false);
        var controller = CreateControllerWithUser(_testUserId);
        var request = new FavoritesController.ToggleFavoriteRequest { CatalogItemId = 5 };

        var result = await controller.Toggle(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<FavoritesController.ToggleFavoriteResponse>(okResult.Value);
        Assert.False(response.IsFavorited);
    }

    [Fact]
    public async Task ReturnsUnauthorizedWhenUserHasNoNameIdentifier()
    {
        var controller = CreateControllerWithUser(null);
        var request = new FavoritesController.ToggleFavoriteRequest { CatalogItemId = 5 };

        var result = await controller.Toggle(request, CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task ReturnsBadRequestWhenCatalogItemIdIsZero()
    {
        var controller = CreateControllerWithUser(_testUserId);
        var request = new FavoritesController.ToggleFavoriteRequest { CatalogItemId = 0 };

        var result = await controller.Toggle(request, CancellationToken.None);

        Assert.IsType<BadRequestResult>(result);
    }
}
