using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using frontoffice.Models;

namespace frontoffice.Controllers
{
    public class HomeController : Controller
    {

        // Index action is default action for Home controller
        // It returns the result on Http Get and allows non authenticated users to execute it
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
                // if user is authenticated returns Index view from Views/Home/Index
                return View();
            else
                // if not redirects user to login View
                return RedirectToAction("Login", "Users");
        }


        // Analytics action executes only if user is authenticated. It then returns Analytics View from Viws/Home/Analytics
        [HttpGet]
        [Authorize]
        public ActionResult Analytics()
        {
            return View();
        }

        // Display DB view
        [HttpGet]
        [Authorize]
        public ActionResult DB()
        {
            return View(Helpers.DBReportHelper.GetAllReports());
        }


        [HttpGet]
        [Authorize]
        public ActionResult AddDBEntry()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddDBEntry(AddDBEntryViewModel Model)
        {
            if (!ModelState.IsValid)
                return View();

            switch (Helpers.DBReportHelper.UploadReport(Model.File, 
                    Model.Title, 
                    Model.Description))
            {
                case "no_xlsx": ModelState.AddModelError("", "File extension should be .xlsx"); return View();
                case "err_01": ModelState.AddModelError("", "Error while writing to DB."); return View();
                case "err_02": ModelState.AddModelError("", "Error while writing file."); return View();
                case "err_03": ModelState.AddModelError("", "Please pick a file."); return View();
                default: return RedirectToAction("DB", "Home");
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult SetDefaultReport(SetDefaultDBReportHelper Model)
        {
            object result = Helpers.DBReportHelper.SetDefaultDBEntry(Model.Report_id);
            if (result == null ||
                string.IsNullOrEmpty(result.ToString()))
            {
                if (Model.RedirectToDashboard)
                    return RedirectToAction("Index", "Home");
                else
                    return RedirectToAction("DB", "Home");
            }
            else
                return View("DisplayError", result);
        }
    }
}
