using Coffee_Shop_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;

namespace Coffee_Shop_Management_System.Controllers
{
    public class ProductController : Controller
    {

        #region BaseAddress
        Uri baseAddress = new Uri("https://localhost:7214/api");
        private readonly HttpClient _Client;
        public ProductController()
        {
            _Client = new HttpClient();
            _Client.BaseAddress = baseAddress;
        }
        #endregion

        #region ProductList

        [HttpGet]
        public IActionResult ProductList()
        {
            List<ProductModel> product = new List<ProductModel>();
            HttpResponseMessage response = _Client.GetAsync($"{_Client.BaseAddress}/Product").Result;
            if (response.IsSuccessStatusCode)
            {
                String data = response.Content.ReadAsStringAsync().Result;
                dynamic jsonConvert = JsonConvert.DeserializeObject<dynamic>(data);

                var extractedDataJson = JsonConvert.SerializeObject(jsonConvert, Newtonsoft.Json.Formatting.Indented);
                product = JsonConvert.DeserializeObject<List<ProductModel>>(extractedDataJson);
            }
            return View("ProductList", product);
        }
        #endregion

        #region DeleteProduct
        [HttpGet]
        public IActionResult DeleteProduct(int ProductID)
        {
            HttpResponseMessage responseMessage = _Client.DeleteAsync($"{_Client.BaseAddress}/Product/{ProductID}").Result;
            if (responseMessage.IsSuccessStatusCode)
            {
                TempData["Message"] = "Product deleted successfully.";
            }
            else
            {
                var errorMessage = "Product cannot be deleted because it is referenced by other records.";
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction("ProductList");
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

        #region AddEditProduct
        [HttpGet]
        public async Task<IActionResult> AddEditProduct(int ProductID)
        {
            await LoadUserList();
            ProductModel product = null;

            HttpResponseMessage response = await _Client.GetAsync($"{_Client.BaseAddress}/Product/{ProductID}");
            if (response.IsSuccessStatusCode)
            {
                string jsonData = await response.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<ProductModel>>(jsonData);
                product = products?.FirstOrDefault();
            }
            else
            {
                ModelState.AddModelError("", "Error fetching product data.");
            }

            product ??= new ProductModel();

            return View("AddEditProduct", product);
        }
        #endregion

        #region SaveProduct
        [HttpPost]
        public async Task<IActionResult> SaveProduct(ProductModel productModel)
        {
            if (!ModelState.IsValid)
            {
                await LoadUserList();
                return View("AddEditProduct", productModel);
            }

            try
            {
                var json = JsonConvert.SerializeObject(productModel);
                var formData = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (productModel.ProductID == null || productModel.ProductID == 0)
                {
                    // Insert new product
                    response = await _Client.PostAsync($"{_Client.BaseAddress}/Product", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Product successfully added.";
                        return RedirectToAction("ProductList");
                    }
                }
                else
                {
                    // Update existing product
                    response = await _Client.PutAsync($"{_Client.BaseAddress}/Product/{productModel.ProductID}", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Product successfully updated.";
                        return RedirectToAction("ProductList");
                    }
                }

                TempData["Error"] = $"API Error: {response.ReasonPhrase}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
            }

            await LoadUserList();
            return View("AddEditProduct", productModel);
        }
        #endregion

    }
}
