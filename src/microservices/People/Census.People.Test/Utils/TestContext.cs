using System.Net.Http.Headers;
using System.Text;
using Census.People.Test.Utils;
using Census.Shared.Auth;
using Census.Testing;
using Newtonsoft.Json;

namespace Census.People.Test.Utils;

public class TestContext
{
    private readonly HttpClient _client;

    public TestContext(MongoFixture mongoFixture, params string[] roles)
    {
        var factory = new PeopleWebApplicationFactory(mongoFixture.ConnectionString);
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.CreateToken(roles.Length > 0 ? roles : [CensusRoles.Admin]));
    }

    public Task<HttpResponseMessage> Get(string url) => _client.GetAsync(url);

    public Task<HttpResponseMessage> Post(string url, object obj)
    {
        var jsonContent = JsonConvert.SerializeObject(obj);
        var contentString = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        contentString.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return _client.PostAsync(url, contentString);
    }

    public Task<HttpResponseMessage> Put(string url, object obj)
    {
        var jsonContent = JsonConvert.SerializeObject(obj);
        var contentString = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        contentString.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return _client.PutAsync(url, contentString);
    }

    public Task<HttpResponseMessage> Delete(string url) => _client.DeleteAsync(url);
}
