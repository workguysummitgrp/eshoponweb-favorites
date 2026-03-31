using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Web.Extensions;
using Microsoft.eShopWeb.Web.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.eShopWeb.Web.Services;

public class CachedCatalogViewModelService : ICatalogViewModelService
{
    private readonly IMemoryCache _cache;
    private readonly CatalogViewModelService _catalogViewModelService;
    private readonly IFavoriteService _favoriteService;

    public CachedCatalogViewModelService(IMemoryCache cache,
        CatalogViewModelService catalogViewModelService,
        IFavoriteService favoriteService)
    {
        _cache = cache;
        _catalogViewModelService = catalogViewModelService;
        _favoriteService = favoriteService;
    }

    public async Task<IEnumerable<SelectListItem>> GetBrands()
    {
        return (await _cache.GetOrCreateAsync(CacheHelpers.GenerateBrandsCacheKey(), async entry =>
                {
                    entry.SlidingExpiration = CacheHelpers.DefaultCacheDuration;
                    return await _catalogViewModelService.GetBrands();
                })) ?? new List<SelectListItem>();
    }

    public async Task<CatalogIndexViewModel> GetCatalogItems(int pageIndex, int itemsPage, int? brandId, int? typeId, string? userId = null)
    {
        var cacheKey = CacheHelpers.GenerateCatalogItemCacheKey(pageIndex, Constants.ITEMS_PER_PAGE, brandId, typeId);

        var cachedResult = (await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = CacheHelpers.DefaultCacheDuration;
            return await _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, brandId, typeId, null);
        })) ?? new CatalogIndexViewModel();

        if (!string.IsNullOrEmpty(userId) && cachedResult.CatalogItems.Count > 0)
        {
            var itemIds = cachedResult.CatalogItems.Select(i => i.Id);
            var favoritedIds = await _favoriteService.GetUserFavoritesForItemsAsync(userId, itemIds);

            return new CatalogIndexViewModel
            {
                CatalogItems = cachedResult.CatalogItems.Select(i => new CatalogItemViewModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    PictureUri = i.PictureUri,
                    Price = i.Price,
                    IsFavorited = favoritedIds.Contains(i.Id)
                }).ToList(),
                Brands = cachedResult.Brands,
                Types = cachedResult.Types,
                BrandFilterApplied = cachedResult.BrandFilterApplied,
                TypesFilterApplied = cachedResult.TypesFilterApplied,
                PaginationInfo = cachedResult.PaginationInfo
            };
        }

        return cachedResult;
    }

    public async Task<IEnumerable<SelectListItem>> GetTypes()
    {
        return (await _cache.GetOrCreateAsync(CacheHelpers.GenerateTypesCacheKey(), async entry =>
        {
            entry.SlidingExpiration = CacheHelpers.DefaultCacheDuration;
            return await _catalogViewModelService.GetTypes();
        })) ?? new List<SelectListItem>();
    }
}
