using daco3.Helpers;
using daco3.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace daco3.Controllers
{
    public class HomeController : Controller
    {
        private Databaza db { get; set; }

        public HomeController()
        {
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
        public ActionResult Zaznami(int? page, string sort = "Datum", string sortdir = "desc")
        {


            var v = (from c in db.Zaznami
                     where
                            c.ZaznamId != 0
                     select new { c.Cas, c.Uzivatel.Username, c.ZaznamId, c.UzivatelId });


            if (sort.Equals("Datum") || sort.Equals("Odpracovane")) sort = "Cas";
            if (sort.Equals("Meno")) sort = "Username";
            v = v.OrderBy(sort + " " + sortdir);
            var list = v.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                var meno = list[i].Username;
                var cas = list[i].Cas.ToString("dd.MM.yyyy");

                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[j].Username == meno && cas == list[j].Cas.ToString("dd.MM.yyyy"))
                    {
                        list.RemoveAt(j);
                        j--;
                    }
                }
            }
            List<GridTabulka> gt = new List<GridTabulka>();
            foreach (var item in list)
            {

                var denZam = db.Zaznami.Where(z => z.UzivatelId == item.UzivatelId).ToList().Where(x => x.Cas.ToString("dd.MM.yyyy").Equals(item.Cas.ToString("dd.MM.yyyy"))).ToList();
                var sumaCasu = odpracovanyCas(denZam);
                gt.Add(new GridTabulka(item.Cas, item.Username, item.ZaznamId, sumaCasu));
            }
            var pagedList = gt.ToPagedList(page ?? 1, 15);
            ViewBag.TotalRow = pagedList.Count;
            return View(pagedList);
        }
        public ActionResult ZaznamPodrobne(int? id)
        {
            var a = id ?? db.Zaznami.Where(x=> x.ZaznamId !=0).FirstOrDefault().ZaznamId;
            var zaznam = db.Zaznami.Where(x => x.ZaznamId == id).FirstOrDefault();
            var vsetkyZaznamiDna = db.Zaznami.Where(x=> x.UzivatelId == zaznam.UzivatelId).ToList().Where(x=> x.Cas.ToString("dd.MM.yyyy").Equals(zaznam.Cas.ToString("dd.MM.yyyy"))).ToList();
            List<GridTabulka> gt = new List<GridTabulka>();
            foreach (var item in vsetkyZaznamiDna)
            {
                gt.Add(new GridTabulka(item.Cas,item.Uzivatel.Username,item.Typ,item.ZaznamId));
            }
            ViewBag.TotalRowSingleDay = gt.Count;
            return View(gt);
        }

        private string odpracovanyCas(List<Zaznam> den)
        {
            var odchod = den.Where(x => x.Typ == "O").OrderBy(z=>z.Cas).ToList();
            var prichod = den.Where(x => x.Typ == "P").OrderBy(z => z.Cas).ToList();
            double pocetMinut = 0;
            if (odchod.Count == prichod.Count)
            {
                for (int i = 0; i < odchod.Count; i++)
                {
                    var odpracovaneVMin = (odchod[i].Cas -prichod[i].Cas).TotalMinutes;
                    pocetMinut = pocetMinut + odpracovaneVMin;
                }
                TimeSpan celkovyCas = TimeSpan.FromMinutes(pocetMinut);
                return celkovyCas.ToString(@"hh\:mm");
            }
            else
            {
                if (prichod[0].Cas.ToString("dd.MM.yyyy").Equals(DateTime.Now.ToString("dd.MM.yyyy")) && DateTime.Now.Hour <17) return "Je v Praci"; 
                return "0";

            }

        }

        [HttpPost]
        public ActionResult save(long id,string propertyName,string value) {
                var status = false;
                var message = "";
            if (value == "O" || value == "P")
            {

                var zaznam = db.Zaznami.Where(x => x.ZaznamId == id).FirstOrDefault();
                if (zaznam != null)
                {
                    //todo napln aj zaznam v tabulke uprava ze prebehla zmena
                    db.Entry(zaznam).Property(propertyName).CurrentValue = value;
                    db.SaveChanges();
                    status = true;
                }
                else
                {
                    message = "Nenajdeny zaznam";
                }

            }
            else {
                message = "Zle zadana hodnota (len P alebo O)";
            }

            //odpoved
            var response = new { value = value, status = status, message = message };

            JObject o = JObject.FromObject(response);
            return Content(o.ToString());

        }
    }
}