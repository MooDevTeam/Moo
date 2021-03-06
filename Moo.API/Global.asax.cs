﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Moo.Core.DB;
using Moo.Core.Daemon;
namespace Moo.API
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            using (MooDB db = new MooDB())
            {
                if (!db.DatabaseExists())
                {
                    DatabaseInstaller.Install(db);
                    MooTestData.AddTestData(db);
                }
            }

            TestDaemon.Instance.TestComplete += (o, record) =>
            {
                WebSockets.WebSocketsAPIHandler.NotifyTestComplete(record);
            };
            Daemon.StartAll();
        }

        protected void Application_End(object sender, EventArgs e)
        {
            Daemon.StopAll();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                //HttpContext.Current.Response.AddHeader("Cache-Control", "no-cache");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, DELETE, OPTIONS, PUT");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", HttpContext.Current.Request.Headers["Access-Control-Request-Headers"]);
                HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
                HttpContext.Current.Response.End();
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }


    }
}