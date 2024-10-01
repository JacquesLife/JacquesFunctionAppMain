using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

public class TableFunction
{
    private readonly TableServiceClient _tableServiceClient;

    public TableFunction(TableServiceClient tableServiceClient)
    {
        _tableServiceClient = tableServiceClient;
    }

    [Function("TableFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("TableFunction");

        // Log the incoming request
        logger.LogInformation("Processing request for storing data.");

        // Read the request body
        string requestBody;
        using (var reader = new StreamReader(req.Body))
        {
            requestBody = await reader.ReadToEndAsync();
        }

        // Parse form data
        var formData = System.Web.HttpUtility.ParseQueryString(requestBody);
        var tableName = formData["tableName"];
        var partitionKey = formData["partitionKey"];
        var rowKey = formData["rowKey"];
        var data = formData["data"];

        if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(partitionKey) ||
            string.IsNullOrWhiteSpace(rowKey) || string.IsNullOrWhiteSpace(data))
        {
            logger.LogWarning("Invalid input data.");
            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            errorResponse.WriteString("{\"error\": \"Invalid input data.\"}");
            return errorResponse;
        }

        // Create the table if it does not exist
        var tableClient = _tableServiceClient.GetTableClient(tableName);
        await tableClient.CreateIfNotExistsAsync();

        // Create an entity to store
        var entity = new TableEntity(partitionKey, rowKey)
        {
            { "Data", data }
        };

        // Store the entity in Azure Table Storage
        await tableClient.AddEntityAsync(entity);

        // Create a success response
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        response.WriteString("{\"message\": \"Data stored successfully.\"}");
        return response;
    }
}
