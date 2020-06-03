using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureCloudStorageRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace tgibbsstoragecrud
{
    public class Crud
    {
        private readonly ITableStorageRepository<StoredJsonEntity> _tableStorageRepository;
        public Crud(ITableStorageRepository<StoredJsonEntity> tableStorageRepository)
        {
            _tableStorageRepository = tableStorageRepository;
        }

        [FunctionName("GetById")]
        public async Task<IActionResult> RunGetId(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{item}/{id}")] HttpRequest req, ExecutionContext context, string item, string id, ILogger log)
        {

            var r = await _tableStorageRepository
                .OverrideTableName(item)
                .GetByIdAsync(id);

            return r != null
                ? (IActionResult)new OkObjectResult(JsonConvert.DeserializeObject(r.Body))
                : new NotFoundResult();
           
        }

        [FunctionName("GetAll")]
        public async Task<IActionResult> RunGet(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{item}")] HttpRequest req, ExecutionContext context, string item, ILogger log)
        {

            var r = await _tableStorageRepository
                .OverrideTableName(item)
                .GetAsync();

            return r != null
                ? (IActionResult)new OkObjectResult(r.Select(o=>JsonConvert.DeserializeObject(o.Body)))
                : new NotFoundResult();
        }

        [FunctionName("Delete")]
        public async Task<IActionResult> RunDelete(
            [HttpTrigger(AuthorizationLevel.Function, "Delete", Route = "{item}/{id}")] HttpRequest req, ExecutionContext context, string item, string id, ILogger log)
        {
            await _tableStorageRepository
                .OverrideTableName(item)
                .DeleteAsync(id, true);

            return new OkResult();
        }

        [FunctionName("Post")]
        public async Task<IActionResult> RunPost(
            [HttpTrigger(AuthorizationLevel.Function, "Post", Route = "{item}")] HttpRequest req, ExecutionContext context, string item, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string id = data.id;

            if (!string.IsNullOrEmpty(id)) return new BadRequestObjectResult($"id must be empty for post, call put if id is known");

            id = Guid.NewGuid().ToString();

            data.id = id;
            var i = JsonConvert.SerializeObject(data);

            var entity = new StoredJsonEntity(item, id) { Body = i };

            var obj = await _tableStorageRepository
                .OverrideTableName(item)
                .InsertOrReplaceAsync(entity);

            return new OkObjectResult(JsonConvert.DeserializeObject(obj.Body));
        }

        [FunctionName("Put")]
        public async Task<IActionResult> RunPut(
            [HttpTrigger(AuthorizationLevel.Function, "Put", Route = "{item}/{id}")] HttpRequest req, ExecutionContext context, string item, string id, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string bodyId = data.id;

            if (bodyId != id) return new BadRequestObjectResult($"Id mismatch {bodyId},{id}");

            var entity = new StoredJsonEntity(item, id) { Body = requestBody };

            var obj = await _tableStorageRepository
                .OverrideTableName(item)
                .InsertOrReplaceAsync(entity);

            return new OkObjectResult(JsonConvert.DeserializeObject(obj.Body));

        }
    }
}
