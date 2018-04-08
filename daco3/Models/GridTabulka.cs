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

        public string Odpracovane { get; set; }
        public string Typ { get; set; }
        public GridTabulka(DateTime cas , string meno,long zaznamId,string odpracovane)
        {
            this.Odpracovane = odpracovane;
            this.ZaznamId = zaznamId;
            this.Cas = cas.ToString("HH:mm");
            this.Datum = cas.ToString("dd.MM.yyyy");
            this.Meno = meno;
        }
        public GridTabulka(DateTime cas, string meno,string typ,long zaznamId)
        {
            this.ZaznamId = zaznamId;
            this.Typ = typ;
            this.Cas = cas.ToString("HH:mm");
            this.Datum = cas.ToString("dd.MM.yyyy");
            this.Meno = meno;
        }
        public GridTabulka(string cas, string meno, string typ, long zaznamId) {
            this.ZaznamId = zaznamId;
            this.Typ = typ;
            this.Cas = cas;
        }
    }
}