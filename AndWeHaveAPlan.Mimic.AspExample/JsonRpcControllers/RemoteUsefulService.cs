using System.Collections.Generic;
using System.Threading.Tasks;
using EdjCase.JsonRpc.Router;

namespace AndWeHaveAPlan.Mimic.AspExample.JsonRpcControllers
{
    /// <summary>
    /// migrated logic
    /// </summary>
    [RpcRoute("/api/v4/useful-jsonrpc")]
    public class RemoteUsefulStuffServiceController : RpcController, IUsefulStuff
    {
        private static Dictionary<string, string> _values = new();

        public async Task<string> GetSomeValue(string key)
        {
            // Some calculations, DB access, etc
            var ret = _values.ContainsKey(key) ? _values[key] : null;
            return ret;
        }

        public async Task SetSomeValue(string key, string value)
        {
            // Some calculations, DB access, etc
            if (_values.ContainsKey(key))
                _values[key] = value;
            else
                _values.Add(key, value);
        }
    }
}