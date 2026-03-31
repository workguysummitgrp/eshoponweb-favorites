using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Web.Controllers.Api;
using NSubstitute;

namespace Microsoft.eShopWeb.UnitTests.Web.Controllers.Api;

/// <summary>
/// Security-focused tests for FavoritesController — OWASP A01, A03, A04.
/// </summary>
public class FavoritesControllerSecurityTests
{
    private readonly IFavoriteService _favoriteService = Substitute.For<IFavoriteService>();
    private readonly FavoritesController _sut;

    public FavoritesControllerSecurityTests()
    {
        _sut = new FavoritesController(_favoriteService);
    }

    private void SetUser(string? userId)
    {
        var claims = new List<Claim>();
        if (userId is not null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // --- A01: Broken Access Control ---

    [Fact]
    public async Task Toggle_ReturnsUnauthorized_WhenNameIdentifierClaimMissing()
    {
        SetUser(null);
        var request = new FavoritesController.ToggleFavoriteRequest { CatalogItemId = 1 };

        var result = await _sut.Toggle(request, CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
        await _favoriteService.DidNotReceiveWithAnyArgs()
            .ToggleFavoriteAsync(default!, default, default);
    }

    [Fact]
    public async Task Toggle_UsesClaimsPrincipalUserId_NotRequestData()
    {
        var expectedUserId = "user-123";
        SetUser(expectedUserId);
        var request = new FavoritesController.ToggleFavoriteRequest { CatalogItemId = 42 };
        _favoriteService.ToggleFavoriteAsync(expectedUserId, 42, Arg.Any<CancellationToken>())
            .Returns(true);

        await _sut.Toggle(request, CancellationToken.None);

        await _favoriteService.Received(1)
            .ToggleFavoriteAsync(expectedUserId, 42, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ToggleFavoriteRequest_HasNoUserIdProperty()
    {
        var request = new FavoritesController.ToggleFavoriteRequest { CatalogItemId = 1 };
        var properties = typeof(FavoritesController.ToggleFavoriteRequest).GetProperties();

        Assert.Single(properties);
        Assert.Equal("CatalogItemId", properties[0].Name);
    }

    // --- A03: Injection ---

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    [InlineData(int.MinValue)]
    public async Task Toggle_ReturnsBadRequest_ForNonPositiveCatalogItemId(int invalidId)
    {
        SetUser("user-123");
        var request = new FavoritesController.ToggleFavoriteRequest { CatalogItemId = invalidId };

        var result = await _sut.Toggle(request, CancellationToken.None);

        Assert.IsType<BadRequestResult>(result);
        await _favoriteService.DidNotReceiveWithAnyArgs()
            .ToggleFavoriteAsync(default!, default, default);
    }

    [Fact]
    public async Task Toggle_AcceptsMaxIntCatalogItemId()
    {
        SetUser("user-123");
        var request = new FavoritesController.ToggleFavoriteRequest { CatalogItemId = int.MaxValue };
        _favoriteService.ToggleFavoriteAsync("user-123", int.MaxValue, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _sut.Toggle(request, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }
}
