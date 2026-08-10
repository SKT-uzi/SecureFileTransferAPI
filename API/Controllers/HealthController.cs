using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResumableFileTransfer.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HealthController : Base.ApiControllerBase
    {
        /// <summary>
        /// An simple api to indicate whether the whole api service is online or not.
        /// </summary>
        [HttpGet]
        public async Task GET()
        {
            await Task.CompletedTask;
        }
    }
}
