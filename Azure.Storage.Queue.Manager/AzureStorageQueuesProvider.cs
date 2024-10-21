using Azure.Storage.Queues;
using Hangfire.SqlServer;

namespace Azure.Storage.Queue.Manager
{
    public class AzureStorageQueuesProvider : IPersistentJobQueueProvider
    {
        private readonly AzureStorageQueuesJobQueue _azureStorageQueuesJobQueue;
        private readonly AzureStorageQueuesMonitorApi _azureStorageQueuesMonitorApi;

        public AzureStorageQueuesProvider(QueueClient queueClient)
        {
            _azureStorageQueuesJobQueue = new AzureStorageQueuesJobQueue(queueClient);
            _azureStorageQueuesMonitorApi = new AzureStorageQueuesMonitorApi(queueClient);
        }
        public IPersistentJobQueue GetJobQueue() => _azureStorageQueuesJobQueue;

        public IPersistentJobQueueMonitoringApi GetJobQueueMonitoringApi() => _azureStorageQueuesMonitorApi;
    }
}
