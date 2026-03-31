using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;
using System.Net;
using System.Threading.Tasks;

namespace PublicApiIntegrationTests.FavoriteEndpoints;

[TestClass]
public class ListFavoritesEndpointTest
{
    [TestMethod]
    public async Task ReturnsOkForAuthenticatedUser()
    {
        var client = HttpClientHelper.GetNormalUserClient();

        var response = await client.GetAsync("api/favorites");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsUnauthorizedForAnonymousUser()
    {
        var client = ProgramTest.NewClient;

        var response = await client.GetAsync("api/favorites");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
