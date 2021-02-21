using System;
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
    public class HttpRealWorker : IMimicWorker
    {
        private readonly ILogger<HttpRealWorker> _logger;
        private readonly HttpClient _client;

        public HttpRealWorker(ILogger<HttpRealWorker> logger, HttpClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<T> Mock<T>(string mockMethodName, MockParameter[] args)
        {
            var requestModel = new Dictionary<string, Object>();

            foreach (var mockParameter in args)
            {
                requestModel.Add(mockParameter.Name, mockParameter.Value);
            }

            var jsonRequestString = JsonSerializer.Serialize(requestModel);

            var responseMessage = await _client.PostAsync($"api/v4/useful/{mockMethodName}",
                new StringContent(jsonRequestString, Encoding.UTF8, "application/json"));

            var responseString = await responseMessage.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseString))
                return default;

            var response = JsonSerializer.Deserialize<T>(responseString);

            return response;
        }
    }
}