using Census.People.Api;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Census.People.Test.Utils
{
    public class TestContext
    {
        public HttpClient Client { get; set; }

        public TestServer Server { get; set; }

        public TestContext()
        {
            SetupClient();
        }

        private void SetupClient()
        {
            var host = WebHost.CreateDefaultBuilder(new string[] { })
                .UseStartup<TestStartup>();

            Server = new TestServer(host);
            Client = Server.CreateClient();
        }

        public async Task<HttpResponseMessage> Get(string url)
        {
            return await Client.GetAsync(url);
        }

        public async Task<HttpResponseMessage> Post(string url, Object obj)
        {
            var jsonContent = JsonConvert.SerializeObject(obj);
            var contentString = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return await Client.PostAsync(url, contentString);
        }

        public async Task<HttpResponseMessage> Put(string url, Object obj)
        {
            var jsonContent = JsonConvert.SerializeObject(obj);
            var contentString = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return await Client.PutAsync(url, contentString);
        }

        public async Task<HttpResponseMessage> Delete(string url, string token)
        {
            return await Client.DeleteAsync(url);
        }
    }
}
