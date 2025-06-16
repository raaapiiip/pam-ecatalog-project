using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ItemListApp.Attributes;
using ItemListApp.Models;
using Newtonsoft.Json.Linq;

namespace ItemListApp.Controllers
{
    [CustomAdminAuthorize]
    public class ProductsController : Controller
    {
        private readonly PAMCatalogContext _context;

        public ProductsController(PAMCatalogContext context)
        {
            _context = context;
        }

        // GET: Products
        public ActionResult Index()
        {
            var products = _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.Category)
                .ToList();
            return View(products);
        }

        // GET: Products/View/Id
        public ActionResult View(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            var product = _context.Products
                            .Include(p => p.Category)
                            .FirstOrDefault(p => p.Product_id == id && !p.IsDeleted);

            if (product == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            var product = new Products();
            PrepareCategories();
            PrepareVendors();
            ViewBag.Owners = _context.Owners.ToList();
            return View(product);
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Products product, HttpPostedFileBase photoFile, HttpPostedFileBase[] drawingFiles, HttpPostedFileBase[] quotationFiles, string[] selectedOwners, string[] selectedVendors)
        {
            if (ModelState.IsValid)
            {
                // Combine selected owners
                if (selectedOwners != null && selectedOwners.Length > 0)
                {
                    product.Product_owner = string.Join("-", selectedOwners);
                }
                else
                {
                    ModelState.AddModelError("Product_owner", "At least one owner must be selected.");
                    PrepareCategories(product.Product_Category_id);
                    PrepareVendors();
                    return View(product);
                }

                // Combine selected vendors
                product.Product_vendor_name = (selectedVendors != null && selectedVendors.Length > 0)
                    ? string.Join(", ", selectedVendors)
                    : null;

                // Check if the accessory name already exists (must be unique)
                bool isAccessoriesNameExists = _context.Products
                    .Any(p => !p.IsDeleted
                            && p.Product_accessories_name == product.Product_accessories_name
                            && p.Product_id != product.Product_id);
                if (isAccessoriesNameExists)
                {
                    ModelState.AddModelError("Product_accessories_name", "Accessories name must be unique.");
                }

                // Maximum limit for each uploaded file = 16 MB
                int maxFileSize = 16 * 1024 * 1024;

                // Validate photo file
                if (photoFile != null && photoFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("Product_photo_filepath", "Photo file must be less than or equal to 16 MB.");
                    return View(product);
                }

                // Validate all drawing files
                if (drawingFiles != null)
                {
                    foreach (var file in drawingFiles)
                    {
                        if (file != null && file.ContentLength > maxFileSize)
                        {
                            ModelState.AddModelError("Product_drawing_filepath", "Each drawing file must be ≤ 16 MB.");
                            return View(product);
                        }
                    }
                }

                // Validate all quotation files
                if (quotationFiles != null)
                {
                    foreach (var file in quotationFiles)
                    {
                        if (file != null && file.ContentLength > maxFileSize)
                        {
                            ModelState.AddModelError("Product_quotation_filepath", "Each quotation file must be ≤ 16 MB.");
                            return View(product);
                        }
                    }
                }

                // Retrieve category name based on Product_Category_id
                var category = _context.Categories.FirstOrDefault(c => c.Category_id == product.Product_Category_id);
                if (category == null)
                {
                    ModelState.AddModelError("Product_Category_id", "Invalid category.");
                    return View(product);
                }

                // Define storage folder based on Category_name
                string categoryFolder = $"~/Files/{category.Category_name}/";
                string serverPath = Server.MapPath(categoryFolder);

                if (!Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }

                // Upload photo file
                if (photoFile != null && photoFile.ContentLength > 0)
                {
                    string photoExt = Path.GetExtension(photoFile.FileName).ToLower();
                    string[] allowedPhotoExt = { ".jpg", ".jpeg", ".png", ".gif" };

                    if (!allowedPhotoExt.Contains(photoExt))
                    {
                        ModelState.AddModelError("Product_photo_filepath", "Photo format must be JPG, JPEG, PNG, or GIF.");
                        return View(product);
                    }

                    string photoFileName = Path.GetFileName(photoFile.FileName);
                    string photoPath = Path.Combine(categoryFolder, photoFileName);
                    string serverPhotoPath = Server.MapPath(photoPath);
                    photoFile.SaveAs(serverPhotoPath);
                    product.Product_photo_filepath = photoPath;
                }

                // Upload multiple drawing files
                List<string> drawingPaths = new List<string>();
                string[] allowedDrawingExt = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

                if (drawingFiles != null)
                {
                    foreach (var file in drawingFiles)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            string ext = Path.GetExtension(file.FileName).ToLower();
                            if (!allowedDrawingExt.Contains(ext))
                            {
                                ModelState.AddModelError("Product_drawing_filepath", "Invalid file format for drawing.");
                                return View(product);
                            }

                            string fileName = Path.GetFileName(file.FileName);
                            string path = Path.Combine(categoryFolder, fileName);
                            string serverPathFile = Server.MapPath(path);
                            file.SaveAs(serverPathFile);
                            drawingPaths.Add(path);
                        }
                    }

                    product.Product_drawing_filepath = drawingPaths.Count > 0
                        ? string.Join(";", drawingPaths)
                        : null;
                }

                // Upload multiple quotation files
                List<string> quotationPaths = new List<string>();
                string[] allowedQuotationExt = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

                if (quotationFiles != null)
                {
                    foreach (var file in quotationFiles)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            string ext = Path.GetExtension(file.FileName).ToLower();
                            if (!allowedQuotationExt.Contains(ext))
                            {
                                ModelState.AddModelError("Product_quotation_filepath", "Invalid file format for quotation.");
                                return View(product);
                            }

                            string fileName = Path.GetFileName(file.FileName);
                            string path = Path.Combine(categoryFolder, fileName);
                            string serverPathFile = Server.MapPath(path);
                            file.SaveAs(serverPathFile);
                            quotationPaths.Add(path);
                        }
                    }

                    product.Product_quotation_filepath = quotationPaths.Count > 0
                        ? string.Join(";", quotationPaths)
                        : null;
                }

                // Save create action from user
                product.CreatedBy = User.Identity.Name;

                // Save product to database
                _context.Products.Add(product);
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"New product \"{product.Product_accessories_name}\" successfully added.";
                return RedirectToAction("Index");
            }

            PrepareCategories(product.Product_Category_id);
            PrepareVendors();
            ViewBag.Owners = _context.Owners.ToList();
            TempData["ErrorMessage"] = $"Failed to add new product \"{product.Product_accessories_name}\".";
            return View(product);
        }

        // GET: Products/Edit/Id
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            var product = _context.Products.FirstOrDefault(p => p.Product_id == id && !p.IsDeleted);

            if (product == null)
            {
                return RedirectToAction("Index", "Home");
            }

            PrepareCategories(product.Product_Category_id);
            PrepareVendors(product.Product_vendor_name?.Split(',').Select(v => v.Trim()).ToArray());
            ViewBag.Owners = _context.Owners.ToList();
            return View(product);
        }

        // POST: Products/Edit/Id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Products product, HttpPostedFileBase photoFile, HttpPostedFileBase[] drawingFiles, HttpPostedFileBase[] quotationFiles,
                                 bool? removePhoto, string[] removeDrawingFiles, string[] removeQuotationFiles, string[] selectedOwners, string[] selectedVendors)
        {
            if (ModelState.IsValid)
            {
                // Combine selected owners
                if (selectedOwners != null && selectedOwners.Length > 0)
                {
                    product.Product_owner = string.Join("-", selectedOwners);
                }
                else
                {
                    ModelState.AddModelError("Product_owner", "At least one owner must be selected.");
                    PrepareCategories(product.Product_Category_id);
                    PrepareVendors(product.Product_vendor_name?.Split(',').Select(v => v.Trim()).ToArray());
                    return View(product);
                }

                // Combine selected vendors
                product.Product_vendor_name = (selectedVendors != null && selectedVendors.Length > 0)
                    ? string.Join(", ", selectedVendors)
                    : null;

                // Check if the accessory name already exists (must be unique)
                bool isAccessoriesNameExists = _context.Products
                    .Any(p => !p.IsDeleted
                            && p.Product_accessories_name == product.Product_accessories_name
                            && p.Product_id != product.Product_id);
                if (isAccessoriesNameExists)
                {
                    ModelState.AddModelError("Product_accessories_name", "Accessories name must be unique.");
                }

                // Maximum limit for each uploaded file = 16 MB
                int maxFileSize = 16 * 1024 * 1024;

                // Validate photo file
                if (photoFile != null && photoFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("Product_photo_filepath", "Photo file must be less than or equal to 16 MB.");
                    return View(product);
                }

                // Validate all drawing files
                if (drawingFiles != null)
                {
                    foreach (var file in drawingFiles)
                    {
                        if (file != null && file.ContentLength > maxFileSize)
                        {
                            ModelState.AddModelError("Product_drawing_filepath", "Each drawing file must be ≤ 16 MB.");
                            return View(product);
                        }
                    }
                }

                // Validate all quotation files
                if (quotationFiles != null)
                {
                    foreach (var file in quotationFiles)
                    {
                        if (file != null && file.ContentLength > maxFileSize)
                        {
                            ModelState.AddModelError("Product_quotation_filepath", "Each quotation file must be ≤ 16 MB.");
                            return View(product);
                        }
                    }
                }

                // Retrieve category name based on Product_Category_id
                var category = _context.Categories.FirstOrDefault(c => c.Category_id == product.Product_Category_id);
                if (category == null)
                {
                    ModelState.AddModelError("Product_Category_id", "Invalid category.");
                    return View(product);
                }

                // Define storage folder based on Category_name
                string categoryFolder = $"~/Files/{category.Category_name}/";
                string serverPath = Server.MapPath(categoryFolder);

                if (!Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }

                // Remove existing photo file
                if (removePhoto == true && !string.IsNullOrEmpty(product.Product_photo_filepath))
                {
                    string singlePhotoPath = Server.MapPath(product.Product_photo_filepath);
                    if (System.IO.File.Exists(singlePhotoPath))
                    {
                        System.IO.File.Delete(singlePhotoPath);
                    }
                    product.Product_photo_filepath = null;
                }

                // Remove existing drawing files
                if (removeDrawingFiles != null && removeDrawingFiles.Length > 0)
                {
                    var currentDrawingPaths = (product.Product_drawing_filepath ?? "")
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                    foreach (var fileNameToRemove in removeDrawingFiles)
                    {
                        var matchingPath = currentDrawingPaths.FirstOrDefault(p => Path.GetFileName(p).Equals(fileNameToRemove, StringComparison.OrdinalIgnoreCase));
                        if (!string.IsNullOrEmpty(matchingPath))
                        {
                            var fullPath = Server.MapPath(matchingPath);
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }
                            currentDrawingPaths.Remove(matchingPath);
                        }
                    }

                    product.Product_drawing_filepath = currentDrawingPaths.Any()
                        ? string.Join(";", currentDrawingPaths)
                        : null;
                }

                // Remove existing quotation files
                if (removeQuotationFiles != null && removeQuotationFiles.Length > 0)
                {
                    var currentQuotationPaths = (product.Product_quotation_filepath ?? "")
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                    foreach (var fileNameToRemove in removeQuotationFiles)
                    {
                        var matchingPath = currentQuotationPaths.FirstOrDefault(p => Path.GetFileName(p).Equals(fileNameToRemove, StringComparison.OrdinalIgnoreCase));
                        if (!string.IsNullOrEmpty(matchingPath))
                        {
                            var fullPath = Server.MapPath(matchingPath);
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }
                            currentQuotationPaths.Remove(matchingPath);
                        }
                    }

                    product.Product_quotation_filepath = currentQuotationPaths.Any()
                        ? string.Join(";", currentQuotationPaths)
                        : null;
                }

                // Upload photo file
                if (photoFile != null && photoFile.ContentLength > 0)
                {
                    string photoExt = Path.GetExtension(photoFile.FileName).ToLower();
                    string[] allowedPhotoExt = { ".jpg", ".jpeg", ".png", ".gif" };

                    if (!allowedPhotoExt.Contains(photoExt))
                    {
                        ModelState.AddModelError("Product_photo_filepath", "Photo format must be JPG, JPEG, PNG, or GIF.");
                        return View(product);
                    }

                    string photoFileName = Path.GetFileName(photoFile.FileName);
                    string photoPath = Path.Combine(categoryFolder, photoFileName);
                    string serverPhotoPath = Server.MapPath(photoPath);
                    photoFile.SaveAs(serverPhotoPath);
                    product.Product_photo_filepath = photoPath;
                }

                // Upload multiple drawing files
                var existingDrawingPaths = (product.Product_drawing_filepath ?? "")
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                List<string> newDrawingPaths = new List<string>();
                string[] allowedDrawingExt = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

                if (drawingFiles != null)
                {
                    foreach (var file in drawingFiles)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            string ext = Path.GetExtension(file.FileName).ToLower();
                            if (!allowedDrawingExt.Contains(ext))
                            {
                                ModelState.AddModelError("Product_drawing_filepath", "Invalid file format for drawing.");
                                return View(product);
                            }

                            string fileName = Path.GetFileName(file.FileName);
                            string path = Path.Combine(categoryFolder, fileName);
                            string serverPathFile = Server.MapPath(path);
                            file.SaveAs(serverPathFile);
                            newDrawingPaths.Add(path);
                        }
                    }
                }

                product.Product_drawing_filepath = string.Join(";", existingDrawingPaths.Concat(newDrawingPaths));

                // Upload multiple quotation files
                var existingQuotationPaths = (product.Product_quotation_filepath ?? "")
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                List<string> newQuotationPaths = new List<string>();
                string[] allowedQuotationExt = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

                if (quotationFiles != null)
                {
                    foreach (var file in quotationFiles)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            string ext = Path.GetExtension(file.FileName).ToLower();
                            if (!allowedQuotationExt.Contains(ext))
                            {
                                ModelState.AddModelError("Product_quotation_filepath", "Invalid file format for quotation.");
                                return View(product);
                            }

                            string fileName = Path.GetFileName(file.FileName);
                            string path = Path.Combine(categoryFolder, fileName);
                            string serverPathFile = Server.MapPath(path);
                            file.SaveAs(serverPathFile);
                            newQuotationPaths.Add(path);
                        }
                    }
                }

                product.Product_quotation_filepath = string.Join(";", existingQuotationPaths.Concat(newQuotationPaths));

                // Retrieve old data from the database
                var existingProduct = _context.Products.AsNoTracking().FirstOrDefault(p => p.Product_id == product.Product_id);
                if (existingProduct == null)
                {
                    TempData["ErrorMessage"] = $"Product not found.";
                    return RedirectToAction("Index");
                }

                // Copy the values that are not resubmitted from the form and stores the information of the user who edited
                product.CreatedBy = existingProduct.CreatedBy;
                product.UpdatedBy = User.Identity.Name;

                // Save product to database
                _context.Entry(product).State = EntityState.Modified;
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Product \"{product.Product_accessories_name}\" successfully updated.";
                return RedirectToAction("Index");
            }

            PrepareCategories(product.Product_Category_id);
            PrepareVendors(product.Product_vendor_name?.Split(',').Select(v => v.Trim()).ToArray());
            ViewBag.Owners = _context.Owners.ToList();
            TempData["ErrorMessage"] = $"Failed to update product \"{product.Product_accessories_name}\".";
            return View(product);
        }

        // GET: Products/Delete/Id
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            Products product = _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Product_id == id && !p.IsDeleted);

            if (product == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(product);
        }

        // POST: Products/Delete/Id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Product_id == id && !p.IsDeleted);

            if (product != null)
            {
                // Save delete action from user
                product.IsDeleted = true;
                product.DeletedBy = User.Identity.Name;

                _context.SaveChanges();
            }

            TempData["SuccessMessage"] = $"Product \"{product.Product_accessories_name}\" successfully deleted.";
            return RedirectToAction("Index");
        }

        public JsonResult CheckUnique(string field, string value, int? id = null)
        {
            bool exists = false;
            string errorMessage = null;

            if (field == "Product_accessories_name")
            {
                exists = _context.Products.Any(p => !p.IsDeleted && p.Product_accessories_name == value && p.Product_id != id);
                if (exists)
                {
                    errorMessage = $"'{value}' already used. Please use a different accessories name.";
                }
            }

            return Json(new { isValid = !exists, message = errorMessage }, JsonRequestBehavior.AllowGet);
        }

        private void PrepareCategories(int? selectedCategory = null)
        {
            var categories = _context.Categories
                .OrderBy(c => c.Category_name)
                .Select(c => new SelectListItem
                {
                    Value = c.Category_id.ToString(),
                    Text = c.Category_name,
                    Selected = selectedCategory.HasValue && c.Category_id == selectedCategory.Value
                })
                .ToList();

            categories.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "Select category...",
                Selected = !selectedCategory.HasValue
            });

            ViewData["Product_Category_id"] = categories;
        }

        private void PrepareVendors(string[] selectedVendors = null)
        {
            var selectedVendorList = selectedVendors?.ToList() ?? new List<string>();

            var allVendors = _context.Vendors.ToList();

            ViewData["Product_vendor_name"] = allVendors
                .Select(v => new SelectListItem
                {
                    Value = v.Vendor_name,
                    Text = v.Vendor_name,
                    Selected = selectedVendorList.Contains(v.Vendor_name)
                })
                .OrderBy(v => v.Text)
                .ToList();
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