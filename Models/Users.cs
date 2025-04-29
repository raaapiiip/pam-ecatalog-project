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
        public int User_id { get; set; }
        [Required(ErrorMessage = "Username is required.")]
        public string User_name { get; set; }
        [Required(ErrorMessage = "Windows account is required.")]
        public string Windows_account { get; set; }
        public bool IsAdmin { get; set; } = true;
        public bool IsActive { get; set; } = true;
    }
}