using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp1.Data;
using WebApp1.Models;
using WebApp1.Services;

namespace WebApp1.Controllers
{
    public class HomeController : Controller
    {
        [SessionAuthorize]
        public ActionResult Index()
        {
            return View(Models.User.GetUserByEmail(HttpContext.Session["Email"].ToString()));
        }

        [SessionAuthorize]
        public ActionResult ManagerReport()
        {
            return View(Models.User.GetUserByEmail(HttpContext.Session["Email"].ToString()));
        }

        [SessionAuthorize]
        public ActionResult Users()
        {
            return View(Models.User.GetUserByEmail(HttpContext.Session["Email"].ToString()));
        }

        [SessionAuthorize]
        public ActionResult AdminReport()
        {
            return View(Models.User.GetUserByEmail(HttpContext.Session["Email"].ToString()));
        }
        
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}