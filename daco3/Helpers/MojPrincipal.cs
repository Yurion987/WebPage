using daco3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace daco3.Helpers
{
    public class MojPrincipal : IPrincipal
    {
        public MojPrincipal(Uzivatel uzivatel)
        {
            this.Uzivatel = uzivatel;
        }

        public Uzivatel Uzivatel { get; set; }
        public IIdentity Identity => new GenericIdentity(Uzivatel.Username);

        public bool IsInRole(string role)
        {
            //todo zisit is id roli napr cez case
            //switch (role) {
            //    case "admin":return Uzivatel.RolaId == 1;
            //}
            return true;
        }
    }
}