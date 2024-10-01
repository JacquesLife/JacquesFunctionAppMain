using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace JacquesFunctionApp
{
    public class FileShareFunction
    {
        [Function("FileShareFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            FunctionContext context)
        {
            var log = context.GetLogger<FileShareFunction>();
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Check if the request contains a file
            if (!req.HasFormContentType)
            {
                return new BadRequestObjectResult("Request does not contain a valid form.");
            }

            var formCollection = await req.ReadFormAsync();
            var file = formCollection.Files["file"];

            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("File must be provided.");
            }

            var connectionString = Environment.GetEnvironmentVariable("AzureStorageString");
            var shareName = "jacquesfileshare"; // name of fileshare 
            var shareClient = new ShareClient(connectionString, shareName);

            // Create the file share if it does not exist
            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(file.FileName);

            // Create the file in the file share
            using (var stream = file.OpenReadStream())
            {
                await fileClient.CreateAsync(file.Length);
                await fileClient.UploadAsync(stream);
            }

            return new OkObjectResult("File uploaded to file share");
        }
    }
}
