using System.Text.Json.Serialization;

namespace AndWeHaveAPlan.Mimic.JsonRpc
{
    public class JsonRpcError
    {
        [JsonPropertyName("code")]
        public long Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}