using Coffee_Shop_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Coffee_Shop_Management_System.Controllers
{
    public class OrderDetailController : Controller
    {

        #region BaseAddress
        Uri baseAddress = new Uri("https://localhost:7214/api");
        private readonly HttpClient _Client;
        public OrderDetailController()
        {
            _Client = new HttpClient();
            _Client.BaseAddress = baseAddress;
        }
        #endregion

        #region OrderDetailList

        [HttpGet]
        public IActionResult OrderDetailList()
        {
            List<OrderDetailModel> orderDetail = new List<OrderDetailModel>();
            HttpResponseMessage response = _Client.GetAsync($"{_Client.BaseAddress}/OrderDetail").Result;
            if (response.IsSuccessStatusCode)
            {
                String data = response.Content.ReadAsStringAsync().Result;
                dynamic jsonConvert = JsonConvert.DeserializeObject<dynamic>(data);

                var extractedDataJson = JsonConvert.SerializeObject(jsonConvert, Newtonsoft.Json.Formatting.Indented);
                orderDetail = JsonConvert.DeserializeObject<List<OrderDetailModel>>(extractedDataJson);
            }
            return View("OrderDetailList", orderDetail);
        }
        #endregion

        #region DeleteOrderDetail
        [HttpGet]
        public IActionResult DeleteOrderDetail(int OrderDetailID)
        {
            HttpResponseMessage responseMessage = _Client.DeleteAsync($"{_Client.BaseAddress}/OrderDetail/{OrderDetailID}").Result;
            if (responseMessage.IsSuccessStatusCode)
            {
                TempData["Message"] = "OrderDetail deleted successfully.";
            }
            else
            {
                var errorMessage = "OrderDetail cannot be deleted because it is referenced by other records.";
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction("OrderDetailList");
        }
        #endregion
    }
}
