using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AndWeHaveAPlan.Mimic.AspExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImportantController : ControllerBase
    {
        private readonly IUsefulStuff _usefulStuff;

        private readonly ILogger<ImportantController> _logger;

        public ImportantController(IUsefulStuff usefulStuff)
        {
            _usefulStuff = usefulStuff;
        }

        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            await _usefulStuff.SetSomeValue("key1", "some data");

            var str1 = await _usefulStuff.GetSomeValue("key2");
            var str2 = await _usefulStuff.GetSomeValue("key3");

            return new[] {str1, str2};
        }
    }
}