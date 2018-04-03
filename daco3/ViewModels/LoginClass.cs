using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace daco3.Models
{
    public class LoginClass
    {
        [Required]
        public string Meno { get;set; }
        [Required]
        public string Heslo { get; set; }


    }
}