using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ItemListApp.Models
{
    [Table("Products", Schema = "ePAMCatalog")]
    public class Products
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Product_id { get; set; }
        [Required(ErrorMessage = "Product Category ID is required.")]
        public int? Product_Category_id { get; set; }
        [Required(ErrorMessage = "Product accessories name is required.")]
        public string Product_accessories_name { get; set; }
        public int? Product_qty { get; set; }
        public string Product_owner { get; set; }
        [NotMapped]
        public List<string> SelectedOwners { get; set; }
        public string Product_dept { get; set; }
        public string Product_drawing_filepath { get; set; }
        public string Product_photo_filepath { get; set; }
        public string Product_eccn { get; set; }
        public int? Product_hs_code { get; set; }
        public string Product_vendor_name { get; set; }
        [NotMapped]
        public List<string> SelectedVendors { get; set; }
        public string Product_quotation_filepath { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime? Product_last_modification { get; set; }
        public string Product_function { get; set; }
        public string Product_remark { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string DeletedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
        [ForeignKey("Product_Category_id")]
        public virtual Categories Category { get; set; }
    }
}