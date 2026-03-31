using Ardalis.Specification;
using Microsoft.eShopWeb.ApplicationCore.Entities;

namespace Microsoft.eShopWeb.ApplicationCore.Specifications;

public sealed class UserFavoritesSpecification : Specification<FavoriteItem>
{
    public UserFavoritesSpecification(string userId)
    {
        Query.Where(f => f.UserId == userId);
    }

    public UserFavoritesSpecification(string userId, IEnumerable<int> catalogItemIds)
    {
        Query.Where(f => f.UserId == userId && catalogItemIds.Contains(f.CatalogItemId));
    }

    public UserFavoritesSpecification(string userId, int catalogItemId)
    {
        Query.Where(f => f.UserId == userId && f.CatalogItemId == catalogItemId);
    }
}
