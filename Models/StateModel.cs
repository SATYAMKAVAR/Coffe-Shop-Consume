using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Coffee_Shop_Management_System.Models
{
    public class StateModel
    {
        [BindNever]

        public int? StateID { get; set; }
        [Required]
        [DisplayName("State Name")]
        public string StateName { get; set; }
        [Required]
        [DisplayName("Country Name")]
        public int CountryID { get; set; }
        public string? CountryName { get; set; }
        [Required]
        [DisplayName("State Code")]
        public string StateCode { get; set; }
        public int CityCount { get; set; }

    }
    public class StateCountryDropDownModel
    {
        public int CountryID { get; set; }
        public string CountryName { get; set; }
    }
}
