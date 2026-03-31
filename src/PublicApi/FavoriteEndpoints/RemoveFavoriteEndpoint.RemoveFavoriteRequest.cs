namespace Microsoft.eShopWeb.PublicApi.FavoriteEndpoints;

public class RemoveFavoriteRequest : BaseRequest
{
    public int CatalogItemId { get; init; }

    public RemoveFavoriteRequest(int catalogItemId)
    {
        CatalogItemId = catalogItemId;
    }
}
