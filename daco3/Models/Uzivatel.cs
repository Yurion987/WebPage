using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace daco3.Models
{
    [Table("Uzivatel")]
    public class Uzivatel
    {
        [Key]
        public int UzivatelId { get; set; }
        [Required]
        [StringLength(25)]
        public string Username { get; set; }
        [Required]
        public string Heslo { get; set; }
        public int RolaId { get; set; }
        [ForeignKey("RolaId")]
        public virtual Rola Rola { get; set; }
    }
}