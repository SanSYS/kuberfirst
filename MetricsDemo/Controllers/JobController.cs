using System;
using System.Threading;
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

        [HttpGet("kill/me")]
        public async void Kill()
        {
            throw new Exception("Selfkill");
        }

        static bool deadlock;

        [HttpGet("alive/{cmd}")]
        public string Kill(string cmd)
        {
            if (cmd == "deadlock")
            {
                deadlock = true;
                return "Deadlocked";
            }

            if (deadlock)
                Thread.Sleep(123 * 1000);

            return deadlock ? "Deadlocked!!!" : "Alive";
        }
    }
}