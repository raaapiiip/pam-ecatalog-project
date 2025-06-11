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
        public ActionResult Create(Products product, HttpPostedFileBase photoFile, HttpPostedFileBase drawingFile, HttpPostedFileBase quotationFile, string[] SelectedOwners, string[] SelectedVendors)
        {
            if (ModelState.IsValid)
            {
                // Combine selected owners
                if (SelectedOwners != null && SelectedOwners.Length > 0)
                {
                    product.Product_owner = string.Join("-", SelectedOwners);
                }
                else
                {
                    ModelState.AddModelError("Product_owner", "At least one owner must be selected.");
                    PrepareCategories(product.Product_Category_id);
                    PrepareVendors();
                    return View(product);
                }

                // Combine selected vendors
                product.Product_vendor_name = (SelectedVendors != null && SelectedVendors.Length > 0)
                    ? string.Join(", ", SelectedVendors)
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

                if (!ModelState.IsValid)
                {
                    PrepareCategories(product.Product_Category_id);
                    PrepareVendors();
                    ViewBag.Owners = _context.Owners.ToList();
                    return View(product);
                }

                // Maximum limit for each uploaded file = 16 MB
                int maxFileSize = 16 * 1024 * 1024;

                if ((photoFile != null && photoFile.ContentLength > maxFileSize) ||
                    (drawingFile != null && drawingFile.ContentLength > maxFileSize) ||
                    (quotationFile != null && quotationFile.ContentLength > maxFileSize))
                {
                    ModelState.AddModelError("", "Each uploaded file must be less than or equal to 16 MB.");
                    return View(product);
                }

                // Retrieve Category Name Based on Product_Category_id
                var category = _context.Categories.FirstOrDefault(c => c.Category_id == product.Product_Category_id);
                if (category == null)
                {
                    ModelState.AddModelError("Product_Category_id", "Invalid category.");
                    return View(product);
                }

                // Define Storage Folder Based on Category Name
                string categoryFolder = $"~/Files/{category.Category_name}/";
                string serverPath = Server.MapPath(categoryFolder);

                if (!Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }

                // Upload Photo Files
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

                // Upload Drawing Files
                if (drawingFile != null && drawingFile.ContentLength > 0)
                {
                    string drawingExt = Path.GetExtension(drawingFile.FileName).ToLower();
                    string[] allowedDrawingExt = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

                    if (!allowedDrawingExt.Contains(drawingExt))
                    {
                        ModelState.AddModelError("Product_drawing_filepath", "Drawing format must be JPG, JPEG, PNG, PDF, DOC, DOCX, XLS, or XLSX.");
                        return View(product);
                    }

                    string drawingFileName = Path.GetFileName(drawingFile.FileName);
                    string drawingPath = Path.Combine(categoryFolder, drawingFileName);
                    string serverDrawingPath = Server.MapPath(drawingPath);
                    drawingFile.SaveAs(serverDrawingPath);
                    product.Product_drawing_filepath = drawingPath;
                }

                // Upload Quotation Files
                if (quotationFile != null && quotationFile.ContentLength > 0)
                {
                    string quotationExt = Path.GetExtension(quotationFile.FileName).ToLower();
                    string[] allowedQuotationExt = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

                    if (!allowedQuotationExt.Contains(quotationExt))
                    {
                        ModelState.AddModelError("Product_quotation_filepath", "Quotation format must be JPG, JPEG, PNG, PDF, DOC, DOCX, XLS, or XLSX.");
                        return View(product);
                    }

                    string quotationFileName = Path.GetFileName(quotationFile.FileName);
                    string quotationPath = Path.Combine(categoryFolder, quotationFileName);
                    string serverQuotationPath = Server.MapPath(quotationPath);
                    quotationFile.SaveAs(serverQuotationPath);
                    product.Product_quotation_filepath = quotationPath;
                }

                // Save create action from user
                product.CreatedBy = User.Identity.Name;

                // Save Product to Database
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
        public ActionResult Edit(Products product, HttpPostedFileBase photoFile, HttpPostedFileBase drawingFile, HttpPostedFileBase quotationFile,
                                 bool? removePhoto, bool? removeDrawing, bool? removeQuotation, string[] SelectedOwners, string[] SelectedVendors)
        {
            if (ModelState.IsValid)
            {
                // Combine selected owners
                if (SelectedOwners != null && SelectedOwners.Length > 0)
                {
                    product.Product_owner = string.Join("-", SelectedOwners);
                }
                else
                {
                    ModelState.AddModelError("Product_owner", "At least one owner must be selected.");
                    PrepareCategories(product.Product_Category_id);
                    PrepareVendors(product.Product_vendor_name?.Split(',').Select(v => v.Trim()).ToArray());
                    return View(product);
                }

                // Combine selected vendors
                product.Product_vendor_name = (SelectedVendors != null && SelectedVendors.Length > 0)
                    ? string.Join(", ", SelectedVendors)
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

                if (!ModelState.IsValid)
                {
                    PrepareCategories(product.Product_Category_id);
                    PrepareVendors(product.Product_vendor_name?.Split(',').Select(v => v.Trim()).ToArray());
                    ViewBag.Owners = _context.Owners.ToList();
                    return View(product);
                }

                // Maximum limit for each uploaded file = 16 MB
                int maxFileSize = 16 * 1024 * 1024;

                if ((photoFile != null && photoFile.ContentLength > maxFileSize) ||
                    (drawingFile != null && drawingFile.ContentLength > maxFileSize) ||
                    (quotationFile != null && quotationFile.ContentLength > maxFileSize))
                {
                    ModelState.AddModelError("", "Each uploaded file must be less than or equal to 16 MB.");
                    return View(product);
                }

                // Retrieve Category Name Based on Product_Category_id
                var category = _context.Categories.FirstOrDefault(c => c.Category_id == product.Product_Category_id);
                if (category == null)
                {
                    ModelState.AddModelError("Product_Category_id", "Invalid category.");
                    return View(product);
                }

                // Define Storage Folder Based on Category Name
                string categoryFolder = $"~/Files/{category.Category_name}/";
                string serverPath = Server.MapPath(categoryFolder);

                if (!Directory.Exists(serverPath))
                {
                    Directory.CreateDirectory(serverPath);
                }

                // Remove Existing Photo File
                if (removePhoto == true && !string.IsNullOrEmpty(product.Product_photo_filepath))
                {
                    string oldPhotoPath = Server.MapPath(product.Product_photo_filepath);
                    if (System.IO.File.Exists(oldPhotoPath))
                    {
                        System.IO.File.Delete(oldPhotoPath);
                    }
                    product.Product_photo_filepath = null;
                }

                // Remove Existing Drawing File
                if (removeDrawing == true && !string.IsNullOrEmpty(product.Product_drawing_filepath))
                {
                    string oldDrawingPath = Server.MapPath(product.Product_drawing_filepath);
                    if (System.IO.File.Exists(oldDrawingPath))
                    {
                        System.IO.File.Delete(oldDrawingPath);
                    }
                    product.Product_drawing_filepath = null;
                }

                // Remove Existing Quotation File
                if (removeQuotation == true && !string.IsNullOrEmpty(product.Product_quotation_filepath))
                {
                    string oldQuotationPath = Server.MapPath(product.Product_quotation_filepath);
                    if (System.IO.File.Exists(oldQuotationPath))
                    {
                        System.IO.File.Delete(oldQuotationPath);
                    }
                    product.Product_quotation_filepath = null;
                }

                // Upload Photo Files
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

                // Upload Drawing Files
                if (drawingFile != null && drawingFile.ContentLength > 0)
                {
                    string drawingExt = Path.GetExtension(drawingFile.FileName).ToLower();
                    string[] allowedDrawingExt = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

                    if (!allowedDrawingExt.Contains(drawingExt))
                    {
                        ModelState.AddModelError("Product_drawing_filepath", "Drawing format must be JPG, JPEG, PNG, PDF, DOC, DOCX, XLS, or XLSX.");
                        return View(product);
                    }

                    string drawingFileName = Path.GetFileName(drawingFile.FileName);
                    string drawingPath = Path.Combine(categoryFolder, drawingFileName);
                    string serverDrawingPath = Server.MapPath(drawingPath);
                    drawingFile.SaveAs(serverDrawingPath);
                    product.Product_drawing_filepath = drawingPath;
                }

                // Upload Quotation Files
                if (quotationFile != null && quotationFile.ContentLength > 0)
                {
                    string quotationExt = Path.GetExtension(quotationFile.FileName).ToLower();
                    string[] allowedQuotationExt = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

                    if (!allowedQuotationExt.Contains(quotationExt))
                    {
                        ModelState.AddModelError("Product_quotation_filepath", "Quotation format must be JPG, JPEG, PNG, PDF, DOC, DOCX, XLS, or XLSX.");
                        return View(product);
                    }

                    string quotationFileName = Path.GetFileName(quotationFile.FileName);
                    string quotationPath = Path.Combine(categoryFolder, quotationFileName);
                    string serverQuotationPath = Server.MapPath(quotationPath);
                    quotationFile.SaveAs(serverQuotationPath);
                    product.Product_quotation_filepath = quotationPath;
                }

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

                // Save Product to Database
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
            // Konversi ke List<string> untuk menghindari masalah dengan EF
            var selectedVendorList = selectedVendors?.ToList() ?? new List<string>();

            // Ambil semua vendors dari database dulu
            var allVendors = _context.Vendors.ToList();

            // Buat SelectListItem di memory, bukan di query EF
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