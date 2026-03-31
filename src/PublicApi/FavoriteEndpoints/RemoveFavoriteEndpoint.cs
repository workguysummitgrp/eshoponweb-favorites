using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

namespace Microsoft.eShopWeb.PublicApi.FavoriteEndpoints;

public class RemoveFavoriteEndpoint(IRepository<FavoriteItem> favoriteRepository)
    : Endpoint<RemoveFavoriteRequest, Results<NoContent, UnauthorizedHttpResult>>
{
    public override void Configure()
    {
        Delete("api/favorites/{catalogItemId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
        {
            d.Produces(StatusCodes.Status204NoContent);
            d.WithTags("FavoriteEndpoints");
        });
    }

    public override async Task<Results<NoContent, UnauthorizedHttpResult>> ExecuteAsync(RemoveFavoriteRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        if (request.CatalogItemId <= 0)
        {
            AddError("CatalogItemId must be a positive integer.");
            ThrowIfAnyErrors(StatusCodes.Status400BadRequest);
        }

        var spec = new UserFavoritesSpecification(userId, request.CatalogItemId);
        var existing = await favoriteRepository.FirstOrDefaultAsync(spec, ct);

        if (existing is not null)
        {
            await favoriteRepository.DeleteAsync(existing, ct);
        }

        return TypedResults.NoContent();
    }
}
