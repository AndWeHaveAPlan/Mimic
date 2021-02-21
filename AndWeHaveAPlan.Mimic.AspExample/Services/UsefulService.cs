using System.Threading.Tasks;

namespace AndWeHaveAPlan.Mimic.AspExample.Services
{
    /// <summary>
    /// Old logic
    /// </summary>
    public class UsefulStuff : IUsefulStuff
    {
        public async Task<string> GetSomeValue(string key)
        {
            // Some calculations, DB access, etc
            return "data";
        }

        public async Task SetSomeValue(string key, string value)
        {
            // Some calculations, DB access, etc
        }
    }
}