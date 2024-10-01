using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Storage.Queues; 
using System.Threading.Tasks;

namespace JacquesFunctionApp
{
    public class QueueFunction
    {
        private readonly ILogger<QueueFunction> _logger;

        public QueueFunction(ILogger<QueueFunction> logger)
        {
            _logger = logger;
        }

        [Function("QueueFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // Read the message from the request body
            var message = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(message))
            {
                return new BadRequestObjectResult("Message cannot be empty.");
            }

            // Get the connection string from environment variables
            var connectionString = Environment.GetEnvironmentVariable("AzureStorageString");

            // Create a QueueClient
            var queueClient = new QueueClient(connectionString, "jacquesqueue");
            await queueClient.CreateIfNotExistsAsync(); 

            // Send the message to the queue
            await queueClient.SendMessageAsync(message);

            return new OkObjectResult($"Message '{message}' added to the queue.");
        }
    }
}
