using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AndWeHaveAPlan.Mimic.JsonRpc
{
    public class JsonRpcMimicWorker : IMimicWorker
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public JsonRpcMimicWorker(HttpClient client, JsonSerializerOptions jsonSerializerOptions=null)
        {
            _client = client;
            _jsonSerializerOptions = jsonSerializerOptions;
        }

        public async Task<T> DoWork<T>(string mockMethodName, MockParameter[] args)
        {
            var requestModel = new List<object>();

            foreach (var mockParameter in args)
            {
                requestModel.Add(mockParameter.Value);
            }

            var jsonRpcRequest = new JsonRpcRequest
            {
                Method = mockMethodName,
                Params = requestModel
            };

            var jsonRequestString = JsonSerializer.Serialize(jsonRpcRequest, _jsonSerializerOptions);

            var responseMessage = await _client.PostAsync($"",
                new StringContent(jsonRequestString, Encoding.UTF8, "application/json"));

            var responseString = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseString))
                return default;
            var response = JsonSerializer.Deserialize<JsonRpcResponse<T>>(responseString, _jsonSerializerOptions);
            return response.Result;
        }
    }
}
