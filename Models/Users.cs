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
        [Required(ErrorMessage = "Badge ID is required.")]
        [Range(10000000, 99999999, ErrorMessage = "Badge ID must be an 8-digit number.")]
        public int? Badge_id { get; set; }
        [Required(ErrorMessage = "Windows account is required.")]
        public string Windows_account { get; set; }
        public bool IsAdmin { get; set; } = true;
        public bool IsActive { get; set; } = true;
    }
}