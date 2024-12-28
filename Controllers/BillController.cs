using Coffee_Shop_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Coffee_Shop_Management_System.Controllers
{
    public class BillController : Controller
    {

        #region BaseAddress
        Uri baseAddress = new Uri("https://localhost:7214/api");
        private readonly HttpClient _Client;
        public BillController()
        {
            _Client = new HttpClient();
            _Client.BaseAddress = baseAddress;
        }
        #endregion

        #region BillList

        [HttpGet]
        public IActionResult BillList()
        {
            List<BillModel> bill = new List<BillModel>();
            HttpResponseMessage response = _Client.GetAsync($"{_Client.BaseAddress}/bill").Result;
            if (response.IsSuccessStatusCode)
            {
                String data = response.Content.ReadAsStringAsync().Result;
                dynamic jsonConvert = JsonConvert.DeserializeObject<dynamic>(data);

                var extractedDataJson = JsonConvert.SerializeObject(jsonConvert, Newtonsoft.Json.Formatting.Indented);
                bill = JsonConvert.DeserializeObject<List<BillModel>>(extractedDataJson);
            }
            return View("BillList", bill);
        }
        #endregion

        #region DeleteBill
        [HttpGet]
        public IActionResult DeleteBill(int BillID)
        {
            HttpResponseMessage responseMessage = _Client.DeleteAsync($"{_Client.BaseAddress}/bill/{BillID}").Result;
            if (responseMessage.IsSuccessStatusCode)
            {
                TempData["Message"] = "Bill deleted successfully.";
            }
            else
            {
                var errorMessage = "Bill cannot be deleted because it is referenced by other records.";
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction("BillList");
        }
        #endregion

        #region LoadOrderList
        private async Task LoadOrderList()
        {
            var response = await _Client.GetAsync($"{_Client.BaseAddress}/bill/orders");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var orders = JsonConvert.DeserializeObject<List<BillOrderDropDownModel>>(data);
                ViewBag.OrderList = new SelectList(orders, "OrderID", "OrderNumber"); ;
            }
        }
        #endregion
      
        #region LoadUserList
        private  async Task LoadUserList()
        {
            var response = await _Client.GetAsync($"{_Client.BaseAddress}/bill/users");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<BillUserDropDownModel>>(data);
                ViewBag.UserList = new SelectList(users,"UserID","UserName");
            }
            
        }
        #endregion

        #region AddEditBill
        [HttpGet]
        public async Task<IActionResult> AddEditBill(int BillID)
        {
            await LoadUserList();
            await LoadOrderList();
            BillModel bill = null;
            HttpResponseMessage response = await _Client.GetAsync($"{_Client.BaseAddress}/Bill/{BillID}");

            if (response.IsSuccessStatusCode)
            {
                string jsonData = await response.Content.ReadAsStringAsync();
                var bills = JsonConvert.DeserializeObject<List<BillModel>>(jsonData);
                bill = bills?.FirstOrDefault();
            }
            else
            {
                ModelState.AddModelError("", "Error fetching bill data.");
            }

            bill ??= new BillModel();

            return View("AddEditBill", bill);
        }
        #endregion
        
        #region SaveBill
        [HttpPost]
        public async Task<IActionResult> SaveBill(BillModel billModel)
        {
            if (!ModelState.IsValid)
            {
                return View("AddEditBill", billModel);
            }

            try
            {
                var json = JsonConvert.SerializeObject(billModel);
                var formData = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (billModel.BillID == null || billModel.BillID == 0)
                {
                    response = await _Client.PostAsync($"{_Client.BaseAddress}/Bill", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Bill successfully inserted.";
                        return RedirectToAction("BillList");
                    }
                }
                else
                {
                    response = await _Client.PutAsync($"{_Client.BaseAddress}/Bill/{billModel.BillID}", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Bill successfully updated.";
                        return RedirectToAction("BillList");
                    }
                }

                TempData["Error"] = $"API Error: {response.ReasonPhrase}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
            }

            return View("AddEditBill", billModel);
        }
        #endregion
    }
}
