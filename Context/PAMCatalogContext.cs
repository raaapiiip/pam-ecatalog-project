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
        public DbSet<Users> Users { get; set; }
        public DbSet<Vendors> Vendors { get; set; }
        public DbSet<Owners> Owners { get; set; }
        public DbSet<CategoriesHierarchy> CategoriesHierarchy { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Products>().ToTable("Products", "ePAMCatalog");
            modelBuilder.Entity<Categories>().ToTable("Categories", "ePAMCatalog");
            modelBuilder.Entity<Users>().ToTable("Users", "ePAMCatalog");
            modelBuilder.Entity<Vendors>().ToTable("Vendors", "ePAMCatalog");
            modelBuilder.Entity<Owners>().ToTable("Owners", "ePAMCatalog");
            modelBuilder.Entity<CategoriesHierarchy>().ToTable("CategoriesHierarchy", "ePAMCatalog");

            base.OnModelCreating(modelBuilder);
        }
    }
}