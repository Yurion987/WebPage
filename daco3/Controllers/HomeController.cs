﻿using daco3.Helpers;
using daco3.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using PagedList;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Web.Security;
using OfficeOpenXml;
using System.Windows.Forms;
using System.IO;
using System.Web;

namespace daco3.Controllers
{
    [RequireHttps]
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
            var prihlaseny = User as MojPrincipal;
            var celaDoch = db.Zaznami.Where(x => x.UzivatelId == prihlaseny.Uzivatel.UzivatelId).OrderByDescending(x=> x.Cas).ToList();
            List<GridTabulka> tab = new List<GridTabulka>();
            foreach (var item in celaDoch)
            {
                tab.Add(new GridTabulka(item.Cas, item.Uzivatel.Username, item.Typ, item.ZaznamId));
            }

            return View(tab);
        }
        [Authorize(Roles = "Admin,Uctovnicka")]
        public ActionResult Zaznamy(int? page, string sort = "Cas", string sortdir = "desc", string userName = "", string zaznamCasOd = "", string zaznamCasDo = "")
        {
          
            if (sort.Equals("Datum")) sort = "Cas";
            if (sort.Equals("Meno")) sort = "Uzivatel.Username";
            var vsetkyZaznami = db.Zaznami.OrderBy($"{sort} {sortdir}")
             .Select(c => new TabulkaZaznami { Cas = c.Cas, UserName = c.Uzivatel.Username, ZaznamId = c.ZaznamId, UzivatelId = c.UzivatelId }).ToList();
            var dennaDochadzka = new List<TabulkaZaznami>();

            if (userName != "")
            {
                vsetkyZaznami = vsetkyZaznami.Where(x => x.UserName.ToLower().Contains(userName.ToLower())).ToList();
            }
            if (zaznamCasOd != "")
            {
                DateTime Od = DateTime.Parse(zaznamCasOd);
                vsetkyZaznami = vsetkyZaznami.Where(x => x.Cas.Date >= Od.Date).ToList();
            }
            if (zaznamCasDo != "")
            {
                DateTime Do = DateTime.Parse(zaznamCasDo);
                vsetkyZaznami = vsetkyZaznami.Where(x => x.Cas.Date <= Do.Date).ToList();
            }

            vsetkyZaznami.ForEach(z =>
            {
                if (!dennaDochadzka.Any(r => r.UserName == z.UserName && r.Cas.Date == z.Cas.Date))
                    dennaDochadzka.Add(z);

            });

            List<GridTabulka> gt = new List<GridTabulka>();
            foreach (var item in dennaDochadzka)
            {
                var denZam = db.Zaznami.Where(z => z.UzivatelId == item.UzivatelId).ToList().Where(x => x.Cas.ToString("dd.MM.yyyy").Equals(item.Cas.ToString("dd.MM.yyyy"))).ToList();
                var sumaCasu = OdpracovanyCas(denZam);
                var dochString = FormatDoch(denZam);
                gt.Add(new GridTabulka(item.Cas, item.UserName, item.ZaznamId, sumaCasu, dochString));
            }

            return View(gt);
        }
        private string OdpracovanyCas(List<Zaznam> den)
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
        private string FormatDoch(List<Zaznam> den)
        {
            string format = "";
            foreach (var item in den)
            {
                format = format + item.Cas.ToString("HH:mm") + "-";
                if (item.Typ == "O")
                {
                    format = format.Remove(format.Length - 1);
                    format = format + " | ";
                }
            }

            if (den.Last().Typ == "O")
                format = format.Remove(format.Length - 2);
            return format;

        }
        [Authorize(Roles = "Admin,Uctovnicka")]
        public ActionResult ZaznamPodrobne(int? id)
        {
            var zaznam = db.Zaznami.Where(x => x.ZaznamId == id).FirstOrDefault();
            if (zaznam != null)
            {
                var vsetkyZaznamiDna = db.Zaznami.Where(x => x.UzivatelId == zaznam.UzivatelId).ToList().Where(x => x.Cas.ToString("dd.MM.yyyy").Equals(zaznam.Cas.ToString("dd.MM.yyyy"))).OrderBy(x=>x.Cas).ToList();
                List<GridTabulka> gt = new List<GridTabulka>();
                foreach (var item in vsetkyZaznamiDna)
                {
                    gt.Add(new GridTabulka(item.Cas, item.Uzivatel.Username, item.Typ, item.ZaznamId, item.ZaznamIdWeb));
                }
                ViewBag.TotalRowSingleDay = gt.Count;
                return View(gt);
            } 
            return RedirectToAction("ZaznamOpravy", "Home");
        }
        [HttpPost]
        public ActionResult Save(long id, string propertyName, string value)
        {
            var status = false;
            var message = "";
            string converVal = "";
            if (propertyName == "Typ")
            {
                converVal = value == "P" ? "Prichod" : "Odchod";
                if (value == "O" || value == "P")
                {

                    var zaznam = db.Zaznami.Where(x => x.ZaznamId == id).FirstOrDefault();
                    if (zaznam != null)
                    {
                        var prihlaseny = User as MojPrincipal;
                        db.Logy.Add(new Log() { UzivatelId = prihlaseny.Uzivatel.UzivatelId, ZmenaTypu = true, ZaznamId = id });
                        db.Entry(zaznam).Property(propertyName).CurrentValue = value;
                        db.SaveChanges();

                        status = true;
                    }
                    else
                    {
                        message = "Nenajdeny zaznam";
                    }

                }
                else
                {
                    message = "Nezadaný typ záznamu(Odchod,Príchod)";
                }

            }
            else if(propertyName=="Cas")
            {
               DateTime t ;
                if (DateTime.TryParse(value,out t) && t.Hour >= 6 && t.Hour <= 17)
                {
                    var zaznam = db.Zaznami.Where(x => x.ZaznamId == id).FirstOrDefault();
                    if (zaznam != null)
                    {
                        var prihlaseny = User as MojPrincipal;
                        db.Logy.Add(new Log() { UzivatelId = prihlaseny.Uzivatel.UzivatelId, ZmenaTypu = false, ZaznamId = id,StaraHodnota=zaznam.Cas});
                        string novyDatum = zaznam.Cas.ToString("dd.MM.yyyy")+" "+t.ToString("HH:mm");
                        DateTime updateCas = DateTime.Parse(novyDatum, CultureInfo.CurrentCulture);
                        converVal = t.ToString("HH:mm");
                        db.Entry(zaznam).Property(propertyName).CurrentValue = updateCas;
                        db.SaveChanges();
                        status = true;
                    }
                    else
                    {
                        message = "Nenajdeny zaznam";
                    }
                }
                else {
                    message = "Zle zadana hodnota (Presny format Hodiny:minuty napr. 07:54)";
                }
             
            }
          
            

            var response = new { value = converVal, status = status, message = message };

            JObject o = JObject.FromObject(response);
            return Content(o.ToString());

        }
        [HttpPost]
        public ActionResult NovyZaznam(DateTime zaznamCas, string zaznamTyp, long zaznamId)
        {
            var chybovaHlaska = "";
            var uspech = false;
            if (zaznamTyp.Equals("") || zaznamCas.ToString("HH:mm").Equals("00:00"))
            {
                chybovaHlaska = "Zle zadane Udaje";
            }
            else
            {
                var copy = db.Zaznami.Find(zaznamId);
                var casZaznamu = copy.Cas.ToString("dd.MM.yyyy");
                var pridanyCas = zaznamCas.ToString("HH:mm");
                var cas = DateTime.Parse(casZaznamu + " " + pridanyCas, CultureInfo.CurrentCulture);
                db.Zaznami.Add(new Zaznam { Cas = cas, Typ = zaznamTyp, UzivatelId = copy.UzivatelId });
                db.SaveChanges();
                uspech = true;
            }

            if (uspech)
            {
                return new JsonResult()
                {
                    Data = new { success = uspech, nameError = "OK" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                return new JsonResult()
                {
                    Data = new { success = uspech, nameError = chybovaHlaska },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }
        [Authorize(Roles = "Admin,Uctovnicka")]
        public ActionResult ZaznamOpravy(int? page, string sort = "Datum", string sortdir = "desc", string userName = "")
        {
            if (sort.Equals("Datum") || sort.Equals("Odpracovane")) sort = "Cas";
            if (sort.Equals("Meno")) sort = "Uzivatel.Username";
            List<TabulkaZaznami> vsetkyZaznami;
            List<TabulkaZaznami> dnoveZaznami = new List<TabulkaZaznami>();
            if (userName == "")
            {
                vsetkyZaznami = db.Zaznami.OrderBy($"{sort} {sortdir}")
                .Select(c => new TabulkaZaznami { Cas = c.Cas, UserName = c.Uzivatel.Username, ZaznamId = c.ZaznamId, UzivatelId = c.UzivatelId }).ToList();
               
            }
            else
            {
                vsetkyZaznami = db.Zaznami.OrderBy($"{sort} {sortdir}").Where(x => x.Uzivatel.Username.ToLower().Contains(userName.ToLower()))
               .Select(c => new TabulkaZaznami { Cas = c.Cas, UserName = c.Uzivatel.Username, ZaznamId = c.ZaznamId, UzivatelId = c.UzivatelId }).ToList();
                
            }
            vsetkyZaznami.ForEach(z =>
            {
                if (!dnoveZaznami.Any(r => r.UserName == z.UserName && r.Cas.Date == z.Cas.Date))
                    dnoveZaznami.Add(z);

            });

            List<GridTabulka> gt = new List<GridTabulka>();
            foreach (var item in dnoveZaznami)
            {

                var denZam = db.Zaznami.Where(z => z.UzivatelId == item.UzivatelId).ToList().Where(x => x.Cas.ToString("dd.MM.yyyy").Equals(item.Cas.ToString("dd.MM.yyyy"))).ToList();
                var sumaCasu = OdpracovanyCas(denZam);
                if (sumaCasu == "0" || sumaCasu == "Je v Praci")
                {
                    gt.Add(new GridTabulka(item.Cas, item.UserName, item.ZaznamId, sumaCasu));
                }
            }
            
            return View(gt);

        }
        public ActionResult CompleteName(string term)
        {
            return Json(db.Uzivatelia.Where(c => c.Username.StartsWith(term)).Select(a => new { label = a.Username }), JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult ZmenaHesla(string stareHeslo, string noveHeslo, string noveHesloKontrola)
        {
            var prihlaseny = User as MojPrincipal;
            if (noveHeslo.Length > 4 || noveHesloKontrola.Length > 4)
            {
                var hashStareHeslo = Hash.ZaHashuj(stareHeslo);
                if (noveHeslo != noveHesloKontrola)
                {
                    return new JsonResult()
                    {
                        Data = new { nameError = "Nové heslá sa nezhodujú" },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else if (hashStareHeslo != prihlaseny.Uzivatel.Heslo)
                {
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
        [Authorize(Roles = "Admin,Uctovnicka")]
        public ActionResult DochadzkaMesiac(int? page, string menoZamestnanca = "", string mesiac = "", string rokZaznamu = "")
        {
            if (rokZaznamu == "")
            {
                rokZaznamu = DateTime.Now.Year.ToString();
            }
            if (mesiac == "")
            {
                mesiac = DateTime.Now.ToString("MMMM");
            }
            var cisloMesiaca = DateTime.ParseExact(mesiac, "MMMM", CultureInfo.CurrentCulture).Month;
            var cisloRoku = DateTime.ParseExact(rokZaznamu, "yyyy", CultureInfo.CurrentCulture).Year;
            List<List<Zaznam>> listMesiacDoch;
            if (menoZamestnanca != "")
            {
                listMesiacDoch = db.Zaznami.Where(x => x.Uzivatel.Username.ToLower().Contains(menoZamestnanca.ToLower()))
                    .GroupBy(x => x.Uzivatel.Username)
                    .Select(x => x.ToList())
                    .ToList();
            }
            else
            {
                listMesiacDoch = db.Zaznami.Where(x => x.Cas.Month == cisloMesiaca && x.Cas.Year == cisloRoku)
                    .GroupBy(x => x.Uzivatel.Username)
                    .Select(x => x.ToList())
                    .ToList();
            }

            List<GridTabulka> gt = new List<GridTabulka>();
            foreach (var item in listMesiacDoch)
            {
                gt.Add(new GridTabulka(item[0].Uzivatel.Username, mesiac, CelkovaDochadzkaZaMesiac(item)));
            }
            return View(gt);

        }
        public string CelkovaDochadzkaZaMesiac(List<Zaznam> mesacnaDoch)
        {
            var prichody = mesacnaDoch.Where(x => x.Typ == "P").ToList();
            var odchody = mesacnaDoch.Where(x => x.Typ == "O").ToList();
            if (prichody.Count == odchody.Count)
            {
                double celkoveMinuty = 0;
                for (int i = 0; i < prichody.Count; i++)
                {
                    var odpracovaneVMin = (odchody[i].Cas - prichody[i].Cas).TotalMinutes;
                    celkoveMinuty = celkoveMinuty + odpracovaneVMin;
                }
                double pocetMin = Math.Floor(celkoveMinuty % 60);
                int pocetHodin = Convert.ToInt32(Math.Floor(celkoveMinuty / 60));
                return pocetHodin.ToString() + ":" + pocetMin.ToString();

            }
            else
            {
                return "Nekompletná dochádzka nutnosť opravy";
            }


        }
        [Authorize(Roles = "Admin")]
        public ActionResult KontrolaZmien(int? id)
        {
            List<GridTabulka> tb = new List<GridTabulka>();
            if (id == null) return View(tb);
            var zaznam = db.Zaznami.Where(x=> x.ZaznamId==id).ToList();
            foreach (var item in zaznam)
            {
                tb.Add(new GridTabulka(item.Cas, item.Uzivatel.Username, item.Typ,item.ZaznamId));
            }
            return View(tb);
        }
        public ActionResult LoadData()
        {
            var tabulkaLog= db.Logy.ToList();
            List<TabulkaZmien> tabZ = new List<TabulkaZmien>();
            foreach (var item in tabulkaLog)
            {
                var zmena = item.StaraHodnota;
                string stratenaHod = "";
                if (zmena != null) {
                    stratenaHod = zmena.ToString();
                }
                tabZ.Add(new TabulkaZmien { Vykonavatel = item.Uzivatel.Username,
                    Zaznam = item.ZaznamId,
                    Akcia = item.ZmenaTypu,
                    StratenaHodota =stratenaHod });
            }
            return Json(new { data = tabZ }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteData(int id)
        {
            bool success = false;
            var zaznamRem = db.Zaznami.Find(id);
            if (zaznamRem != null)
            {
                db.Zaznami.Remove(zaznamRem);
                db.SaveChanges();
                success = true;
            }
            return new JsonResult()
            {
                Data = new { success = success },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        [Authorize(Roles = "Admin")]
        public ActionResult CreateExcel()
        {
            var ulozData = db.Zaznami.Where(x => x.ZaznamId != 0).ToList();
            var filePath = Path.GetTempFileName();
            FileInfo inf = new FileInfo(filePath);
            try
            {
                if (inf.Exists) inf.Delete();
                ExcelPackage pck = new ExcelPackage(inf);
                var worksheet = pck.Workbook.Worksheets.Add("Dochadzka");
                var worksheet2 = pck.Workbook.Worksheets.Add("Mesacna Dochadzka");
                worksheet.Cells["A1"].Value = "Meno";
                worksheet.Cells["B1"].Value = "Mesiac";
                worksheet.Cells["C1"].Value = "Den";
                worksheet.Cells["D1"].Value = "Cas";
                worksheet.Cells["E1"].Value = "Typ";
                worksheet2.Cells["A1"].Value = "Meno";
                worksheet2.Cells["B1"].Value = "Mesiac";
                worksheet2.Cells["C1"].Value = "Celkovo";
                worksheet.Cells["A1:E1"].Style.Font.Bold = true;
                worksheet2.Cells["A1:C1"].Style.Font.Bold = true;
                int row = 2;
                foreach (var item in ulozData)
                {
                    worksheet.Cells["A" + row.ToString()].Value = item.Uzivatel.Username;
                    worksheet.Cells["B" + row.ToString()].Value = item.Cas.ToString("MMMM");
                    worksheet.Cells["C" + row.ToString()].Value = Int32.Parse(item.Cas.ToString("dd"));
                    worksheet.Cells["D" + row.ToString()].Value = item.Cas.ToString("HH:mm");
                    worksheet.Cells["E" + row.ToString()].Value = item.Typ == "P" ? "Prichod" : "Odchod";
                    row++;
                }
                worksheet.Cells["A1:E" + row.ToString()].AutoFitColumns();
                worksheet.Cells["A1:E1"].AutoFilter = true;
                var cisloRoku = DateTime.ParseExact(DateTime.Now.Year.ToString(), "yyyy", CultureInfo.CurrentCulture).Year;
                var pocetLudi = db.Uzivatelia.Select(x => x.UzivatelId).ToList();
                row = 2;
                for (int i = 1; i < 13; i++)
                {
                    var ulozMesiacData = db.Zaznami.Where(x => x.Cas.Month == i && x.Cas.Year == cisloRoku).GroupBy(x => x.Uzivatel.Username).Select(x => x.ToList()).ToList();
                    if (ulozMesiacData != null)
                    {
                        foreach (var item in ulozMesiacData)
                        {
                            worksheet2.Cells["A" + row.ToString()].Value = item[0].Uzivatel.Username;
                            worksheet2.Cells["B" + row.ToString()].Value = item[0].Cas.ToString("MMMM");
                            var doch = CelkovaDochadzkaZaMesiac(item);
                            worksheet2.Cells["C" + row.ToString()].Value = doch == "Nekompletná dochádzka nutnosť opravy" ? "chyba v dochadzke" : doch;
                            row++;
                        }
                    }
                }
                worksheet2.Cells["A1:C" + row.ToString()].AutoFitColumns();
                worksheet2.Cells["A1:B1"].AutoFilter = true;
                worksheet.View.FreezePanes(2, 1);
                worksheet2.View.FreezePanes(2, 1);
                pck.Save();
            }
            catch (IOException)
            {
                MessageBox.Show("Subor je otvoreny (Zatvorit subor: " + inf.Name + ")");
            }
           
            Response.Clear();
            Response.ContentType = "application/octect-stream";
            Response.AppendHeader("content-disposition", "filename=Dochadzka.xlsx");
            Response.TransmitFile(filePath);
            Response.End();
           
            return RedirectToAction("Index","Home");
        }
    }
    public class TabulkaZaznami
    {
        public DateTime Cas { get; set; }
        public string UserName { get; set; }
        public long ZaznamId { get; set; }
        public int UzivatelId { get; set; }
    }
    public class TabulkaZmien
    {
        public string Vykonavatel { get; set; }
        public long Zaznam { get; set; }
        public string StratenaHodota { get; set; }
        public bool Akcia { get; set; }
    }
}