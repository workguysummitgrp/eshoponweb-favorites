namespace Microsoft.eShopWeb.PublicApi.FavoriteEndpoints;

public class ListFavoritesResponse : BaseResponse
{
    public ListFavoritesResponse(Guid correlationId) : base(correlationId) { }
    public ListFavoritesResponse() { }

    public List<FavoriteItemDto> Favorites { get; set; } = new();
}
