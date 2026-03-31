using Microsoft.eShopWeb.ApplicationCore.Entities;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Entities.FavoriteItemTests;

public class Construction
{
    [Fact]
    public void SetsPropertiesWhenValid()
    {
        var item = new FavoriteItem("user-1", 42);

        Assert.Equal("user-1", item.UserId);
        Assert.Equal(42, item.CatalogItemId);
    }

    [Fact]
    public void SetsDateCreatedCloseToUtcNow()
    {
        var before = DateTimeOffset.UtcNow;
        var item = new FavoriteItem("user-1", 1);
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(item.DateCreated, before, after);
    }

    [Fact]
    public void ThrowsWhenUserIdIsNull()
    {
        Assert.Throws<ArgumentException>(() => new FavoriteItem(null!, 1));
    }

    [Fact]
    public void ThrowsWhenUserIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new FavoriteItem("", 1));
    }

    [Fact]
    public void ThrowsWhenUserIdIsWhitespace()
    {
        Assert.Throws<ArgumentException>(() => new FavoriteItem("   ", 1));
    }

    [Fact]
    public void ThrowsWhenCatalogItemIdIsZero()
    {
        Assert.Throws<ArgumentException>(() => new FavoriteItem("user-1", 0));
    }

    [Fact]
    public void ThrowsWhenCatalogItemIdIsNegative()
    {
        Assert.Throws<ArgumentException>(() => new FavoriteItem("user-1", -5));
    }

    [Fact]
    public void AcceptsMaxIntCatalogItemId()
    {
        var item = new FavoriteItem("user-1", int.MaxValue);

        Assert.Equal(int.MaxValue, item.CatalogItemId);
    }
}
