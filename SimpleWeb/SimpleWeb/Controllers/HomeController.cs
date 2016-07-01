using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "You are able to view this page after signing in!";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Connect with Aaron!";

            return View();
        }
    }
}