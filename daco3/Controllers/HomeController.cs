using daco3.Helpers;
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
        private Databaza db { get; set; }

        public HomeController() {
            db = new Databaza();
        }
        [Authorize]
        public ActionResult Index()
        {
            var tralala = User.Identity.Name;
            var b = User as MojPrincipal;
            return View();
        }
        [Authorize]
        public ActionResult Zaznami()
        {
            Tabulka top20 = new Tabulka();
            top20.tableZaznami = db.Zaznami.Where(z => z.Uzivatel.UzivatelId == db.Uzivatelia.Where(u => u.UzivatelId == z.UzivatelId).FirstOrDefault().UzivatelId).Take(20).ToList();
            foreach (var item in top20.tableZaznami)
            {
                var meno = db.Uzivatelia.Where(z => z.UzivatelId == item.UzivatelId).FirstOrDefault().Username;
                item.Uzivatel = new Uzivatel() { Username = meno} ;
            }
            return View(top20);

        }
    }
}