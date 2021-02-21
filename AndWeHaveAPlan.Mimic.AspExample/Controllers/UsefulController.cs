using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AndWeHaveAPlan.Mimic.AspExample.Controllers
{
    /// <summary>
    /// Migrated to remote service logic
    /// </summary>
    [ApiController]
    [Route("api/v4/[controller]")]
    public class UsefulController : ControllerBase, IUsefulStuff
    {
        [HttpPost("[action]")]
        [Produces("application/json")]
        public async Task<string> GetSomeValue(string key)
        {
            // Some calculations, DB access, etc
            return "data";
        }

        [HttpPost("[action]")]
        public async Task SetSomeValue(string key, string value)
        {
            // Some calculations, DB access, etc
        }
    }
}