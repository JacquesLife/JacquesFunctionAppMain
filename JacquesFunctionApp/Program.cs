using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.Storage.Blobs;
using Microsoft.Azure.WebJobs.Extensions.Storage.Queues;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddLogging(); // Ensure logging is registered
    })
    .ConfigureWebJobs(b =>
    {
        // Register specific storage bindings
        b.AddHttp();
        b.AddAzureStorageBlobs(); // For Blob Storage functions
        b.AddAzureStorageQueues(); // For Queue Storage functions
        // b.AddAzureStorageQueuesScaleForTrigger(); // Uncomment if scaling is needed for Queue Triggers
    })
    .Build();

host.Run();
