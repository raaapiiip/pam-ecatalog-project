using ItemListApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ItemListApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            UnityConfig.RegisterComponents();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            // Pastikan session tersedia
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                // Cek apakah session belum diset
                if (HttpContext.Current.Session["IsAdmin"] == null)
                {
                    string username = HttpContext.Current.User?.Identity?.Name;

                    if (!string.IsNullOrEmpty(username))
                    {
                        using (var db = new PAMCatalogContext())
                        {
                            var user = db.Users.FirstOrDefault(u => u.Windows_account == username);
                            bool isAdmin = user != null && user.IsAdmin;
                            HttpContext.Current.Session["IsAdmin"] = isAdmin;
                        }
                    }
                    else
                    {
                        HttpContext.Current.Session["IsAdmin"] = false;
                    }
                }
            }
        }

    }
}
