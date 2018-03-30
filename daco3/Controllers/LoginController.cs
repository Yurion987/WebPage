using daco3.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace daco3.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            //zavolam db
            //nieco selectnem
            //a vyrobim model
            var a = new DatabazaModel();
            ViewBag.Err = "";
            return View(a);
        }
        
        public ActionResult Submit(DatabazaModel model)
        {
            //zavolam DB ci som validny
          
            var a = ConfigurationManager.AppSettings["dbCon"];
            
            if (model.Prihlas())
            {
                return RedirectToAction("Index", "Home",model);
            }

            ViewBag.Err = "Zle zadane udaje";
            return View("Index",model);
        }

    }
 }