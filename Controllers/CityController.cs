using Coffee_Shop_Management_System.Helper;
using Coffee_Shop_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;


namespace Coffee_Shop_Management_System.Controllers
{
    public class CityController : Controller
    {
        #region BaseAddress
        Uri baseAddress = new Uri("https://localhost:7214/api");
        private readonly HttpClient _Client;
        public CityController()
        {
            _Client = new HttpClient();
            _Client.BaseAddress = baseAddress;
        }
        #endregion

        #region CityList

        [HttpGet]
        public async Task<IActionResult> CityList(int? StateID)
        {
            List<CityModel> cities = new List<CityModel>();
            List<CityModel> newCities = new List<CityModel>();

            try
            {
                // Make the HTTP GET request
                HttpResponseMessage response = await _Client.GetAsync($"{_Client.BaseAddress}/City/");

                if (response.IsSuccessStatusCode)
                {
                    // Read the response content
                    string data = await response.Content.ReadAsStringAsync();

                    // Directly deserialize the JSON array into a List<CityModel>
                    newCities = JsonConvert.DeserializeObject<List<CityModel>>(data);
                    if (StateID.HasValue)
                    {
                        foreach (var item in newCities)
                        {
                            if (item.StateID == StateID)
                            {
                                cities.Add(item);
                            }
                        }
                    }
                    else
                    {
                        cities = newCities;
                    }
                }
                else
                {
                    // Handle unsuccessful responses
                    Console.WriteLine($"API Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Log or handle exceptions
                Console.WriteLine($"Exception occurred: {ex.Message}");
            }
            Console.WriteLine(cities);
            return View(cities);
        }
        #endregion

        #region DeleteCity
        [HttpGet]
        public IActionResult DeleteCity(string CityID)
        {
            string decryptedCityID = UrlEncryptor.Decrypt(CityID.ToString());

            HttpResponseMessage responseMessage = _Client.DeleteAsync($"{_Client.BaseAddress}/City/{decryptedCityID}").Result;
            if (responseMessage.IsSuccessStatusCode)
            {
                TempData["Message"] = "City deleted successfully.";
            }
            else
            {
                var errorMessage = "City cannot be deleted because it is referenced by other records.";
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction("CityList");
        }
        #endregion

        #region LoadCountryList
        private async Task LoadCountryList()
        {
            var response = await _Client.GetAsync($"{_Client.BaseAddress}/city/countries");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var countries = JsonConvert.DeserializeObject<List<CityCountryDropDownModel>>(data);
                ViewBag.CountryList = new SelectList(countries, "CountryID", "CountryName");
            }
        }
        #endregion

        #region GetStatesByCountryID
        [HttpPost]
        public async Task<JsonResult> GetStatesByCountry(int CountryID)
        {
            try
            {
                var states = await GetStatesByCountryID(CountryID);
                return Json(states);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        private async Task<List<CityStateDropDownModel>> GetStatesByCountryID(int CountryID)
        {
            var response = await _Client.GetAsync($"{_Client.BaseAddress}/city/states/{CountryID}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CityStateDropDownModel>>(data);
            }
            return new List<CityStateDropDownModel>();
        }
        #endregion

        #region AddEditCity
        public async Task<IActionResult> AddEditCity(string? CityID)
        {
            int? decryptedCityID = null;

            if (!string.IsNullOrEmpty(CityID))
            {
                string decryptedCityIDString = UrlEncryptor.Decrypt(CityID);
                decryptedCityID = int.Parse(decryptedCityIDString);
            }
            await LoadCountryList();
            if (decryptedCityID.HasValue)
            {
                var response = await _Client.GetAsync($"{_Client.BaseAddress}/city/{decryptedCityID}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var cities = JsonConvert.DeserializeObject<List<CityModel>>(data);
                    var city = cities.FirstOrDefault();

                    if (city != null)
                    {
                        ViewBag.StateList = await GetStatesByCountryID(city.CountryID);
                        return View(city);
                    }
                }
                Console.WriteLine($"Failed to fetch city details. Status Code: {response.StatusCode}");
            }
            // Ensure StateList is set to an empty list when creating a new city
            ViewBag.StateList = new List<SelectListItem>();
            return View(new CityModel());
        }
        #endregion

        #region SaveCity
        [HttpPost]
        public async Task<IActionResult> SaveCity(CityModel cityModel)
        {
            try
            {
                // Decrypt the encrypted CityID manually
                string encryptedCityID = Request.Form["CityID"];
                if (!string.IsNullOrEmpty(encryptedCityID))
                {
                    try
                    {
                        string decryptedValue = UrlEncryptor.Decrypt(encryptedCityID); // Replace with your decryption method
                        cityModel.CityID = int.Parse(decryptedValue); // Convert to integer
                    }
                    catch
                    {
                        ModelState.AddModelError("CityID", "Invalid encrypted City ID.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    await LoadCountryList();
                    return View("AddEditCity", cityModel);
                }

                var json = JsonConvert.SerializeObject(cityModel);
                var formData = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (cityModel.CityID == null || cityModel.CityID == 0)
                {
                    // Create new city
                    response = await _Client.PostAsync($"{_Client.BaseAddress}/City", formData);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "City successfully inserted.";
                        return RedirectToAction("CityList");
                    }
                }
                else
                {
                    // Update existing city
                    response = await _Client.PutAsync($"{_Client.BaseAddress}/City/{cityModel.CityID}", formData);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "City successfully updated.";
                        return RedirectToAction("CityList");
                    }
                }

                TempData["Error"] = $"API Error: {response.ReasonPhrase}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
            }

            await LoadCountryList();
            return View("AddEditCity", cityModel);
        }
        #endregion

    }
}
