﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Hangfire;
using Hangfire.Azure.ServiceBusQueue;
using Hangfire.SqlServer;

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
            var option = new ServiceBusQueueOptions()
            {
                QueuePollInterval = TimeSpan.FromSeconds(60),
                ConnectionString = queueConnString,
                Queues = new[] { "default" }
            };
            var sqlServerStorageOption = new SqlServerStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(60),
            };
            Hangfire.GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(dbConnString, sqlServerStorageOption)
                //.UseServiceBusQueues(option)
                .UseMsmqQueues(queueConnString);


            BackgroundJob.Enqueue(() => Console.WriteLine("Message sent from client - Hello server!"));
        }
    }
}
