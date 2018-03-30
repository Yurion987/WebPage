using daco3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace daco3.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index(DatabazaModel model)
        {
            
            return View(model);
        }
        public ActionResult Zaznami(DatabazaModel model) {

            model.selectData();
            

            return View(model);

        }
    }
}