using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace daco3.Models
{
    [Table("Log")]
    public class Log
    {
        [Key]
        public int LogId { get; set; }

        
        public long ZaznamId { get; set; }

        
        public int UzivatelId { get; set; }

        public DateTime? StaraHodnota { get; set; }
        public bool ZmenaTypu { get; set; }

        [ForeignKey("UzivatelId")]
        public virtual Uzivatel Uzivatel { get; set; }
        [ForeignKey("ZaznamId")]
        public virtual Zaznam Zaznam { get; set; }
    }
}