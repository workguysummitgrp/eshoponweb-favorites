using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

namespace Microsoft.eShopWeb.PublicApi.FavoriteEndpoints;

public class ListFavoritesEndpoint(IRepository<FavoriteItem> favoriteRepository)
    : EndpointWithoutRequest<ListFavoritesResponse>
{
    public override void Configure()
    {
        Get("api/favorites");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
            d.Produces<ListFavoritesResponse>()
             .WithTags("FavoriteEndpoints"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var spec = new UserFavoritesSpecification(userId);
        var favorites = await favoriteRepository.ListAsync(spec, ct);

        var response = new ListFavoritesResponse(Guid.NewGuid())
        {
            Favorites = favorites.Select(f => new FavoriteItemDto
            {
                CatalogItemId = f.CatalogItemId,
                DateCreated = f.DateCreated
            }).ToList()
        };

        await SendOkAsync(response, ct);
    }
}
