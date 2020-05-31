using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;

namespace tgibbsstoragecrud
{
    public class TableStorageRepository<T> : IRepository<T> where T : TableEntity, new ()
    {
        public string TableName { get; set; }       
        public CloudTableClient TableClient { get; set; }
        public TableStorageRepository(CloudTableClient tableClient)
        {
            TableClient = tableClient;
            TableName = typeof(T).Name;
        }

        public IRepository<T> OverrideTableName(string tableName)
        {
            TableName = tableName;
            return this;
        }

        public async Task<IEnumerable<T>> Get()
        {
            var table = TableClient.GetTableReference(TableName);

            var entities = new List<T>();

            if (table.Exists())
            {
                TableContinuationToken token = null;
                do
                {
                    var queryResult = await table.CreateQuery<T>().ExecuteSegmentedAsync(token);
                    entities.AddRange(queryResult.Results);
                    token = queryResult.ContinuationToken;
                } while (token != null);
            }

            return entities;
        }

        public async Task<IEnumerable<T>> Get(Predicate<T> func)
        {
            var table = TableClient.GetTableReference(TableName);
            var entities = new List<T>();

            if (table.Exists())
            {
                TableContinuationToken token = null;
                do
                {
                    var queryResult = await table.CreateQuery<T>().Where(o=>func(o)).AsTableQuery().ExecuteSegmentedAsync(token);
                    entities.AddRange(queryResult.Results);
                    token = queryResult.ContinuationToken;
                } while (token != null);
            }

            return entities;
        }

        public async Task<T> GetById(string id)
        {
            var table = TableClient.GetTableReference(TableName);
            if (table.Exists())
            {
                var retrieveOperation = TableOperation.Retrieve<T>(TableName, id);
                var result = await table.ExecuteAsync(retrieveOperation);

                if ((result.Result is T resultEntity)) return resultEntity;
            }
            return default;

        }

        public async Task<T> InsertOrReplace(T entity)
        {
            var table = TableClient.GetTableReference(TableName);
            await table.CreateIfNotExistsAsync();

            var insertOrMergeOperation = TableOperation.InsertOrReplace(entity);

            var a = await table.ExecuteAsync(insertOrMergeOperation);
            return a.Result as T;
        }

        public async Task InsertOrReplace(IEnumerable<T> entities)
        {
            var table = TableClient.GetTableReference(TableName);
            await table.CreateIfNotExistsAsync();

            var batch = new TableBatchOperation();
            foreach (var entity in entities)
            {
                batch.InsertOrReplace(entity);
            }
            var a= await table.ExecuteBatchAsync(batch);


        }


        public async Task Delete(string id)
        {
            var table = TableClient.GetTableReference(TableName);
            var entity = await GetById(id);

            if (table.Exists())
            {
                var deleteOperation = TableOperation.Delete(entity);

                await table.ExecuteAsync(deleteOperation);
                await table.DeleteIfEmpty();
            }
        }

        public async Task Delete(Predicate<T> func)
        {
            var table = TableClient.GetTableReference(TableName);
            var entities = await Get(func);

            if (table.Exists())
            {
                var batch = new TableBatchOperation();
                foreach (var tableEntity in entities)
                {
                    batch.Delete(tableEntity);
                }

                await table.ExecuteBatchAsync(batch);
                await table.DeleteIfEmpty();

            }
        }

       
    }
}
