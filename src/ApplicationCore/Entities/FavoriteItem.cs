using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Entities;

public class FavoriteItem : BaseEntity, IAggregateRoot
{
    public string UserId { get; private set; }
    public int CatalogItemId { get; private set; }
    public DateTimeOffset DateCreated { get; private set; }

#pragma warning disable CS8618 // Required by Entity Framework
    private FavoriteItem() { }
#pragma warning restore CS8618

    public FavoriteItem(string userId, int catalogItemId)
    {
        Guard.Against.NullOrWhiteSpace(userId, nameof(userId));
        Guard.Against.NegativeOrZero(catalogItemId, nameof(catalogItemId));
        UserId = userId;
        CatalogItemId = catalogItemId;
        DateCreated = DateTimeOffset.UtcNow;
    }
}
