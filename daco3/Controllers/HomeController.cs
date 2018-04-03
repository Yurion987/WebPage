using daco3.Helpers;
using daco3.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;

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
        public ActionResult Zaznami(int? page ,string sort="ZaznamId", string sortdir="desc")
        {

            var v = (from a in db.Zaznami
                     where
                            a.ZaznamId != 0
                     select a);

            // todo sprav sortovanie podla mena a nie ID ked na to klikne 
            if (sort.Equals("Datum")) sort = "Cas";
            if (sort.Equals("Meno")) sort = "UzivatelId";
            v = v.OrderBy(sort + " " + sortdir);

            
            List<GridTabulka> gt = new List<GridTabulka>();
            foreach (var item in v)
            {
                var meno = db.Uzivatelia.Where(z => z.UzivatelId == item.UzivatelId).FirstOrDefault().Username;
                gt.Add(new GridTabulka(item.Cas, item.Typ, meno));
            }
             var pagedList = gt.ToPagedList(page ?? 1,15);
             ViewBag.TotalRow = pagedList.Count;
             return View(pagedList);

        }
        
    }
}