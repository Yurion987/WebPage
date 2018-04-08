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
using Newtonsoft.Json;
using System.Threading;
using System.Globalization;
using System.Web.Security;

namespace daco3.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private Databaza db { get; set; }

        public HomeController()
        {
            db = new Databaza();
        }
       
        public ActionResult Index()
        {
            var tralala = User.Identity.Name;
            var b = User as MojPrincipal;
            return View();
        }
       
        public ActionResult Zaznami(int? page, string sort = "Cas", string sortdir = "desc", string userName = "", string zaznamCasOd = "", string zaznamCasDo = "" )
        {
            if (sort.Equals("Datum")) sort = "Cas";
            if (sort.Equals("Meno")) sort = "Uzivatel.Username";

            var list = db.Zaznami.OrderBy($"{sort} {sortdir}")
             .Select(c => new TabulkaZaznami { Cas = c.Cas, UserName = c.Uzivatel.Username, ZaznamId = c.ZaznamId, UzivatelId = c.UzivatelId }).ToList();
            var reduced = new List<TabulkaZaznami>();

            if (userName != "") {
                list = list = list.Where(x => x.UserName == userName).ToList();
            }
            if (zaznamCasOd != "") {
                DateTime Od = DateTime.Parse(zaznamCasOd);
                list = list.Where(x => x.Cas.Date >= Od.Date).ToList();
            }
            if (zaznamCasDo != "") {
                DateTime Do = DateTime.Parse(zaznamCasDo);
                list = list.Where(x =>x.Cas.Date <= Do.Date).ToList();
            }

            list.ForEach(z => {
                if (!reduced.Any(r => r.UserName == z.UserName && r.Cas.Date == z.Cas.Date))
                    reduced.Add(z);

            });

            List<GridTabulka> gt = new List<GridTabulka>();
            foreach (var item in reduced)
            {

                var denZam = db.Zaznami.Where(z => z.UzivatelId == item.UzivatelId).ToList().Where(x => x.Cas.ToString("dd.MM.yyyy").Equals(item.Cas.ToString("dd.MM.yyyy"))).ToList();
                var sumaCasu = odpracovanyCas(denZam);
                gt.Add(new GridTabulka(item.Cas, item.UserName, item.ZaznamId, sumaCasu));
            }
            var pagedList = gt.ToPagedList(page ?? 1, 15);
            ViewBag.TotalRow = pagedList.Count;
            return View(pagedList);
        }
        public ActionResult ZaznamPodrobne(int? id)
        {

            var a = id ?? db.Zaznami.Where(x => x.ZaznamId != 0).FirstOrDefault().ZaznamId;
            var zaznam = db.Zaznami.Where(x => x.ZaznamId == id).FirstOrDefault();
            var vsetkyZaznamiDna = db.Zaznami.Where(x => x.UzivatelId == zaznam.UzivatelId).ToList().Where(x => x.Cas.ToString("dd.MM.yyyy").Equals(zaznam.Cas.ToString("dd.MM.yyyy"))).ToList();
            List<GridTabulka> gt = new List<GridTabulka>();
            foreach (var item in vsetkyZaznamiDna)
            {
                gt.Add(new GridTabulka(item.Cas, item.Uzivatel.Username, item.Typ, item.ZaznamId));
            }
            ViewBag.TotalRowSingleDay = gt.Count;
            return View(gt);
        }

        private string odpracovanyCas(List<Zaznam> den)
        {
            var odchod = den.Where(x => x.Typ == "O").OrderBy(z => z.Cas).ToList();
            var prichod = den.Where(x => x.Typ == "P").OrderBy(z => z.Cas).ToList();
            double pocetMinut = 0;
            if (odchod.Count == prichod.Count)
            {
                for (int i = 0; i < odchod.Count; i++)
                {
                    var odpracovaneVMin = (odchod[i].Cas - prichod[i].Cas).TotalMinutes;
                    pocetMinut = pocetMinut + odpracovaneVMin;
                }
                TimeSpan celkovyCas = TimeSpan.FromMinutes(pocetMinut);
                return celkovyCas.ToString(@"hh\:mm");
            }
            else
            {
                if (prichod[0].Cas.ToString("dd.MM.yyyy").Equals(DateTime.Now.ToString("dd.MM.yyyy")) && DateTime.Now.Hour < 17) return "Je v Praci";
                return "0";

            }

        }

        [HttpPost]
        public ActionResult save(long id, string propertyName, string value) {
            var status = false;
            var message = "";
            if (value == "O" || value == "P")
            {

                var zaznam = db.Zaznami.Where(x => x.ZaznamId == id).FirstOrDefault();
                if (zaznam != null)
                {

                    var idPrihlaseneho = db.Uzivatelia.Where(x => x.Username == User.Identity.Name).FirstOrDefault().UzivatelId;
                    db.Logy.Add(new Log() { UzivatelId = idPrihlaseneho, ZmenaTypu = true, ZaznamId = id });
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
        [HttpPost]
        public ActionResult NovyZaznam(DateTime zaznamCas, string zaznamTyp, long zaznamId) {
            var chybovaHlaska = "";
            var uspech = false;
            if (zaznamTyp.Equals("") || zaznamCas.ToString("HH:mm").Equals("00:00"))
            {
                chybovaHlaska = "Zle zadane Udaje";
            }
            else {
                var copy = db.Zaznami.Find(zaznamId);
                Thread.CurrentThread.CurrentCulture = new CultureInfo("sk-SK");
                var casZaznamu = copy.Cas.ToString("dd.MM.yyyy");
                var pridanyCas = zaznamCas.ToString("HH:mm");
                var cas = DateTime.Parse(casZaznamu + " " + pridanyCas, CultureInfo.CurrentCulture);
                db.Zaznami.Add(new Zaznam { Cas = cas, Typ = zaznamTyp, UzivatelId = copy.UzivatelId });
                db.SaveChanges();
                uspech = true;
            }

            if (uspech)
            {
                return Redirect(Request.UrlReferrer.PathAndQuery);
            }
            else {
                return new JsonResult()
                {
                    Data = new { success = uspech, nameError = chybovaHlaska },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

        }

        public ActionResult Logout() {

            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }

        public ActionResult ZaznamOpravy(int? page, string sort = "Datum", string sortdir = "desc", string userName="") {
            if (sort.Equals("Datum") || sort.Equals("Odpracovane")) sort = "Cas";
            if (sort.Equals("Meno")) sort = "Uzivatel.Username";
            List<TabulkaZaznami> list;
            List<TabulkaZaznami> reduced;
            if (userName == "")
            {
                list = db.Zaznami.OrderBy($"{sort} {sortdir}")
                .Select(c => new TabulkaZaznami { Cas = c.Cas, UserName = c.Uzivatel.Username, ZaznamId = c.ZaznamId, UzivatelId = c.UzivatelId }).ToList();
                reduced = new List<TabulkaZaznami>();
            }
            else {
                list = db.Zaznami.OrderBy($"{sort} {sortdir}").Where(x=> x.Uzivatel.Username==userName)
               .Select(c => new TabulkaZaznami { Cas = c.Cas, UserName = c.Uzivatel.Username, ZaznamId = c.ZaznamId, UzivatelId = c.UzivatelId }).ToList();
                reduced = new List<TabulkaZaznami>();
            }
            

            list.ForEach(z => {
                if (!reduced.Any(r => r.UserName == z.UserName && r.Cas.Date == z.Cas.Date))
                    reduced.Add(z);

            });

            List<GridTabulka> gt = new List<GridTabulka>();
            foreach (var item in reduced)
            {

                var denZam = db.Zaznami.Where(z => z.UzivatelId == item.UzivatelId).ToList().Where(x => x.Cas.ToString("dd.MM.yyyy").Equals(item.Cas.ToString("dd.MM.yyyy"))).ToList();
                var sumaCasu = odpracovanyCas(denZam);
                if (sumaCasu == "0") {
                    gt.Add(new GridTabulka(item.Cas, item.UserName, item.ZaznamId, sumaCasu));
                }
            }
            var pagedList = gt.ToPagedList(page ?? 1, 15);
            ViewBag.TotalRow = pagedList.Count;
            return View(pagedList);

        }
        public ActionResult CompleteName(string term) {
            return Json(db.Uzivatelia.Where(c => c.Username.StartsWith(term)).Select(a => new { label = a.Username }), JsonRequestBehavior.AllowGet);

        }

        public ActionResult Settings() {



            return View();
        }

        [HttpPost]
        public ActionResult ZmenaHesla(string stareHeslo, string noveHeslo, string noveHesloKontrola) {
            var prihlaseny = User as MojPrincipal;
            if (noveHeslo.Length >4 || noveHesloKontrola.Length >4) {
                var hashStareHeslo = Hash.ZaHashuj(stareHeslo);
                if (noveHeslo != noveHesloKontrola) {
                    return new JsonResult()
                    {
                        Data = new { nameError = "Nové heslá sa nezhodujú" },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                } else if (hashStareHeslo != prihlaseny.Uzivatel.Heslo) {
                    return new JsonResult()
                    {
                        Data = new { nameError = "Staré heslá sa nezhodujú" },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                var hashNoveHeslo = Hash.ZaHashuj(noveHeslo);
                var uzivatel = db.Uzivatelia.Find(prihlaseny.Uzivatel.UzivatelId);
                db.Entry(uzivatel).Property("Heslo").CurrentValue = hashNoveHeslo;
                db.SaveChanges();
                return new JsonResult()
                {
                    Data = new { nameError = "Heslo úspešne zmenené" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            return new JsonResult()
            {
                Data = new { nameError = "Príliš krátke nové heslo (aspon 5 znakov)" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }


    public class TabulkaZaznami
    {
        public DateTime Cas { get; set; }
        public string UserName { get; set; }
        public long ZaznamId { get; set; }
        public int UzivatelId { get; set; }
    }
}