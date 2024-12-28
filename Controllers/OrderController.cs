using Coffee_Shop_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;

namespace Coffee_Shop_Management_System.Controllers
{
    public class OrderController : Controller
    {

        #region BaseAddress
        Uri baseAddress = new Uri("https://localhost:7214/api");
        private readonly HttpClient _Client;
        public OrderController()
        {
            _Client = new HttpClient();
            _Client.BaseAddress = baseAddress;
        }
        #endregion

        #region OrderList

        [HttpGet]
        public IActionResult OrderList()
        {
            List<OrderModel> order = new List<OrderModel>();
            HttpResponseMessage response = _Client.GetAsync($"{_Client.BaseAddress}/Order").Result;
            if (response.IsSuccessStatusCode)
            {
                String data = response.Content.ReadAsStringAsync().Result;
                dynamic jsonConvert = JsonConvert.DeserializeObject<dynamic>(data);

                var extractedDataJson = JsonConvert.SerializeObject(jsonConvert, Newtonsoft.Json.Formatting.Indented);
                order = JsonConvert.DeserializeObject<List<OrderModel>>(extractedDataJson);
            }
            return View("OrderList", order);
        }
        #endregion

        #region DeleteOrder
        [HttpGet]
        public IActionResult DeleteOrder(int OrderID)
        {
            HttpResponseMessage responseMessage = _Client.DeleteAsync($"{_Client.BaseAddress}/Order/{OrderID}").Result;
            if (responseMessage.IsSuccessStatusCode)
            {
                TempData["Message"] = "Order deleted successfully.";
            }
            else
            {
                var errorMessage = "Order cannot be deleted because it is referenced by other records.";
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction("OrderList");
        }
        #endregion

        #region LoadUserList
        private async Task LoadUserList()
        {
            var response = await _Client.GetAsync($"{_Client.BaseAddress}/customer/users");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<CustomerUserDropDownModel>>(data);
                ViewBag.UserList = new SelectList(users, "UserID", "UserName");
            }
        }
        #endregion

        #region LoadCustomerList
        private async Task LoadCustomerList()
        {
            var response = await _Client.GetAsync($"{_Client.BaseAddress}/customer");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var customers = JsonConvert.DeserializeObject<List<CustomerUserDropDownModel>>(data);
                ViewBag.CustomerList = new SelectList(customers, "CustomerID", "CustomerName");
            }
        }
        #endregion

        #region AddEditOrder
        [HttpGet]
        public async Task<IActionResult> AddEditOrder(int OrderID)
        {
            await LoadUserList();
            await LoadCustomerList();

            OrderModel order = null;

            HttpResponseMessage response = await _Client.GetAsync($"{_Client.BaseAddress}/Order/{OrderID}");
            if (response.IsSuccessStatusCode)
            {
                string jsonData = await response.Content.ReadAsStringAsync();
                var orders = JsonConvert.DeserializeObject<List<OrderModel>>(jsonData);
                order = orders?.FirstOrDefault();
            }
            else
            {
                ModelState.AddModelError("", "Error fetching order data.");
            }

            order ??= new OrderModel();  // Ensure 'order' is never null

            return View("AddEditOrder", order);  // Pass the model to the view
        }
        #endregion

        #region SaveOrder
        [HttpPost]
        public async Task<IActionResult> SaveOrder(OrderModel orderModel)
        {
            if (!ModelState.IsValid)
            {
                await LoadUserList();
                await LoadCustomerList();
                return View("AddEditOrder", orderModel);
            }

            try
            {
                var json = JsonConvert.SerializeObject(orderModel);
                var formData = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (orderModel.OrderID == null || orderModel.OrderID == 0)
                {
                    // Insert new order
                    response = await _Client.PostAsync($"{_Client.BaseAddress}/Order", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Order successfully added.";
                        return RedirectToAction("OrderList");
                    }
                }
                else
                {
                    // Update existing order
                    response = await _Client.PutAsync($"{_Client.BaseAddress}/Order/{orderModel.OrderID}", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Order successfully updated.";
                        return RedirectToAction("OrderList");
                    }
                }

                TempData["Error"] = $"API Error: {response.ReasonPhrase}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
            }

            await LoadUserList();
            await LoadCustomerList();
            return View("AddEditOrder", orderModel);
        }
        #endregion

    }
}
