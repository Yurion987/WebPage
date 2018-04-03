using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace daco3.Models
{
    public class Tabulka
    {
        public List<Zaznam> tableZaznami { get; set; }

        public Tabulka() {
            this.tableZaznami = new List<Zaznam>();
        }
    }
}