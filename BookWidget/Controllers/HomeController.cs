using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BookWidget.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index() {
            return RedirectToRoute(new { controller = "Book", action = "index" }); 
        }
    }
}
