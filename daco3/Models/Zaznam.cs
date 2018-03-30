using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace daco3.Models
{
    public class Zaznam
    {
        private string meno;
        private string datum;
        private string cas;
        private string typ;

        public Zaznam(string meno, string datum, string cas, string typ)
        {
            this.meno = meno;
            this.datum = datum;
            this.cas = cas;
            this.typ = typ;
        }

        public string Meno { get => meno; set => meno = value; }
        public string Datum { get => datum; set => datum = value; }
        public string Cas { get => cas; set => cas = value; }
        public string Typ { get => typ; set => typ = value; }
    }
}