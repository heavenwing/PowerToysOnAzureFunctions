using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Net.Http;
using System.Net;
using System.Timers;
using System.Diagnostics;

namespace Zyg.PowerToys
{
    public static class Downloader
    {
        [FunctionName("Downloader")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Blob("downloads", FileAccess.Write), StorageAccount("AzureWebJobsStorage")] CloudBlobContainer fileContainer,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request: " + req.QueryString.ToString());

            var fileUrl = req.Query["url"];
            var fileName = req.Query["name"];
            if (string.IsNullOrEmpty(fileName)) fileName = Path.GetFileName(fileUrl);

            const int BUFFER_SIZE = 128 * 1024;
            using (var client = new WebClient())
            {
                var sw = Stopwatch.StartNew();
                using (var stream = await client.OpenReadTaskAsync(fileUrl))
                {
                    var buffer = new byte[BUFFER_SIZE];
                    int bytesRead;
                    var length = 0;

                    var blob = fileContainer.GetBlockBlobReference(fileName);
                    using (var blobStream = await blob.OpenWriteAsync())
                    {
                        do
                        {
                            bytesRead = await stream.ReadAsync(buffer, 0, BUFFER_SIZE);
                            length += bytesRead;
                            await blobStream.WriteAsync(buffer, 0, bytesRead);
                        } while (bytesRead > 0);
                    }
                    stream.Close();

                    sw.Stop();
                    return new OkObjectResult(new
                    {
                        url = blob.Uri.AbsoluteUri,
                        size = length,
                        time = sw.ElapsedMilliseconds / 1000
                    });
                }
            }
        }
    }
}
