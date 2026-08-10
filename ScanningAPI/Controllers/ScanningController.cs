using Microsoft.AspNetCore.Mvc;
using ResumableFileTransfer.ScanningAPI.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ResumableFileTransfer.ScanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScanningController : ControllerBase
    {
        private static readonly string directoryPath =
            Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            await Task.CompletedTask;
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult<bool>> Post([FromBody] ScanningFileRequest fileInfo)
        {
            try
            {
                if (fileInfo is null || string.IsNullOrWhiteSpace(fileInfo.Name) || string.IsNullOrWhiteSpace(fileInfo.FileUrl))
                {
                    return BadRequest();
                }

                using HttpClient client = new();
                var content = await client.GetByteArrayAsync(fileInfo.FileUrl);

                // true: OK, false: Bad
                if (content.Length == 0)
                {
                    return Ok(true);
                }

                var flag = false;
                using (var application = AmsiContext.Create("ScanningAPI"))
                using (var session = application.CreateSession())
                {
                    flag = session.IsMalware(content, fileInfo.Name);
                }

                // true: OK, false: Bad
                return Ok(!flag);
            }
            catch (Exception ex)
            {
                await WriteLogAsync(ex.Message);
                await WriteLogAsync(ex.StackTrace);

                throw;
            }
            finally
            {
                GC.Collect();
            }
        }

        public string GetMemory()
        {
            Process proc = Process.GetCurrentProcess();
            long b = proc.PrivateMemorySize64;
            for (int i = 0; i < 2; i++)
            {
                b /= 1024;
            }
            return b + "MB";
        }

        public async Task WriteLogAsync(string content)
        {
            using StreamWriter sw = new(Path.Combine(directoryPath, "log.txt"), true);
            await sw.WriteLineAsync($"{DateTime.Now:MM/dd/yyyy HH:mm:ss}\t{content}");
            await sw.FlushAsync();
        }
    }
}
