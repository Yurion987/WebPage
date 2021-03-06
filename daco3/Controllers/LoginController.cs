﻿using daco3.Helpers;
using daco3.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace daco3.Controllers
{
    [RequireHttps]
    public class LoginController : Controller
    {
        private Databaza db { get; set; }

        public LoginController()
        {
           db = new Databaza();
        }

        public ActionResult Index()
        {

            var loginData = new LoginClass();
            ViewBag.Err = "";
            return View(loginData);
        }
        public ActionResult Login(LoginClass model)
        {
            var heslo = Hash.ZaHashuj(model.Heslo);
            var user = db.Uzivatelia.FirstOrDefault(u => u.Username == model.Meno && heslo == u.Heslo);
            if (user != null)
            {
                string userData = Newtonsoft.Json.JsonConvert.SerializeObject(user);
                FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                         1,
                         user.Username,
                         DateTime.Now,
                         DateTime.Now.AddMinutes(15),
                         false,
                         userData);
                string encTicket = FormsAuthentication.Encrypt(authTicket);
                HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                faCookie.Expires = DateTime.Now.AddDays(1);
                Response.Cookies.Add(faCookie);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Err = "Zle zadané údaje";
            return View("Index", model);
        }
    }
}