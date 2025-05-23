using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ItemListApp.Models
{
    [Table("Owners", Schema = "ePAMCatalog")]
    public class Owners
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Owner_id { get; set; }
        public string Owner_name { get; set; }
    }
}