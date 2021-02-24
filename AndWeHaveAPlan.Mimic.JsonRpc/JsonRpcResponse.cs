using System.Text.Json.Serialization;

namespace AndWeHaveAPlan.Mimic.JsonRpc
{
    public class JsonRpcResponse<T>
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("result")]
        public T Result { get; set; }

        [JsonPropertyName("error")]
        public JsonRpcError Error { get; set; }

        public bool IsError => Error != null;
    }
}