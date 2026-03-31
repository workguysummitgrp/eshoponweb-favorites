using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;
using System.Net;
using System.Threading.Tasks;

namespace PublicApiIntegrationTests.FavoriteEndpoints;

[TestClass]
public class RemoveFavoriteEdgeCasesTest
{
    [TestMethod]
    public async Task DeleteWithNegativeIdReturnsBadRequest()
    {
        var client = HttpClientHelper.GetNormalUserClient();

        var response = await client.DeleteAsync("api/favorites/-1");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteWithZeroIdReturnsBadRequest()
    {
        var client = HttpClientHelper.GetNormalUserClient();

        var response = await client.DeleteAsync("api/favorites/0");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteNonExistentFavoriteIsIdempotent()
    {
        var client = HttpClientHelper.GetNormalUserClient();

        var response = await client.DeleteAsync("api/favorites/9999");

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }
}
