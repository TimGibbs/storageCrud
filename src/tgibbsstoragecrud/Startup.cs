using System;
using AzureCloudStorageRepository;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(tgibbsstoragecrud.Startup))]

namespace tgibbsstoragecrud
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {

            builder.Services
                .AddTransient(o => 
                    CloudStorageAccount.Parse(o.GetService<IConfiguration>()["AzureWebJobsStorage"]).CreateCloudTableClient());
            builder.Services
                .AddTransient(typeof(ITableStorageRepository<>), typeof(TableStorageRepository<>));

        }
    }
}
