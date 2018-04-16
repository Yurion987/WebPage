using daco3.Helpers;
using daco3.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace daco3
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private void zbierajData()
        {
            LoadData d = new LoadData();
            while (true)
            {
                
                d.WebParsing();
                Trace.TraceInformation("Uspavam thread");
                Thread.Sleep(180000);
            }
        }
      
        protected void Application_Start()
        {
            
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            Thread t = new Thread(zbierajData);
            Trace.TraceInformation("Starting thread");
            t.Start();
        }


        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                Uzivatel serializeModel = Newtonsoft.Json.JsonConvert.DeserializeObject<Uzivatel>(authTicket.UserData);

                MojPrincipal newUser = new MojPrincipal(serializeModel);

                HttpContext.Current.User = newUser;
            }
            else
            {
                if (Request.Url.AbsolutePath.Contains("Home"))
                {
                    // redirect to unauthorize
                }
            }
        }
    }
}
