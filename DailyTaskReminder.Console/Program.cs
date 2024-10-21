using System;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Azure.ServiceBusQueue;
using System.Messaging;
using System.Threading;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using System.Configuration;
using Azure.Storage.Queue.Manager;
using Azure.Storage.Queues;
using Microsoft.Extensions.Azure;

namespace DailyTaskReminder.Server
{
    public class Program
    {
        static void Main(string[] args)
        {

            try
            {
                var connString = ConfigurationManager.ConnectionStrings["hangfire"]?.ToString();
                var queueConnString = ConfigurationManager.AppSettings["hangfireQueue"].ToString();
                var queueUri = new Uri($"https://neblobstoragetestaccount.queue.core.windows.net/default");

                System.Console.WriteLine(connString);
                System.Console.WriteLine(queueConnString);

                GlobalConfiguration.Configuration
                    .UseSqlServerStorage(connString)
#if DEBUG
                    .UseAzureStorageQueue(queueUri)
                    //.UseMsmqQueues(queueConnString)
                    ;
#else
                    .UseServiceBusQueues(new ServiceBusQueueOptions()
                    {
                        ConnectionString = queueConnString,
                        Queues = new[] { "default" }
                    });
#endif

                using (var server = new BackgroundJobServer())
                {
                    System.Console.WriteLine("Hangfire Server Started. Press any key to exit...");
                    System.Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }

        }
    }
}
