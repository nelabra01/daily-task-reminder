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
            var ServiceBusQueueOptions = new ServiceBusQueueOptions()
            {
                QueuePollInterval = TimeSpan.FromSeconds(60),
                ConnectionString = connString,
                Queues = new[] { "default" }
            };
            var sqlServerStorageOption = new SqlServerStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(60),
            };
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(connString)
                //.UseServiceBusQueues(option);
                .UseMsmqQueues(@"FormatName:Direct=OS:icc12076\private$\default");
            //.UseMsmqQueues("private$\\hanfire-queue", new[] {"hanfire-queue"} );

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
