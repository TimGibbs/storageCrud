using System.Collections.Generic;
using System.Linq;
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
    public static class GetAll
    {
        [FunctionName("GetAll")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{item}")] HttpRequest req, ExecutionContext context, string item, ILogger log)
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
                TableContinuationToken token = null;
                var entities = new List<StoredJsonEntity>();
                do
                {
                    var queryResult = await table.ExecuteQuerySegmentedAsync(new TableQuery<StoredJsonEntity>(), token);
                    entities.AddRange(queryResult.Results);
                    token = queryResult.ContinuationToken;
                } while (token != null);

                if (entities.Any())
                    return new OkObjectResult(entities.Select(o => JsonConvert.DeserializeObject(o.Body)));
            }

            return new NotFoundResult();
        }
    }
}