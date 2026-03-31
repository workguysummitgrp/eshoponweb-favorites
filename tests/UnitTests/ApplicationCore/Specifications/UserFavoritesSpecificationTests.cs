using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Specifications;

public class UserFavoritesSpecificationTests
{
    private readonly string _userId = "spec-user";
    private readonly string _otherUserId = "other-user";

    private List<FavoriteItem> GetTestCollection()
    {
        // Use reflection to set Ids since BaseEntity.Id has protected setter
        var items = new List<FavoriteItem>
        {
            new FavoriteItem(_userId, 1),
            new FavoriteItem(_userId, 5),
            new FavoriteItem(_userId, 10),
            new FavoriteItem(_otherUserId, 1),
            new FavoriteItem(_otherUserId, 20)
        };

        for (int i = 0; i < items.Count; i++)
        {
            typeof(BaseEntity).GetProperty("Id")!.SetValue(items[i], i + 1);
        }

        return items;
    }

    [Fact]
    public void UserIdOnlyOverload_FiltersToMatchingUser()
    {
        var spec = new UserFavoritesSpecification(_userId);

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Equal(3, result.Count);
        Assert.All(result, f => Assert.Equal(_userId, f.UserId));
    }

    [Fact]
    public void UserIdOnlyOverload_ExcludesOtherUsers()
    {
        var spec = new UserFavoritesSpecification(_userId);

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.DoesNotContain(result, f => f.UserId == _otherUserId);
    }

    [Fact]
    public void SingleItemOverload_MatchesExactUserAndItem()
    {
        var spec = new UserFavoritesSpecification(_userId, 5);

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Single(result);
        Assert.Equal(_userId, result[0].UserId);
        Assert.Equal(5, result[0].CatalogItemId);
    }

    [Fact]
    public void SingleItemOverload_ReturnsEmptyWhenItemNotFavorited()
    {
        var spec = new UserFavoritesSpecification(_userId, 999);

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void SingleItemOverload_DoesNotMatchOtherUsersSameItem()
    {
        var spec = new UserFavoritesSpecification(_otherUserId, 1);

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Single(result);
        Assert.Equal(_otherUserId, result[0].UserId);
    }

    [Fact]
    public void ListOverload_FiltersToMatchingUserAndItems()
    {
        var spec = new UserFavoritesSpecification(_userId, new[] { 1, 10 });

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.Equal(_userId, f.UserId));
        Assert.Contains(result, f => f.CatalogItemId == 1);
        Assert.Contains(result, f => f.CatalogItemId == 10);
    }

    [Fact]
    public void ListOverload_IgnoresItemIdsNotInFavorites()
    {
        var spec = new UserFavoritesSpecification(_userId, new[] { 1, 999 });

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Single(result);
        Assert.Equal(1, result[0].CatalogItemId);
    }

    [Fact]
    public void ListOverload_ReturnsEmptyForNoMatchingItems()
    {
        var spec = new UserFavoritesSpecification(_userId, new[] { 888, 999 });

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void ListOverload_ReturnsEmptyForEmptyCatalogItemIds()
    {
        var spec = new UserFavoritesSpecification(_userId, Array.Empty<int>());

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void UserIdOnlyOverload_ReturnsEmptyWhenUserHasNoFavorites()
    {
        var spec = new UserFavoritesSpecification("nonexistent-user");

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Empty(result);
    }
}
