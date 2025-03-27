using ItemListApp.Controllers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using static ItemListApp.Models.Categories;
using static ItemListApp.Models.Products;

namespace ItemListApp.Models
{
    public class PAMCatalogContext : DbContext
    {
        public DbSet<Products> Products { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<CategoriesHierarchy> CategoriesHierarchy { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}