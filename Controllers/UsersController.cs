using ItemListApp.Attributes;
using ItemListApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ItemListApp.Controllers
{
    [CustomAdminAuthorize]
    public class UsersController : Controller
    {
        private readonly PAMCatalogContext _context;

        public UsersController(PAMCatalogContext context)
        {
            _context = context;
        }

        // GET: Users
        public ActionResult Index(string status = "All")
        {
            var users = _context.Users.AsQueryable();

            switch (status)
            {
                case "All":
                default:
                    break;
                case "Admin":
                    users = users.Where(u => u.IsActive);
                    break;
                case "User":
                    users = users.Where(u => !u.IsActive);
                    break;
            }

            ViewBag.CurrentStatus = status;
            return View(users.ToList());
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            var user = new Users();
            return View(user);
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "User_name,Windows_account,IsAdmin,IsActive")] Users user)
        {
            if (ModelState.IsValid)
            {
                // Check if the username or windows account already exists (must be unique)
                bool isUserNameExists = _context.Users.Any(u => u.User_name == user.User_name);
                if (isUserNameExists)
                {
                    ModelState.AddModelError("User_name", "Username must be unique.");
                }

                bool isWindowsAccountExists = _context.Users.Any(u => u.Windows_account == user.Windows_account);
                if (isWindowsAccountExists)
                {
                    ModelState.AddModelError("Windows_account", "Windows account must be unique.");
                }

                user.IsAdmin = true;
                user.IsActive = true;
                _context.Users.Add(user);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "New user successfully added.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Failed to add new user.";
            return View(user);
        }

        // GET: Users/Edit/Id
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            Users user = _context.Users.Find(id);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        // POST: Users/Edit/Id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "User_id,User_name,Windows_account")] Users user)
        {
            if (ModelState.IsValid)
            {
                // Check if the username or windows account already exists (must be unique)
                bool isUserNameExists = _context.Users.Any(u => u.User_name == user.User_name);
                if (isUserNameExists)
                {
                    ModelState.AddModelError("User_name", "Username must be unique.");
                }

                bool isWindowsAccountExists = _context.Users.Any(u => u.Windows_account == user.Windows_account);
                if (isWindowsAccountExists)
                {
                    ModelState.AddModelError("Windows_account", "Windows account must be unique.");
                }

                _context.Entry(user).State = EntityState.Modified;
                _context.SaveChanges();
                TempData["SuccessMessage"] = "User successfully updated.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Failed to update user.";
            return View(user);
        }

        // POST: Users/Deactivate/Id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            Users user = _context.Users.Find(id);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            user.IsAdmin = false;
            user.IsActive = false;
            _context.SaveChanges();
            TempData["SuccessMessage"] = "User successfully deactivated.";

            return RedirectToAction("Index");
        }

        // POST: Users/Activate/Id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Activate(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            Users user = _context.Users.Find(id);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            user.IsAdmin = true;
            user.IsActive = true;
            _context.SaveChanges();
            TempData["SuccessMessage"] = "User successfully activated.";

            return RedirectToAction("Index");
        }

        public JsonResult CheckUnique(string field, string value, int? id = null)
        {
            bool exists = false;
            string errorMessage = null;

            if (field == "User_name")
            {
                exists = _context.Users.Any(u => u.User_name == value && u.User_id != id);
                if (exists)
                {
                    errorMessage = $"'{value}' already exists. Please use a different username.";
                }
            }
            else if (field == "Windows_account")
            {
                exists = _context.Users.Any(u => u.Windows_account == value && u.User_id != id);
                if (exists)
                {
                    errorMessage = $"'{value}' already exists. Please use a different windows account.";
                }
            }

            return Json(new { isValid = !exists, message = errorMessage }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}