using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace daco3.Models
{
    [Table("Zaznam")]
    public class Zaznam
    {
        [Key]
        public long ZaznamId { get; set; }
        public string ZaznamIdWeb { get; set; }
        [Required]
        public DateTime Cas { get; set; }
        public int UzivatelId { get; set; }
        [Required]
        [StringLength(1)]
        public string Typ { get; set; }
        [ForeignKey("UzivatelId")]
        public virtual Uzivatel Uzivatel { get; set; }
    }
}