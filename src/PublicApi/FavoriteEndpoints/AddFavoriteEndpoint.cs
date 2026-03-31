using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

namespace Microsoft.eShopWeb.PublicApi.FavoriteEndpoints;

public class AddFavoriteEndpoint(IRepository<FavoriteItem> favoriteRepository)
    : Endpoint<AddFavoriteRequest, AddFavoriteResponse>
{
    public override void Configure()
    {
        Post("api/favorites");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
            d.Produces<AddFavoriteResponse>(StatusCodes.Status201Created)
             .Produces(StatusCodes.Status409Conflict)
             .WithTags("FavoriteEndpoints"));
    }

    public override async Task HandleAsync(AddFavoriteRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        if (request.CatalogItemId <= 0)
        {
            AddError("CatalogItemId must be a positive integer.");
            await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
            return;
        }

        var spec = new UserFavoritesSpecification(userId, request.CatalogItemId);
        var existing = await favoriteRepository.FirstOrDefaultAsync(spec, ct);

        if (existing is not null)
        {
            await SendStringAsync("Favorite already exists", StatusCodes.Status409Conflict, ct);
            return;
        }

        var favorite = new FavoriteItem(userId, request.CatalogItemId);
        await favoriteRepository.AddAsync(favorite, ct);

        var response = new AddFavoriteResponse(request.CorrelationId())
        {
            CatalogItemId = favorite.CatalogItemId,
            DateCreated = favorite.DateCreated
        };

        await SendAsync(response, StatusCodes.Status201Created, ct);
    }
}
