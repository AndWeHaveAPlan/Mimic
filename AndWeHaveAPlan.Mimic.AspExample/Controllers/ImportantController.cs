using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AndWeHaveAPlan.Mimic.AspExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImportantController : ControllerBase
    {
        private readonly IUsefulStuff _usefulStuff;

        public ImportantController(IUsefulStuff usefulStuff)
        {
            _usefulStuff = usefulStuff;
        }
        
        [HttpGet]
        public async Task<IEnumerable<string>> Get( )
        {
            await _usefulStuff.SetSomeValue("key1", "some data");

            var str1 = await _usefulStuff.GetSomeValue("key1");
            var str2 = await _usefulStuff.GetSomeValue("key2");

            return new[] {str1, str2};
        }
    }
}