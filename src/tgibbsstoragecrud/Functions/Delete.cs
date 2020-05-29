using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace tgibbsstoragecrud.Functions
{
    public static class Delete
    {
        [FunctionName("Delete")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "Delete", Route = "{item}/{id}")] HttpRequest req, ExecutionContext context, string item, string id, ILogger log)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            var account = CloudStorageAccount.Parse(config["AzureWebJobsStorage"]);

            var entity = new TableEntity(item, id){ETag = "*"};

            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference(item);

            if (table.Exists())
            {
                var deleteOperation = TableOperation.Delete(entity);

                await table.ExecuteAsync(deleteOperation);
                var query = new TableQuery<StoredJsonEntity>().Take(1);
                var items = table.ExecuteQuery(query);
                if (!items.Any())
                {
                    await table.DeleteAsync();
                }
            }
            return new OkResult();
        }
    }
}