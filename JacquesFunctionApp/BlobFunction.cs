using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace JacquesFunctionApp
{
    public class BlobFunction
    {
        [Function("BlobFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            FunctionContext context)
        {
            var log = context.GetLogger<BlobFunction>();
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

            // Retrieve the connection string from environment variables
            var connectionString = Environment.GetEnvironmentVariable("AzureStorageString");
            if (string.IsNullOrEmpty(connectionString))
            {
                log.LogError("Connection string is not set in the environment variables.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerName = "functionscontainer"; 
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Create the container if it doesn't already exist
            await containerClient.CreateIfNotExistsAsync();
            log.LogInformation($"Blob container '{containerName}' is ready.");

            // Get a reference to the blob client
            var blobClient = containerClient.GetBlobClient(file.FileName);

            // Upload the file to Blob Storage
            await blobClient.UploadAsync(file.OpenReadStream(), true);
            log.LogInformation($"File '{file.FileName}' uploaded to blob storage successfully.");

            return new OkObjectResult("File uploaded to blob storage");
        }
    }
}
