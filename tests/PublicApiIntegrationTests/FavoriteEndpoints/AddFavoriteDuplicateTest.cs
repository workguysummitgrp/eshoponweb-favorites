using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PublicApiIntegrationTests.FavoriteEndpoints;

[TestClass]
public class AddFavoriteDuplicateTest
{
    [TestMethod]
    public async Task DuplicateAddReturnsConflict()
    {
        var client = HttpClientHelper.GetNormalUserClient();
        var payload = new { CatalogItemId = 60 };
        var content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var firstResponse = await client.PostAsync("api/favorites", content);
        Assert.AreEqual(HttpStatusCode.Created, firstResponse.StatusCode);

        var duplicateContent = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var secondResponse = await client.PostAsync("api/favorites", duplicateContent);
        Assert.AreEqual(HttpStatusCode.Conflict, secondResponse.StatusCode);

        await client.DeleteAsync("api/favorites/60");
    }

    [TestMethod]
    public async Task AddWithNegativeCatalogItemIdReturnsBadRequest()
    {
        var client = HttpClientHelper.GetNormalUserClient();
        var payload = new { CatalogItemId = -5 };
        var content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("api/favorites", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
