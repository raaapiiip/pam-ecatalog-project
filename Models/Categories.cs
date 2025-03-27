using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ItemListApp.Models
{
    [Table("Categories", Schema = "ePAMCatalog")]
    public class Categories
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Category_id { get; set; }
        public string Category_name { get; set; }
        [ForeignKey("Hierarchy")]
        public int? Category_Hierarchy_id { get; set; }
        public virtual ICollection<Products> Products { get; set; }
        public virtual CategoriesHierarchy Hierarchy { get; set; }
    }
}