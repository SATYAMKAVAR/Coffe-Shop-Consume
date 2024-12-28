using Coffee_Shop_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;

namespace Coffee_Shop_Management_System.Controllers
{
    public class CustomerController : Controller
    {
        #region BaseAddress
        Uri baseAddress = new Uri("https://localhost:7214/api");
        private readonly HttpClient _Client;
        public CustomerController()
        {
            _Client = new HttpClient();
            _Client.BaseAddress = baseAddress;
        }
        #endregion

        #region CustomerList

        [HttpGet]
        public IActionResult CustomerList()
        {
            List<CustomerModel> customer = new List<CustomerModel>();
            HttpResponseMessage response = _Client.GetAsync($"{_Client.BaseAddress}/Customer").Result;
            if (response.IsSuccessStatusCode)
            {
                String data = response.Content.ReadAsStringAsync().Result;
                dynamic jsonConvert = JsonConvert.DeserializeObject<dynamic>(data);

                var extractedDataJson = JsonConvert.SerializeObject(jsonConvert, Newtonsoft.Json.Formatting.Indented);
                customer = JsonConvert.DeserializeObject<List<CustomerModel>>(extractedDataJson);
            }
            return View("CustomerList", customer);
        }
        #endregion

        #region DeleteCustomer
        [HttpGet]
        public IActionResult DeleteCustomer(int CustomerID)
        {
            HttpResponseMessage responseMessage = _Client.DeleteAsync($"{_Client.BaseAddress}/Customer/{CustomerID}").Result;
            if (responseMessage.IsSuccessStatusCode)
            {
                TempData["Message"] = "Customer deleted successfully.";
            }
            else
            {
                var errorMessage = "Customer cannot be deleted because it is referenced by other records.";
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction("CustomerList");
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

        #region AddEditCustomer
        [HttpGet]
        public async Task<IActionResult> AddEditCustomer(int CustomerID)
        {
            await LoadUserList();
            CustomerModel customer = null;

            HttpResponseMessage response = await _Client.GetAsync($"{_Client.BaseAddress}/Customer/{CustomerID}");
            if (response.IsSuccessStatusCode)
            {
                string jsonData = await response.Content.ReadAsStringAsync();
                var customers = JsonConvert.DeserializeObject<List<CustomerModel>>(jsonData);
                customer = customers?.FirstOrDefault();
            }
            else
            {
                ModelState.AddModelError("", "Error fetching customer data.");
            }

            customer ??= new CustomerModel();

            return View("AddEditCustomer", customer);
        }
        #endregion

        #region SaveCustomer
        [HttpPost]
        public async Task<IActionResult> SaveCustomer(CustomerModel customerModel)
        {
            if (!ModelState.IsValid)
            {
                await LoadUserList();
                return View("AddEditCustomer", customerModel);
            }

            try
            {
                var json = JsonConvert.SerializeObject(customerModel);
                var formData = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (customerModel.CustomerID == null || customerModel.CustomerID == 0)
                {
                    // Insert new customer
                    response = await _Client.PostAsync($"{_Client.BaseAddress}/Customer", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Customer successfully added.";
                        return RedirectToAction("CustomerList");
                    }
                }
                else
                {
                    // Update existing customer
                    response = await _Client.PutAsync($"{_Client.BaseAddress}/Customer/{customerModel.CustomerID}", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Customer successfully updated.";
                        return RedirectToAction("CustomerList");
                    }
                }

                TempData["Error"] = $"API Error: {response.ReasonPhrase}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
            }

            await LoadUserList();
            return View("AddEditCustomer", customerModel);
        }
        #endregion
    }
}
