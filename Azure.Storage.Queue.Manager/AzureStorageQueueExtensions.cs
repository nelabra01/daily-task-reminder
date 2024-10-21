using Hangfire.SqlServer;
using Hangfire;
using System;
using Azure.Storage.Queues;
using Microsoft.Extensions.Azure;
using Azure.Identity;

namespace Azure.Storage.Queue.Manager
{
    public static class AzureStorageQueueExtensions
    {
        public static IGlobalConfiguration<SqlServerStorage> UseAzureStorageQueue(
            this IGlobalConfiguration<SqlServerStorage> configuraiton,
            Uri queueUri)
        {
            var queueClient = new QueueClient(queueUri, new DefaultAzureCredential());
            
            var provider = new AzureStorageQueuesProvider(queueClient);
            configuraiton.Entry.QueueProviders.Add(provider, new string[] { "default"});
            
            return configuraiton;
        }
    }
}
