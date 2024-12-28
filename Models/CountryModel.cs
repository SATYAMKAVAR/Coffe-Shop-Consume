using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Coffee_Shop_Management_System.Models
{
    public class CountryModel
    {
        [BindNever]
        public int? CountryID { get; set; }
        [Required]
        [DisplayName("Country Name")]
        public string CountryName { get; set; }
        [Required]
        [DisplayName("Country Code")]
        public string CountryCode { get; set; }
    }
}
