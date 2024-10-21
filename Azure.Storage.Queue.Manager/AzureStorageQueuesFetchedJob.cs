using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Hangfire.Logging;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Storage.Queue.Manager
{
    public class AzureStorageQueuesFetchedJob : IFetchedJob
    {
        private readonly QueueMessage _message;
        private readonly QueueClient _queueClient;
        private readonly ILog logger = LogProvider.GetCurrentClassLogger();

        public AzureStorageQueuesFetchedJob(QueueClient queueClient, QueueMessage message)
        {
            _queueClient = queueClient ?? throw new ArgumentNullException(nameof(queueClient));
            _message = message ?? throw new ArgumentNullException(nameof(message));
        }
        public string JobId => _message.Body.ToString();

        public void Dispose()
        {
            return;
        }

        public void RemoveFromQueue()
        {
            _queueClient.DeleteMessage(_message.MessageId, _message.PopReceipt);
        }

        public void Requeue()
        {
            return;
        }
    }
}
