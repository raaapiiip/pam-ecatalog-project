using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ItemListApp.Models
{
    [Table("Users", Schema = "ePAMCatalog")]
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BadgeId { get; set; }
        public string Windows_Account { get; set; }
        public bool Admin { get; set; }
    }
}