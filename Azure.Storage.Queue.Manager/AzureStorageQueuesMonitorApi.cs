using Azure.Storage.Queues;
using Hangfire.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Azure.Storage.Queue.Manager
{
    public class AzureStorageQueuesMonitorApi : IPersistentJobQueueMonitoringApi
    {
        private readonly QueueClient _queueClient;
        private readonly string[] _queues;

        public AzureStorageQueuesMonitorApi(QueueClient queueClient)
        {
            _queueClient = queueClient ?? throw new ArgumentNullException(nameof(queueClient));
            _queues = new string[] { };
        }

        public EnqueuedAndFetchedCountDto GetEnqueuedAndFetchedCount(string queue)
        {
            return new EnqueuedAndFetchedCountDto
            {
                EnqueuedCount = _queueClient.GetProperties().Value.ApproximateMessagesCount,
                FetchedCount = null
            };
        }

        public IEnumerable<long> GetFetchedJobIds(string queue, int from, int perPage) => Enumerable.Empty<long>();

        public IEnumerable<long> GetEnqueuedJobIds(string queue, int from, int perPage)
        {
            var result = new List<long>();

            var end = from + perPage;

            var peekedMessages = _queueClient.PeekMessages().Value;
            for (var current = 0; current < peekedMessages.Length; current++)
            {
                if(current >= from && current < end)
                {
                    if (peekedMessages[current] == null) continue;

                    result.Add(long.Parse(peekedMessages[current].Body.ToString()));
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        public IEnumerable<string> GetQueues() => _queues;
    }
}