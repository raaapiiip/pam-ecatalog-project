using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ItemListApp.Models
{
    [Table("CategoriesHierarchy", Schema = "ePAMCatalog")]
    public class CategoriesHierarchy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Category_Hierarchy_id { get; set; }
        public string Category_Hierarchy_name { get; set; }
        public int? Parent_id { get; set; }
        [InverseProperty("Hierarchy")]
        public virtual ICollection<Categories> Categories { get; set; }
    }
}