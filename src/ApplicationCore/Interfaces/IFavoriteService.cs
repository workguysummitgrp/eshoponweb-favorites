namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;

public interface IFavoriteService
{
    Task<bool> ToggleFavoriteAsync(string userId, int catalogItemId, CancellationToken ct = default);
    Task<IReadOnlySet<int>> GetUserFavoritesForItemsAsync(string userId, IEnumerable<int> catalogItemIds, CancellationToken ct = default);
    Task<IReadOnlyList<int>> GetUserFavoriteIdsAsync(string userId, CancellationToken ct = default);
}
