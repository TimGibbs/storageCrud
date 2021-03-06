﻿using Microsoft.Azure.Cosmos.Table;

namespace tgibbsstoragecrud
{
    public class StoredJsonEntity : TableEntity
    {
        public StoredJsonEntity()
        {
        }

        public StoredJsonEntity(string type, string id)
        {
            PartitionKey = type;
            RowKey = id;
        }

        public string Body { get; set; }
    }
}
