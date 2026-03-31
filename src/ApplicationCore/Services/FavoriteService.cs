using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IRepository<FavoriteItem> _favoriteRepository;
    private readonly IAppLogger<FavoriteService> _logger;

    public FavoriteService(IRepository<FavoriteItem> favoriteRepository,
        IAppLogger<FavoriteService> logger)
    {
        _favoriteRepository = favoriteRepository;
        _logger = logger;
    }

    public async Task<bool> ToggleFavoriteAsync(string userId, int catalogItemId, CancellationToken ct = default)
    {
        Guard.Against.NullOrWhiteSpace(userId, nameof(userId));
        Guard.Against.NegativeOrZero(catalogItemId, nameof(catalogItemId));

        var spec = new UserFavoritesSpecification(userId, catalogItemId);
        var existing = await _favoriteRepository.FirstOrDefaultAsync(spec, ct);

        if (existing is not null)
        {
            await _favoriteRepository.DeleteAsync(existing, ct);
            _logger.LogInformation("User {UserId} unfavorited CatalogItem {CatalogItemId}.", userId, catalogItemId);
            return false;
        }

        var favorite = new FavoriteItem(userId, catalogItemId);
        await _favoriteRepository.AddAsync(favorite, ct);
        _logger.LogInformation("User {UserId} favorited CatalogItem {CatalogItemId}.", userId, catalogItemId);
        return true;
    }

    public async Task<IReadOnlySet<int>> GetUserFavoritesForItemsAsync(string userId, IEnumerable<int> catalogItemIds, CancellationToken ct = default)
    {
        Guard.Against.NullOrWhiteSpace(userId, nameof(userId));

        var itemIdsList = catalogItemIds as IList<int> ?? catalogItemIds.ToList();
        if (itemIdsList.Count == 0)
        {
            return new HashSet<int>();
        }

        var spec = new UserFavoritesSpecification(userId, itemIdsList);
        var favorites = await _favoriteRepository.ListAsync(spec, ct);

        return favorites.Select(f => f.CatalogItemId).ToHashSet();
    }

    public async Task<IReadOnlyList<int>> GetUserFavoriteIdsAsync(string userId, CancellationToken ct = default)
    {
        Guard.Against.NullOrWhiteSpace(userId, nameof(userId));

        var spec = new UserFavoritesSpecification(userId);
        var favorites = await _favoriteRepository.ListAsync(spec, ct);

        return favorites.Select(f => f.CatalogItemId).ToList().AsReadOnly();
    }
}
