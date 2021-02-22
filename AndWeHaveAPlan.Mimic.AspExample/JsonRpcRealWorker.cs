using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AndWeHaveAPlan.Mimic.AspExample
{
    /// <summary>
    /// Example mocking with http request
    /// </summary>
    public class JsonRpcRealWorker : IMimicWorker
    {
        private readonly ILogger<JsonRpcRealWorker> _logger;
        private readonly HttpClient _client;

        public JsonRpcRealWorker(ILogger<JsonRpcRealWorker> logger, HttpClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<T> Mock<T>(string mockMethodName, MockParameter[] args)
        {
            var requestModel = new List<object>();

            foreach (var mockParameter in args)
            {
                requestModel.Add(mockParameter.Value);
            }

            var jsonRpcRequest = new
            {
                id = 1,
                jsonrpc = "2.0",
                method = mockMethodName,
                @params = requestModel
            };

            var jsonRequestString = JsonSerializer.Serialize(jsonRpcRequest);

            var responseMessage = await _client.PostAsync($"api/v4/useful-jsonrpc",
                new StringContent(jsonRequestString, Encoding.UTF8, "application/json"));

            var responseString = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseString))
                return default;
            var response = JsonSerializer.Deserialize<JsonRpcResponse<T>>(responseString,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
            return response.Result;
        }
    }

    public class JsonRpcResponse<T>
    {
        public T Result { get; set; }
    }
}