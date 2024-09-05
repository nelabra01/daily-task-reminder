using System;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Azure.ServiceBusQueue;
using System.Messaging;
using System.Threading;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

namespace DailyTaskReminder.Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
        }


    }
}
