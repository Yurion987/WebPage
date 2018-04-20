using daco3.Models;
using System.Security.Principal;


namespace daco3.Helpers
{
    public class MojPrincipal : IPrincipal
    {
        public MojPrincipal(Uzivatel uzivatel)
        {
            Uzivatel = uzivatel;
        }

        public Uzivatel Uzivatel { get; set; }
        public IIdentity Identity => new GenericIdentity(Uzivatel.Username);

        public bool IsInRole(string role)
        {
            if (Uzivatel.Rola.Nazov==role) {
                return true;
            }
            return false;
         
        }
    }
}