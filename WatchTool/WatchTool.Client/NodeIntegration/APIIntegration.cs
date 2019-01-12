using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WatchTool.Client.NodeIntegration
{
    public class APIIntegration
    {
        private readonly string apiEndPoint;

        public APIIntegration()
        {
            this.apiEndPoint = $"http://localhost:{ClientConfiguration.ApiPort}/";
        }

        public async Task<string> GetConsoleOutput(CancellationToken token)
        {
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(apiEndPoint, token))
            using (HttpContent content = response.Content)
            {
                // ... Read the string.
                string result = await content.ReadAsStringAsync().ConfigureAwait(false);

                return result;
            }
        }

        public async Task StopNode()
        {
            string endpoint = apiEndPoint + "api/Node/stop";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Content = new StringContent("true",
                Encoding.UTF8,
                "application/json");//CONTENT-TYPE header

            await client.SendAsync(request);
        }
    }
}
