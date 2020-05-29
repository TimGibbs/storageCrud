using System;
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
    public static class Post
    {
        [FunctionName("Post")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "Post", Route = "{item}")] HttpRequest req, ExecutionContext context, string item, ILogger log)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string id = data.id;

            if(!string.IsNullOrEmpty(id)) return new BadRequestObjectResult($"id must be empty for post, call put if id is known");

            id = Guid.NewGuid().ToString();

            data.id = id;
            var i = JsonConvert.SerializeObject(data);

            var entity = new StoredJsonEntity(item, id) { Body = i };

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