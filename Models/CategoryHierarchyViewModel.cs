using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ItemListApp.Models
{
    public class CategoryHierarchyViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public CategoryHierarchyViewModel ParentCategory { get; set; }
        public List<CategoryViewModel> Categories { get; set; }
        public List<CategoryHierarchyViewModel> Subcategories { get; set; }
    }
}