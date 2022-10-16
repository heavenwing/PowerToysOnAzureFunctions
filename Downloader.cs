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
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Zyg.PowerToys
{
    public static class Downloader
    {
        [FunctionName("Downloader")]
        [StorageAccount("FilesStorage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Blob("downloads", FileAccess.Write)] BlobContainerClient containerClient,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request: " + req.QueryString.ToString());

            var fileUrl = req.Query["url"];
            if (string.IsNullOrEmpty(fileUrl))
                return new OkObjectResult(new { message = "Please provide url and name (optional) in query string." });

            var fileName = req.Query["name"];
            if (string.IsNullOrEmpty(fileName)) fileName = Path.GetFileName(fileUrl);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            using var client = new HttpClient();
            var sw = Stopwatch.StartNew();
            await using var fileStream = await client.GetStreamAsync(fileUrl);
            var blobCient = containerClient.GetBlobClient(fileName);
            var length = 0;
            await blobCient.UploadAsync(fileStream);
            fileStream.Close();

            sw.Stop();
            return new OkObjectResult(new
            {
                url = blobCient.Uri.AbsoluteUri,
                size = length,
                time = sw.ElapsedMilliseconds / 1000
            });
        }
    }
}
