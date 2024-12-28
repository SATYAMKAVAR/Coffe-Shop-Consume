using System.ComponentModel.DataAnnotations;

namespace Coffee_Shop_Management_System.Models
{
    public class BillModel
    {
        public int? BillID { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        public string BillNumber { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        public DateTime BillDate { get; set; } = DateTime.Now;
        public int? OrderID { get; set; }
        public int OrderNumber { get; set; }
        public double? TotalAmount { get; set; }
        public double? Discount { get; set; }
        public double? NetAmount { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        public int UserID { get; set; }
        public string? UserName { get; set; }


    }
    public class BillUserDropDownModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
    }
    public class BillOrderDropDownModel
    {
        public int OrderID { get; set; }
        public int OrderNumber { get; set; }
    }
}
