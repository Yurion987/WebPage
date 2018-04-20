using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace daco3.Models
{
    public class GridTabulka
    {
        public string Datum { get; set; }
        public string Cas { get; set; }
        public string Meno { get; set; }
        public long ZaznamId { get; set; }
        public string Mesiac { get; set; }
        public string Odpracovane { get; set; }
        public string Typ { get; set; }
        public string SkratDoch { get; set; }
        public string WebId { get; set; }
        public GridTabulka(DateTime cas , string meno,long zaznamId,string odpracovane,string skratDoch = "")
        {
            SkratDoch = skratDoch;
            Odpracovane = odpracovane;
            ZaznamId = zaznamId;
            Cas = cas.ToString("HH:mm");
            Datum = cas.ToString("dd.MM.yyyy");
            Meno = meno;
        }
        public GridTabulka(DateTime cas, string meno,string typ,long zaznamId, string webId=null)
        {
            WebId = webId;
            ZaznamId = zaznamId;
            Typ = typ;
            Cas = cas.ToString("HH:mm");
            Datum = cas.ToString("dd.MM.yyyy");
            Meno = meno;
        }
        public GridTabulka(string meno, string mesiac, string odpracovane)
        {
            Mesiac = mesiac;
            Meno = meno;
            Odpracovane = odpracovane;
        }
    }
}