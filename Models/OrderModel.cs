using System.ComponentModel.DataAnnotations;

namespace Coffee_Shop_Management_System.Models
{
    public class OrderModel
    {
        public int? OrderID { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        public int OrderNumber { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        public DateTime OrderDate { get; set; } = DateTime.Now;
        [Required(ErrorMessage = "This Field is Required")]
        public int CustomerID { get; set; }
        public string? CustomerName { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        public string PaymentMode { get; set; }
        public double? TotalAmount { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        public string ShippingAddress { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        public int UserID { get; set; }
        public string? UserName { get; set; }
    }
    public class OrderUserDropDownModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
    }
    public class CustomerDropDownModel
    {
        public int CustomerID { get; set; } 
        public string CustomerName { get; set; }
    }
}
