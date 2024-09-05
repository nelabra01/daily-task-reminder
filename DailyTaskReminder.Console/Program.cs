using System;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Azure.ServiceBusQueue;
using System.Messaging;
using System.Threading;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using System.Configuration;

namespace DailyTaskReminder.Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            var connString = ConfigurationManager.ConnectionStrings["hangfire"]?.ToString();
            var queueConnString = ConfigurationManager.AppSettings["hangfireQueue"].ToString();

            GlobalConfiguration.Configuration
                .UseSqlServerStorage(connString)
#if DEBUG
                .UseMsmqQueues(queueConnString);
#else
                .UseServiceBusQueues(new ServiceBusQueueOptions()
                     {
                         QueuePollInterval = TimeSpan.FromSeconds(60),
                         ConnectionString = queueConnString,
                         Queues = new[] { "default" }
                     });
#endif

            var options = new BackgroundJobServerOptions
            {
                SchedulePollingInterval = TimeSpan.FromSeconds(60),
            };

            using (var server = new BackgroundJobServer())
            {
                Console.WriteLine("Hangfire Server Started. Press any key to exit...");
                Console.ReadLine();
            }
        }
    }
}
