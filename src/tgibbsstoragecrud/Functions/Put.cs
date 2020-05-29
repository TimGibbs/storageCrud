using System.IO;
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
    public static class Put
    {
        [FunctionName("Put")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "Put", Route = "{item}/{id}")] HttpRequest req, ExecutionContext context, string item, string id, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string bodyId = data.id;

            if(bodyId != id) return new BadRequestObjectResult($"Id mismatch {bodyId},{id}");

            var entity = new StoredJsonEntity(item,id){Body = requestBody};

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            var account = CloudStorageAccount.Parse(config["AzureWebJobsStorage"]);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference(item);
            await table.CreateIfNotExistsAsync();

            var insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

            var result = await table.ExecuteAsync(insertOrMergeOperation);
            var insertedEntity = result.Result as StoredJsonEntity;
            return new OkObjectResult(JsonConvert.DeserializeObject(insertedEntity.Body));
        }
    }
}