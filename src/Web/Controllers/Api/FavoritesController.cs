using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.Web.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpPost("toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle([FromBody] ToggleFavoriteRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized();
        }

        if (request.CatalogItemId <= 0)
        {
            return BadRequest();
        }

        var isFavorited = await _favoriteService.ToggleFavoriteAsync(userId, request.CatalogItemId, ct);

        return Ok(new ToggleFavoriteResponse { IsFavorited = isFavorited });
    }

    public class ToggleFavoriteRequest
    {
        public int CatalogItemId { get; set; }
    }

    public class ToggleFavoriteResponse
    {
        public bool IsFavorited { get; set; }
    }
}
