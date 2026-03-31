using Ardalis.Specification;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using NSubstitute;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Services;

/// <summary>
/// Security-focused tests for FavoriteService — OWASP A01, A03, A09.
/// </summary>
public class FavoriteServiceSecurityTests
{
    private readonly IRepository<FavoriteItem> _repository = Substitute.For<IRepository<FavoriteItem>>();
    private readonly IAppLogger<FavoriteService> _logger = Substitute.For<IAppLogger<FavoriteService>>();
    private readonly FavoriteService _sut;

    public FavoriteServiceSecurityTests()
    {
        _sut = new FavoriteService(_repository, _logger);
    }

    // --- A03: Input Validation ---

    [Fact]
    public async Task ToggleFavoriteAsync_ThrowsOnNullUserId()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.ToggleFavoriteAsync(null!, 1));
    }

    [Fact]
    public async Task ToggleFavoriteAsync_ThrowsOnEmptyUserId()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.ToggleFavoriteAsync("", 1));
    }

    [Fact]
    public async Task ToggleFavoriteAsync_ThrowsOnWhitespaceUserId()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.ToggleFavoriteAsync("   ", 1));
    }

    [Fact]
    public async Task ToggleFavoriteAsync_ThrowsOnZeroCatalogItemId()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.ToggleFavoriteAsync("user-1", 0));
    }

    [Fact]
    public async Task ToggleFavoriteAsync_ThrowsOnNegativeCatalogItemId()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.ToggleFavoriteAsync("user-1", -100));
    }

    [Fact]
    public async Task GetUserFavoritesForItemsAsync_ThrowsOnNullUserId()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.GetUserFavoritesForItemsAsync(null!, new[] { 1 }));
    }

    [Fact]
    public async Task GetUserFavoriteIdsAsync_ThrowsOnNullUserId()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.GetUserFavoriteIdsAsync(null!));
    }

    // --- A01: User Isolation ---

    [Fact]
    public async Task ToggleFavoriteAsync_QueriesRepositoryWithUserIdFilter()
    {
        _repository.FirstOrDefaultAsync(Arg.Any<Specification<FavoriteItem>>(), Arg.Any<CancellationToken>())
            .Returns((FavoriteItem?)null);

        await _sut.ToggleFavoriteAsync("user-alpha", 42);

        await _repository.Received(1)
            .FirstOrDefaultAsync(Arg.Any<Specification<FavoriteItem>>(), Arg.Any<CancellationToken>());
        await _repository.Received(1)
            .AddAsync(Arg.Is<FavoriteItem>(f => f.UserId == "user-alpha" && f.CatalogItemId == 42), Arg.Any<CancellationToken>());
    }

    // --- A09: Security Logging ---

    [Fact]
    public async Task ToggleFavoriteAsync_LogsWhenFavoriteAdded()
    {
        _repository.FirstOrDefaultAsync(Arg.Any<Specification<FavoriteItem>>(), Arg.Any<CancellationToken>())
            .Returns((FavoriteItem?)null);

        await _sut.ToggleFavoriteAsync("user-1", 10);

        _logger.Received(1).LogInformation(
            Arg.Is<string>(s => s.Contains("favorited")),
            Arg.Any<object[]>());
    }

    [Fact]
    public async Task ToggleFavoriteAsync_LogsWhenFavoriteRemoved()
    {
        var existing = new FavoriteItem("user-1", 10);
        _repository.FirstOrDefaultAsync(Arg.Any<Specification<FavoriteItem>>(), Arg.Any<CancellationToken>())
            .Returns(existing);

        await _sut.ToggleFavoriteAsync("user-1", 10);

        _logger.Received(1).LogInformation(
            Arg.Is<string>(s => s.Contains("unfavorited")),
            Arg.Any<object[]>());
    }
}
