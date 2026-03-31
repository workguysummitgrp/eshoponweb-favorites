using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;
using System.Net;
using System.Threading.Tasks;

namespace PublicApiIntegrationTests.FavoriteEndpoints;

[TestClass]
public class RemoveFavoriteEndpointTest
{
    [TestMethod]
    public async Task ReturnsNoContentForAuthenticatedUser()
    {
        var client = HttpClientHelper.GetNormalUserClient();

        var response = await client.DeleteAsync("api/favorites/999");

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsUnauthorizedForAnonymousUser()
    {
        var client = ProgramTest.NewClient;

        var response = await client.DeleteAsync("api/favorites/1");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
