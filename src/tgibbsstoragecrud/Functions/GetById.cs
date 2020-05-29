using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace tgibbsstoragecrud.Functions
{
    public static class GetById
    {
        [FunctionName("GetById")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{item}/{id}")] HttpRequest req, ExecutionContext context, string item, string id, ILogger log)
        {

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            var account = CloudStorageAccount.Parse(config["AzureWebJobsStorage"]);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference(item);

            if (table.Exists())
            {
                var retrieveOperation = TableOperation.Retrieve<StoredJsonEntity>(item, id);
                var result = await table.ExecuteAsync(retrieveOperation);

                if ((result.Result is StoredJsonEntity resultEntity)) return new OkObjectResult(JsonConvert.DeserializeObject(resultEntity.Body));
            }

            

            return new NotFoundResult();
        }
    }
}
