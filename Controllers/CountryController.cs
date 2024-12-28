using Coffee_Shop_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using System.Xml;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Coffee_Shop_Management_System.Helper;

namespace Coffee_Shop_Management_System.Controllers
{
    public class CountryController : Controller
    {
        #region BaseAddress
        Uri baseAddress = new Uri("https://localhost:7214/api");
        private readonly HttpClient _Client;
        public CountryController()
        {
            _Client = new HttpClient();
            _Client.BaseAddress = baseAddress;
        }
        #endregion

        #region CountryList
        [HttpGet]
        public IActionResult CountryList()
        {
            List<CountryModel> Country = new List<CountryModel>();
            HttpResponseMessage response = _Client.GetAsync($"{_Client.BaseAddress}/Country").Result;
            if (response.IsSuccessStatusCode)
            {
                String data = response.Content.ReadAsStringAsync().Result;
                dynamic jsonConvert = JsonConvert.DeserializeObject<dynamic>(data);

                var extractedDataJson = JsonConvert.SerializeObject(jsonConvert, Newtonsoft.Json.Formatting.Indented);
                Country = JsonConvert.DeserializeObject<List<CountryModel>>(extractedDataJson);
            }
            return View("CountryList", Country);
        }
        #endregion

        #region DeleteCountry
        [HttpGet]
        public IActionResult DeleteCountry(string CountryID)
        {
            string decryptedCountryID = UrlEncryptor.Decrypt(CountryID.ToString());

            HttpResponseMessage responseMessage = _Client.DeleteAsync($"{_Client.BaseAddress}/Country/{decryptedCountryID}").Result;
            if (responseMessage.IsSuccessStatusCode)
            {
                TempData["Message"] = "Country deleted successfully.";
            }
            else
            {
                var errorMessage = "City cannot be deleted because it is referenced by other records.";
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction("CountryList");
        }
        #endregion

        #region AddEditCountry
        [HttpGet]
        public async Task<IActionResult> AddEditCountry(string? CountryID)
        {
            int? decryptedCityID = null;
            CountryModel country = null;
            if (!string.IsNullOrEmpty(CountryID))
            {
                string decryptedCityIDString = UrlEncryptor.Decrypt(CountryID);
                decryptedCityID = int.Parse(decryptedCityIDString);
                HttpResponseMessage response = await _Client.GetAsync($"{_Client.BaseAddress}/Country/{decryptedCityID}");
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    // Deserialize into a list of countries
                    var countries = JsonConvert.DeserializeObject<List<CountryModel>>(jsonData);
                    country = countries?.FirstOrDefault(); // Get the first country (or null if empty)
                }
                else
                {
                    ModelState.AddModelError("", "Error fetching country data.");
                }
            }
            country ??= new CountryModel();
            return View("AddEditCountry", country);
        }
        #endregion

        #region SaveCountry
        [HttpPost]
        public async Task<IActionResult> SaveCountry(CountryModel countryModel)
        {
            try
            {

                // Decrypt the encrypted CountryID manually
                string encryptedCountryID = Request.Form["CountryID"];
                if (!string.IsNullOrEmpty(encryptedCountryID))
                {
                    try
                    {
                        string decryptedValue = UrlEncryptor.Decrypt(encryptedCountryID); // Replace with your decryption method
                        countryModel.CountryID = int.Parse(decryptedValue); // Convert to integer
                    }
                    catch
                    {
                        ModelState.AddModelError("CountryID", "Invalid encrypted Country ID.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    return View("AddEditCountry", countryModel);
                }

                var json = JsonConvert.SerializeObject(countryModel);
                var formData = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (countryModel.CountryID == null || countryModel.CountryID == 0)
                {
                    // Create new country
                    response = await _Client.PostAsync($"{_Client.BaseAddress}/Country", formData);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Country successfully inserted.";
                        return RedirectToAction("CountryList");
                    }
                }
                else
                {
                    // Update existing country
                    response = await _Client.PutAsync($"{_Client.BaseAddress}/Country/{countryModel.CountryID}", formData);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Country successfully updated.";
                        return RedirectToAction("CountryList");
                    }
                }

                TempData["Error"] = $"API Error: {response.ReasonPhrase}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
            }

            return View("AddEditCountry", countryModel);
        }
        #endregion
    }
}
