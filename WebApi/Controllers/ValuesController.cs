using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AspRelay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private static int _currentId = 0;

        // GET api/values
        [HttpGet]
        public ActionResult<int> Get()
        {
            return Interlocked.Increment(ref _currentId);
        }
    }
}
