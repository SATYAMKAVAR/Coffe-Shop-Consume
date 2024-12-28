using Coffee_Shop_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Coffee_Shop_Management_System.Controllers
{
    public class UserController : Controller
    {
        #region BaseAddress
        Uri baseAddress = new Uri("https://localhost:7214/api");
        private readonly HttpClient _Client;
        public UserController()
        {
            _Client = new HttpClient();
            _Client.BaseAddress = baseAddress;
        }
        #endregion

        #region UserList

        [HttpGet]
        public IActionResult UserList()
        {
            List<UserModel> user = new List<UserModel>();
            HttpResponseMessage response = _Client.GetAsync($"{_Client.BaseAddress}/User").Result;
            if (response.IsSuccessStatusCode)
            {
                String data = response.Content.ReadAsStringAsync().Result;
                dynamic jsonConvert = JsonConvert.DeserializeObject<dynamic>(data);

                var extractedDataJson = JsonConvert.SerializeObject(jsonConvert, Newtonsoft.Json.Formatting.Indented);
                user = JsonConvert.DeserializeObject<List<UserModel>>(extractedDataJson);
            }
            return View("UserList", user);
        }
        #endregion

        #region DeleteUser
        [HttpGet]
        public IActionResult DeleteUser(int UserID)
        {
            HttpResponseMessage responseMessage = _Client.DeleteAsync($"{_Client.BaseAddress}/User/{UserID}").Result;
            if (responseMessage.IsSuccessStatusCode)
            {
                TempData["Message"] = "User deleted successfully.";
            }
            else
            {
                var errorMessage = "User cannot be deleted because it is referenced by other records.";
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction("UserList");
        }
        #endregion

        #region AddEdituser
        [HttpGet]
        public async Task<IActionResult> AddEditUser(int UserID)
        {
            UserModel user = null;
            HttpResponseMessage response = await _Client.GetAsync($"{_Client.BaseAddress}/User/{UserID}");
            
            if (response.IsSuccessStatusCode)
            {
                string jsonData = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<UserModel>>(jsonData);
                user = users?.FirstOrDefault();
            }
            else
            {
                ModelState.AddModelError("", "Error fetching user data.");
            }
            user ??= new UserModel();
            
            return View("AddEditUser", user);
        }

        #endregion

        #region SaveUser
        [HttpPost]
        public async Task<IActionResult> SaveUser(UserModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return View("AddEditUser", userModel);
            }

            try
            {
                var json = JsonConvert.SerializeObject(userModel);
                var formData = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (userModel.UserID == null || userModel.UserID == 0)
                {
                    response = await _Client.PostAsync($"{_Client.BaseAddress}/user", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "User successfully inserted.";
                        return RedirectToAction("UserList");
                    }
                }
                else
                {
                    response = await _Client.PutAsync($"{_Client.BaseAddress}/User/{userModel.UserID}", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "User successfully updated.";
                        return RedirectToAction("UserList");
                    }
                }

                TempData["Error"] = $"API Error: {response.ReasonPhrase}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
            }

            return View("AddEditUser", userModel);
        }
        #endregion
    }
}
