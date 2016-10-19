using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Diagnostics.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            GetValue();
            return View();
        }

        private static void GetValue()
        {
            GetValue1();
        }

        private static void GetValue1()
        {
            try
            {
                throw new IOException("aaaa");
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Exception", ex);
            }
        }
    }
}
