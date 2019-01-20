using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MetricsDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        [HttpGet("run/{name}")]
        public async Task<string> Run(string name)
        {
            await Console.Out.WriteLineAsync($"Run job [{name}]");

            return $"Job [{name}] runned!";
        }
    }
}