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
        public ActionResult Zaznami(int? page ,string sort="Datum", string sortdir="desc")
        {
            
            var v = (from c in db.Zaznami
                     where
                            c.ZaznamId != 0
                     select new {c.Cas,c.Uzivatel.Username,c.ZaznamId});

            
            if (sort.Equals("Datum")) sort = "Cas";
            if (sort.Equals("Meno")) sort = "Username";
            v = v.OrderBy(sort + " " + sortdir);
            var list = v.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                var meno = list[i].Username;
                var cas = list[i].Cas.ToString("dd.MM.yyyy");

                for (int j = i+1; j < list.Count; j++)
                {
                    if (list[j].Username == meno && cas == list[j].Cas.ToString("dd.MM.yyyy")) {
                        list.RemoveAt(j);
                        j--;
                    }
                }
            }

            ;
            List<GridTabulka> gt = new List<GridTabulka>();
            foreach (var item in list)
            {
                gt.Add(new GridTabulka(item.Cas, item.Username,item.ZaznamId));
            }
             var pagedList = gt.ToPagedList(page ?? 1,10);
             ViewBag.TotalRow = pagedList.Count;
             return View(pagedList);
        }
        public ActionResult ZaznamPodrobne(int id) {

            return View();
        }
        
    }
}