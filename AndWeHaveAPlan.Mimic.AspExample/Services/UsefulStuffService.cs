using System.Collections.Generic;
using System.Threading.Tasks;

namespace AndWeHaveAPlan.Mimic.AspExample.Services
{
    /// <summary>
    /// Old logic
    /// </summary>
    public class UsefulStuffService : IUsefulStuff
    {
        private static Dictionary<string, string> _values = new Dictionary<string, string>();

        public Task<string> GetSomeValue(string key)
        {
            // Some calculations, DB access, etc
            return Task.FromResult(_values[key]);
        }

        public Task SetSomeValue(string key, string value)
        {
            // Some calculations, DB access, etc
            if (_values.ContainsKey(key))
                _values[key] = value;
            else
                _values.Add(key, value);

            return Task.CompletedTask;
        }
    }
}