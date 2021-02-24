using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;

namespace AndWeHaveAPlan.Mimic.JsonRpc
{
    public class JsonRpcRequest
    {
        private static int _id = 1;

        private static uint NewId => (uint)Interlocked.Increment(ref _id);

        [JsonPropertyName("id")]
        public long Id { get; set; } = NewId;

        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("params")]
        public List<object> Params { get; set; }
    }
}