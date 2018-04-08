using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Text;
using System.Linq;
using daco3.Models;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Security.Cryptography;

namespace daco3.Helpers
{

    class LoadData
    {
        
        private CookieContainer cookies = new CookieContainer();
       
        public LoadData()
        {
           
        }

        public void WebParsing()
        {
            System.Diagnostics.Trace.TraceInformation("web Parsing started");
            string loginPageReq = "https://www.jablonet.net/ajax/login.php";
            string dataPageReq = "https://www.jablonet.net/app/ja100?service=257168";
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(loginPageReq);
            webRequest.CookieContainer = cookies;
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            string reqBody = String.Format("login=uhrin9%40stud.uniza.sk&heslo=DGbfhk&aStatus=200&loginType=Login");
            try
            {
                using (StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream()))
                {

                    requestWriter.Write(reqBody);
                    requestWriter.Close();

                    using (HttpWebResponse res = (HttpWebResponse)webRequest.GetResponse())
                    {
                        if (res.StatusCode != HttpStatusCode.OK)
                            throw new WebException("Logon failed", null, WebExceptionStatus.Success, res);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Nepresiel Logom",ex);
            }
            System.Diagnostics.Trace.TraceInformation("Nalogoval sa");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dataPageReq);
            request.CookieContainer = cookies;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var webPage = new HtmlDocument();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }
                webPage.LoadHtml(readStream.ReadToEnd());
                response.Close();
                readStream.Close();
            }
            Trace.TraceInformation("Na stranke");
            var vsetkyZaznamy = webPage.DocumentNode.SelectSingleNode("//*[@id='timeline']").SelectNodes(".//li[contains(@class, 'ON')]");
            string datum = webPage.DocumentNode.SelectSingleNode("/html/body/div/div/div[3]/div[2]/div/div/div/div/div/div[1]/div/div[1]/div/span[2]").InnerText;
            if (datum.Equals("")) datum = webPage.DocumentNode.SelectNodes("/html/body/div/div/div[3]/div[2]/div/div/div/div/div/div[1]/div/div[1]/div/span[1]")[0].InnerText;
            datum = FormatujDatum(datum);

            Trace.TraceInformation("Vsetky zaznami: "+vsetkyZaznamy.Count);
            var db = new Databaza();
            for (int i = vsetkyZaznamy.Count-1; i >= 0; i--)
            {
                
                var IdZaznamu = vsetkyZaznamy[i].Id;
                Trace.TraceInformation($"Pokus o vlozenie {IdZaznamu}");
                
                var check = db.Zaznami.Any(z => z.ZaznamIdWeb == IdZaznamu);
                Trace.TraceInformation($"Existuje zaznam? {check}");
                if (!check) {
                    Trace.TraceInformation("Vkladanie zaznamu");
                    var meno = vsetkyZaznamy[i].SelectNodes(".//div[contains(@class, 'name')]")[0].InnerText;
                    var time = vsetkyZaznamy[i].SelectNodes(".//div[contains(@class, 'time')]")[0].InnerText.Replace(" ", "");
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("sk-SK");
                    var cas = DateTime.Parse(datum + " "+time,CultureInfo.CurrentCulture);
                    
                    if (db.Uzivatelia.Where(u => u.Username == meno).FirstOrDefault() == null)
                    {

                        db.Uzivatelia.Add(new Uzivatel() { Heslo =Hash.ZaHashuj("heslo"), Username = meno, RolaId = 2});
                        db.SaveChanges();
                    }
                    var predchadzajuci = db.Zaznami.Where(z => z.Uzivatel.Username == meno).OrderByDescending(z => z.Cas).FirstOrDefault();
                    if (predchadzajuci == null)
                    {
                        var zaznamUzivatel = db.Uzivatelia.Where(m => m.Username == meno).FirstOrDefault().UzivatelId;
                        db.Zaznami.Add(new Zaznam() { Cas = cas, Typ = "P", UzivatelId = zaznamUzivatel, ZaznamIdWeb = IdZaznamu });
                        db.SaveChanges();
                    } else if (cas.Day == predchadzajuci.Cas.Day && cas.Month == predchadzajuci.Cas.Month && cas.Year == predchadzajuci.Cas.Year)
                    {
                        var typ = predchadzajuci.Typ == "O" ? "P" : "O";
                        db.Zaznami.Add(new Zaznam() { Cas = cas, Typ = typ, UzivatelId = predchadzajuci.UzivatelId, ZaznamIdWeb = IdZaznamu });
                        db.SaveChanges();
                    } else
                    {
                        var typ = "P";
                        db.Zaznami.Add(new Zaznam() { Cas = cas, Typ = typ, UzivatelId = predchadzajuci.UzivatelId, ZaznamIdWeb = IdZaznamu });
                        db.SaveChanges();
                    }
                }
            }
            Trace.TraceInformation("Skoncil Parsing");
        }
        public string FormatujDatum(string datum)
        {
            datum = Regex.Replace(datum, "[^A-Za-z0-9 ]", "");
            var originDatum = datum.Split(' ');
            if (originDatum[0].Length != 2) originDatum[0] = "0" + originDatum[0];
            switch (originDatum[1])
            {
                case "January":
                case "1":
                    originDatum[1] = "01";
                    break;
                case "February":
                case "2":
                    originDatum[1] = "02";
                    break;
                case "March":
                case "3":
                    originDatum[1] = "03";
                    break;
                case "April":
                case "4":
                    originDatum[1] = "04";
                    break;
                case "May":
                case "5":
                    originDatum[1] = "05";
                    break;
                case "June":
                case "6":
                    originDatum[1] = "06";
                    break;
                case "July":
                case "7":
                    originDatum[1] = "07";
                    break;
                case "August":
                case "8":
                    originDatum[1] = "08";
                    break;
                case "September":
                case "9":
                    originDatum[1] = "09";
                    break;
                case "October":
                    originDatum[1] = "10";
                    break;
                case "November":
                    originDatum[1] = "11";
                    break;
                case "December":
                    originDatum[1] = "12";
                    break;
                default:
                    break;
            }
            string celyDatum = "";
            for (int i = 0; i < originDatum.Length; i++)
            {
                if (i < 2)
                {
                    celyDatum += originDatum[i] + ".";
                }
                else
                {
                    celyDatum += originDatum[i];
                    break;
                }

            }
            return celyDatum;
        }

    
    }
    public class CookieAwareWebClient : WebClient
    {
        public CookieAwareWebClient(CookieContainer cont)
        {
            CookieContainer = cont;
        }
        public CookieContainer CookieContainer { get; private set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = CookieContainer;
            return request;
        }
    }
}
