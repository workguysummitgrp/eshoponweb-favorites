namespace Microsoft.eShopWeb.PublicApi.FavoriteEndpoints;

public class AddFavoriteResponse : BaseResponse
{
    public AddFavoriteResponse(Guid correlationId) : base(correlationId) { }
    public AddFavoriteResponse() { }

    public int CatalogItemId { get; set; }
    public DateTimeOffset DateCreated { get; set; }
}
