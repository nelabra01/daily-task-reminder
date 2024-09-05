using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DailyTaskRemider.API.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEnumerable<string> _tasks = new string[]
        {
            "Pick up kids from school.",
            "Take out the trash",
            "Check the mail",
            "Water the plants",
            "Clean the basement"
        };

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View(_tasks);
        }

        public RedirectToRouteResult SendReminder(string message)
        {
            // BackgroundJob.Schedule(() => Console.WriteLine(message), TimeSpan.FromSeconds(5));
            for(int i = 0; i < 100; i++)
            {
                BackgroundJob.Enqueue(() => Console.WriteLine("New message on its way:" + i));
            }
            
            BackgroundJob.Schedule(() => Console.WriteLine(message), TimeSpan.FromSeconds(5));
            return RedirectToAction("Index");
        }
    }
}
