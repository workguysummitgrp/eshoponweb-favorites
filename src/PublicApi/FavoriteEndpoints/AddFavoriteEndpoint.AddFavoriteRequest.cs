namespace Microsoft.eShopWeb.PublicApi.FavoriteEndpoints;

public class AddFavoriteRequest : BaseRequest
{
    public int CatalogItemId { get; set; }
}
