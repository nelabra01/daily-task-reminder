using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Hangfire.Logging;
using Hangfire.SqlServer;
using Hangfire.Storage;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Storage.Queue.Manager
{
    public class AzureStorageQueuesJobQueue : IPersistentJobQueue
    {
        private readonly QueueClient _queueClient;
        private readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        public AzureStorageQueuesJobQueue(QueueClient queueClient)
        {
            _queueClient = queueClient;
        }
        public IFetchedJob Dequeue(string[] queues, CancellationToken cancellationToken)
        {
            QueueMessage message;
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    message = _queueClient.ReceiveMessage(cancellationToken: cancellationToken);

                    if (message != null)
                    {
                        _logger.Info($"{DateTime.Now} - Dequeue message with body {message.Body.ToString()} from queue {_queueClient.Uri}");
                        return new AzureStorageQueuesFetchedJob(_queueClient, message);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                _logger.Warn($"Cancellation was requested for queue {_queueClient.Uri}");
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"Failed to dequeue message from queue {_queueClient.Uri}", ex);
            }
            return null;
        }

        public void Enqueue(IDbConnection connection, string queue, string jobId)
        {
            if(jobId == null)
            {
                throw new ArgumentNullException(nameof(jobId));
            }

            _logger.Info($"{DateTime.Now} - Enqueue job {jobId} to queue {_queueClient.Uri}");
            _queueClient.SendMessage(jobId);
        }
    }
}