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
        private readonly PAMCatalogContext _context;

        public HomeController(PAMCatalogContext context)
        {
            _context = context;
        }

        // GET: Home/Index
        public ActionResult Index()
        {
            var allCategories = _context.CategoriesHierarchy
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
                            ProductCount = c.Products.Count(p => !p.IsDeleted)
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

        // GET: Home/View/Id
        public ActionResult View(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            var product = _context.Products
                            .Include(p => p.Category)
                            .Include(p => p.Vendor)
                            .FirstOrDefault(p => p.Product_id == id && !p.IsDeleted);

            if (product == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(product);
        }

        // GET: Home/ProductByCategory
        public ActionResult ProductByCategory(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                return RedirectToAction("Index", "Home");
            }

            var products = _context.Products
                .Where(p => p.Category.Category_name == categoryName && !p.IsDeleted)
                .ToList();

            ViewBag.CategoryFilter = categoryName;

            return View(products);
        }

        // GET: Home/Subcategories
        public ActionResult Subcategories(int? categoryId)
        {
            if (!categoryId.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            var selectedCategory = _context.CategoriesHierarchy
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
                return RedirectToAction("Index", "Home");
            }

            if (selectedCategory.ParentId.HasValue)
            {
                var parentCategory = _context.CategoriesHierarchy
                    .Where(ch => ch.Category_Hierarchy_id == selectedCategory.ParentId.Value)
                    .Select(ch => new CategoryHierarchyViewModel
                    {
                        Id = ch.Category_Hierarchy_id,
                        Name = ch.Category_Hierarchy_name
                    })
                    .FirstOrDefault();

                selectedCategory.ParentCategory = parentCategory;
            }

            selectedCategory.Subcategories = _context.CategoriesHierarchy
                .Where(sub => sub.Parent_id == categoryId)
                .Select(sub => new CategoryHierarchyViewModel
                {
                    Id = sub.Category_Hierarchy_id,
                    Name = sub.Category_Hierarchy_name,
                    ParentId = sub.Parent_id
                })
                .ToList();

            selectedCategory.Categories = _context.Categories
                .Where(c => c.Category_Hierarchy_id == categoryId)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Category_id,
                    Name = c.Category_name,
                    ProductCount = c.Products.Count(p => !p.IsDeleted)
                })
                .ToList();

            return View(selectedCategory);
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