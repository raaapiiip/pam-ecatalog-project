using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ItemListApp.Models;

namespace ItemListApp.Controllers
{
    public class HomeController : Controller
    {
        private PAMCatalogContext db = new PAMCatalogContext();

        // GET: Home
        public ActionResult Index()
        {
            var allCategories = db.CategoriesHierarchy
                .Include(ch => ch.Categories.Select(c => c.Products))
                .ToList();

            var categoryViewModels = allCategories
                .Select(ch => new CategoryHierarchyViewModel
                {
                    Id = ch.Category_Hierarchy_id,
                    Name = ch.Category_Hierarchy_name,
                    ParentId = ch.Parent_id,
                    Categories = ch.Categories
                        .Select(c => new CategoryViewModel
                        {
                            Id = c.Category_id,
                            Name = c.Category_name,
                            ProductCount = c.Products.Count()
                        }).ToList(),
                    Subcategories = new List<CategoryHierarchyViewModel>()
                }).ToList();

            var categoryDict = categoryViewModels.ToDictionary(c => c.Id);

            foreach (var category in categoryViewModels)
            {
                if (category.ParentId.HasValue && categoryDict.ContainsKey(category.ParentId.Value))
                {
                    var parentCategory = categoryDict[category.ParentId.Value];
                    parentCategory.Subcategories.Add(category);
                    category.ParentCategory = parentCategory;
                }
            }

            var rootCategories = categoryViewModels.Where(c => c.ParentId == null).ToList();

            return View(rootCategories);
        }

        public ActionResult View(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            var product = db.Products
                            .Include(p => p.Category)
                            .FirstOrDefault(p => p.Product_id == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        public ActionResult ProductByCategory(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                return RedirectToAction("Index", "Home");
            }

            var products = db.Products
                .Where(p => p.Category.Category_name == categoryName)
                .ToList();

            ViewBag.CategoryFilter = categoryName;

            return View(products);
        }

        public ActionResult Subcategories(int? categoryId)
        {
            if (!categoryId.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            var selectedCategory = db.CategoriesHierarchy
                .Where(ch => ch.Category_Hierarchy_id == categoryId)
                .Select(ch => new CategoryHierarchyViewModel
                {
                    Id = ch.Category_Hierarchy_id,
                    Name = ch.Category_Hierarchy_name,
                    ParentId = ch.Parent_id
                })
                .FirstOrDefault();

            if (selectedCategory == null)
            {
                return HttpNotFound();
            }

            if (selectedCategory.ParentId.HasValue)
            {
                var parentCategory = db.CategoriesHierarchy
                    .Where(ch => ch.Category_Hierarchy_id == selectedCategory.ParentId.Value)
                    .Select(ch => new CategoryHierarchyViewModel
                    {
                        Id = ch.Category_Hierarchy_id,
                        Name = ch.Category_Hierarchy_name
                    })
                    .FirstOrDefault();

                selectedCategory.ParentCategory = parentCategory;
            }

            selectedCategory.Subcategories = db.CategoriesHierarchy
                .Where(sub => sub.Parent_id == categoryId)
                .Select(sub => new CategoryHierarchyViewModel
                {
                    Id = sub.Category_Hierarchy_id,
                    Name = sub.Category_Hierarchy_name,
                    ParentId = sub.Parent_id
                })
                .ToList();

            selectedCategory.Categories = db.Categories
                .Where(c => c.Category_Hierarchy_id == categoryId)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Category_id,
                    Name = c.Category_name,
                    ProductCount = c.Products.Count()
                })
                .ToList();

            return View(selectedCategory);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}