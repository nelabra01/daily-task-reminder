using Hangfire.SqlServer;
using Hangfire;
using System;
using Azure.Storage.Queues;
using Microsoft.Extensions.Azure;
using Azure.Identity;
using Azure.Core;

namespace Azure.Storage.Queue.Manager
{
    public static class AzureStorageQueueExtensions
    {
        public static IGlobalConfiguration<SqlServerStorage> UseAzureStorageQueue(
            this IGlobalConfiguration<SqlServerStorage> configuraiton,
            Uri queueUri, TokenCredential credential)
        {
            var queueClient = new QueueClient(queueUri, credential);
            
            var provider = new AzureStorageQueuesProvider(queueClient);
            configuraiton.Entry.QueueProviders.Add(provider, new string[] { "default"});
            
            return configuraiton;
        }
    }
}
