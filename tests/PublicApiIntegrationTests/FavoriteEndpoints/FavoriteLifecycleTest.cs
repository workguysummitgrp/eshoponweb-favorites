using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PublicApiIntegrationTests.FavoriteEndpoints;

[TestClass]
public class FavoriteLifecycleTest
{
    [TestMethod]
    public async Task AddThenListThenRemoveLifecycle()
    {
        var client = HttpClientHelper.GetNormalUserClient();

        var addPayload = new { CatalogItemId = 50 };
        var addContent = new StringContent(
            JsonSerializer.Serialize(addPayload), Encoding.UTF8, "application/json");
        var addResponse = await client.PostAsync("api/favorites", addContent);
        Assert.AreEqual(HttpStatusCode.Created, addResponse.StatusCode);

        var listResponse = await client.GetAsync("api/favorites");
        Assert.AreEqual(HttpStatusCode.OK, listResponse.StatusCode);
        var listBody = await listResponse.Content.ReadAsStringAsync();
        Assert.IsTrue(
            listBody.Contains("\"catalogItemId\":50") || listBody.Contains("\"CatalogItemId\":50"),
            "List response should contain the added favorite item with CatalogItemId 50.");

        var removeResponse = await client.DeleteAsync("api/favorites/50");
        Assert.AreEqual(HttpStatusCode.NoContent, removeResponse.StatusCode);

        var listAfterRemove = await client.GetAsync("api/favorites");
        Assert.AreEqual(HttpStatusCode.OK, listAfterRemove.StatusCode);
        var listBodyAfterRemove = await listAfterRemove.Content.ReadAsStringAsync();
        Assert.IsFalse(
            listBodyAfterRemove.Contains("\"catalogItemId\":50") || listBodyAfterRemove.Contains("\"CatalogItemId\":50"),
            "List response should no longer contain removed favorite item.");
    }

    [TestMethod]
    public async Task ListResponseContainsFavoritesProperty()
    {
        var client = HttpClientHelper.GetNormalUserClient();

        var response = await client.GetAsync("api/favorites");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(
            body.Contains("favorites") || body.Contains("Favorites"),
            "Response body should contain 'favorites' property.");
    }

    [TestMethod]
    public async Task AddResponseContainsExpectedProperties()
    {
        var client = HttpClientHelper.GetNormalUserClient();
        var payload = new { CatalogItemId = 51 };
        var content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("api/favorites", content);

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(
            body.Contains("catalogItemId") || body.Contains("CatalogItemId"),
            "Response should contain CatalogItemId.");
        Assert.IsTrue(
            body.Contains("dateCreated") || body.Contains("DateCreated"),
            "Response should contain DateCreated.");

        await client.DeleteAsync("api/favorites/51");
    }
}
