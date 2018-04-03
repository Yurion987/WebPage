using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace daco3.Models
{
    [Table("Rola")]
    public class Rola
    {
        [Key]
        public int RolaId { get; set; }

        [Required]
        [StringLength(15)]
        public string Nazov { get; set; }

    }
}