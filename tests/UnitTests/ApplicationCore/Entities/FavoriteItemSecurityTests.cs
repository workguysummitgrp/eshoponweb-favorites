using Microsoft.eShopWeb.ApplicationCore.Entities;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Entities;

/// <summary>
/// Security-focused tests for FavoriteItem entity — OWASP A03.
/// </summary>
public class FavoriteItemSecurityTests
{
    [Fact]
    public void Constructor_ThrowsOnNullUserId()
    {
        Assert.Throws<ArgumentException>(() => new FavoriteItem(null!, 1));
    }

    [Fact]
    public void Constructor_ThrowsOnEmptyUserId()
    {
        Assert.Throws<ArgumentException>(() => new FavoriteItem("", 1));
    }

    [Fact]
    public void Constructor_ThrowsOnZeroCatalogItemId()
    {
        Assert.Throws<ArgumentException>(() => new FavoriteItem("user-1", 0));
    }

    [Fact]
    public void Constructor_ThrowsOnNegativeCatalogItemId()
    {
        Assert.Throws<ArgumentException>(() => new FavoriteItem("user-1", -1));
    }

    [Fact]
    public void Constructor_AcceptsValidInputs()
    {
        var item = new FavoriteItem("user-1", 42);
        Assert.Equal("user-1", item.UserId);
        Assert.Equal(42, item.CatalogItemId);
        Assert.True(item.DateCreated <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Properties_ArePrivateSetters()
    {
        var userIdProp = typeof(FavoriteItem).GetProperty("UserId")!;
        var catalogItemIdProp = typeof(FavoriteItem).GetProperty("CatalogItemId")!;
        var dateCreatedProp = typeof(FavoriteItem).GetProperty("DateCreated")!;

        Assert.False(userIdProp.SetMethod?.IsPublic ?? false);
        Assert.False(catalogItemIdProp.SetMethod?.IsPublic ?? false);
        Assert.False(dateCreatedProp.SetMethod?.IsPublic ?? false);
    }
}
