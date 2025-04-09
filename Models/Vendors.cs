using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ItemListApp.Models
{
    [Table("Vendors", Schema = "ePAMCatalog")]
    public class Vendors
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Vendor_id { get; set; }
        public string Vendor_name { get; set; }
    }
}