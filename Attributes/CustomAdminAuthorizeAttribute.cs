using ItemListApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ItemListApp.Attributes
{
    public class CustomAdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var username = httpContext.User.Identity.Name;

            using (var db = new PAMCatalogContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Windows_account == username);
                return user != null && user.IsAdmin;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/Home/Index");
        }
    }
}