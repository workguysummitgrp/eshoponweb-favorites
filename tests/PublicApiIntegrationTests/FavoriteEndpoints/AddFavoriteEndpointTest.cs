using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PublicApiIntegrationTests.FavoriteEndpoints;

[TestClass]
public class AddFavoriteEndpointTest
{
    [TestMethod]
    public async Task ReturnsCreatedForValidRequest()
    {
        var client = HttpClientHelper.GetNormalUserClient();
        var payload = new { CatalogItemId = 1 };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("api/favorites", content);

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsUnauthorizedForAnonymousUser()
    {
        var client = ProgramTest.NewClient;
        var payload = new { CatalogItemId = 1 };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("api/favorites", content);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsBadRequestForInvalidCatalogItemId()
    {
        var client = HttpClientHelper.GetNormalUserClient();
        var payload = new { CatalogItemId = 0 };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("api/favorites", content);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
