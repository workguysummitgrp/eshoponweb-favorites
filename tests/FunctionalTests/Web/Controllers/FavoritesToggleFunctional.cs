using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Controllers;

/// <summary>
/// Functional tests for the FavoritesController toggle endpoint and catalog page heart icon rendering.
/// Covers US-005 (AJAX toggle), US-006 (disabled heart for anonymous), US-013 (accessibility markup).
/// </summary>
[Collection("Sequential")]
public class FavoritesToggleFunctional(TestApplication factory) : IClassFixture<TestApplication>
{
    private readonly HttpClient _noRedirectClient = factory.CreateClient(
        new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    public HttpClient Client { get; } = factory.CreateClient();

    [Fact]
    public async Task AnonymousPostToToggleIsRejected()
    {
        var payload = new StringContent(
            """{"catalogItemId": 5}""",
            Encoding.UTF8,
            "application/json");

        var response = await _noRedirectClient.PostAsync(
            "/api/favorites/toggle", payload, TestContext.Current.CancellationToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Redirect,
            $"Expected 401 or 302 for anonymous toggle, got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task CatalogPageRendersHeartIconMarkup()
    {
        var response = await Client.GetAsync("/", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("esh-favorite-heart", content);
    }

    [Fact]
    public async Task CatalogPageRendersDisabledHeartsForAnonymous()
    {
        var response = await Client.GetAsync("/", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("esh-favorite-heart--disabled", content);
    }

    [Fact]
    public async Task CatalogPageHeartIconsHaveAccessibilityAttributes()
    {
        var response = await Client.GetAsync("/", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("aria-pressed=", content);
        Assert.Contains("aria-label=", content);
        Assert.Contains("data-catalog-item-id=", content);
    }

    [Fact]
    public async Task AnonymousToggleDoesNotReturnOk()
    {
        var payload = new StringContent(
            """{"catalogItemId": 1}""",
            Encoding.UTF8,
            "application/json");

        var response = await _noRedirectClient.PostAsync(
            "/api/favorites/toggle", payload, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
