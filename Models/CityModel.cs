using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Coffee_Shop_Management_System.Models
{
    public class CityModel
    {
        [BindNever]
        public int? CityID { get; set; }
        [Required]
        [DisplayName("City Name")]
        public string CityName { get; set; }
        [Required]
        [DisplayName("Country Name")]
        public int CountryID { get; set; }
        [Required]
        [DisplayName("State Name")]
        public int StateID { get; set; }
        public string? CountryName { get; set; }
        public string? StateName { get; set; }
        [Required]
        [DisplayName("City Code")]
        public string CityCode { get; set; }
    }
    public class CityCountryDropDownModel
    {
        public int CountryID { get; set; }
        public string CountryName { get; set; }
    }
    public class CityStateDropDownModel
    {
        public int StateID { get; set; }
        public string StateName { get; set; }
    }
}
