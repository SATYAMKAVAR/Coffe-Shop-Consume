using System.ComponentModel.DataAnnotations;

namespace Coffee_Shop_Management_System.Models
{
    public class OrderDetailModel
    {
        public int? OrderDetailID { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        public int OrderID { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        public int ProductID { get; set; }
        public string? ProductName { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        public int Quantity { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        public double Amount { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        public double TotalAmount { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        public int UserID { get; set; }
        public string? UserName { get; set; }
    }
    public class OrderDropDownModel
    {
        public int OrderID { get; set; }
        public int OrderNumber { get; set; }
    }
    public class ProductDropDownModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
    }
    public class OrderDetailUserDropDownModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
    }
}
