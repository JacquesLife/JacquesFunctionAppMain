using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO; 
using System.Threading.Tasks;

namespace JacquesFunctionApp
{
    public class TableFunction
    {
        private readonly ILogger<TableFunction> _logger;

        public TableFunction(ILogger<TableFunction> logger)
        {
            _logger = logger;
        }

        [Function("TableFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // Read the data from the request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var entity = System.Text.Json.JsonSerializer.Deserialize<TableEntity>(requestBody);

            if (entity == null || string.IsNullOrWhiteSpace(entity.RowKey) || string.IsNullOrWhiteSpace(entity.PartitionKey))
            {
                return new BadRequestObjectResult("Entity must contain valid RowKey and PartitionKey.");
            }

            // Get the connection string from environment variables
            var connectionString = Environment.GetEnvironmentVariable("AzureStorageString");
            var tableServiceClient = new TableServiceClient(connectionString);
            var tableClient = tableServiceClient.GetTableClient("jacquestable");
            await tableClient.CreateIfNotExistsAsync(); // Create the table if it doesn't exist

            // Add the entity to the table
            await tableClient.AddEntityAsync(entity);

            return new OkObjectResult($"Entity added to the table: {entity.RowKey}");
        }
    }

    public class TableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; } 
        public string ETag { get; set; } 
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Country { get; set; }

    }
}


