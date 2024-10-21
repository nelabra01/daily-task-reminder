using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Azure.Identity;
using Azure.Storage.Queue.Manager;
using Azure.Storage.Queues;
using Hangfire;
using Hangfire.Azure.ServiceBusQueue;
using Hangfire.SqlServer;
using Microsoft.Extensions.Azure;

namespace DailyTaskRemider.API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ConfigureHangfire();
        }

        private void ConfigureHangfire()
        {
            var dbConnString = ConfigurationManager.ConnectionStrings["default"]?.ToString();
            var queueConnString = ConfigurationManager.AppSettings["hangfireQueue"].ToString();
            var queueUri = new Uri($"https://neblobstoragetestaccount.queue.core.windows.net/default");

            var config = Hangfire.GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(dbConnString)
#if DEBUG
                //.Entry.AddQueueServiceClient(new QueueServiceClient(queueUri))
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
            BackgroundJob.Enqueue(() => Console.WriteLine("Message sent from client using hangfire"));
        }
    }
}
