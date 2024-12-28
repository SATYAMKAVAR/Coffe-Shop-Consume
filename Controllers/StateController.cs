using Coffee_Shop_Management_System.Helper;
using Coffee_Shop_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Coffee_Shop_Management_System.Controllers
{
    public class StateController : Controller
    {
        #region BaseAddress
        Uri baseAddress = new Uri("https://localhost:7214/api");
        private readonly HttpClient _Client;
        public StateController()
        {
            _Client = new HttpClient();
            _Client.BaseAddress = baseAddress;
        }
        #endregion

        #region StateList
        [HttpGet]
        public IActionResult StateList()
        {
            List<StateModel> state = new List<StateModel>();
            HttpResponseMessage response = _Client.GetAsync($"{_Client.BaseAddress}/State").Result;
            if (response.IsSuccessStatusCode)
            {
                String data = response.Content.ReadAsStringAsync().Result;
                dynamic jsonConvert = JsonConvert.DeserializeObject<dynamic>(data);

                var extractedDataJson = JsonConvert.SerializeObject(jsonConvert, Newtonsoft.Json.Formatting.Indented);
                state = JsonConvert.DeserializeObject<List<StateModel>>(extractedDataJson);
            }
            return View("StateList", state);
        }
        #endregion

        #region DeleteState
        [HttpGet]
        public IActionResult DeleteState(string StateID)
        {
            string decryptedStateID = UrlEncryptor.Decrypt(StateID.ToString());

            HttpResponseMessage responseMessage = _Client.DeleteAsync($"{_Client.BaseAddress}/state/{decryptedStateID}").Result;
            if (responseMessage.IsSuccessStatusCode)
            {
                TempData["Message"] = "State deleted successfully.";
            }
            else
            {
                var errorMessage = "State cannot be deleted because it is referenced by other records.";
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction("StateList");
        }
        #endregion

        #region LoadCountryList
        private async Task LoadCountryList()
        {
            var response = await _Client.GetAsync($"{_Client.BaseAddress}/city/countries");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var countries = JsonConvert.DeserializeObject<List<StateCountryDropDownModel>>(data);
                ViewBag.CountryList = new SelectList(countries, "CountryID", "CountryName");
            }
        }
        #endregion

        #region AddEditState
        [HttpGet]
        public async Task<IActionResult> AddEditState(string? StateID)
        {
            int? decryptedStateID = null;
            StateModel state = null;

            if (!string.IsNullOrEmpty(StateID))
            {
                // Decrypt the StateID
                string decryptedStateIDString = UrlEncryptor.Decrypt(StateID);
                decryptedStateID = int.Parse(decryptedStateIDString);

                // Fetch the state data
                HttpResponseMessage response = await _Client.GetAsync($"{_Client.BaseAddress}/State/{decryptedStateID}");
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    // Deserialize into a list of states
                    var states = JsonConvert.DeserializeObject<List<StateModel>>(jsonData);
                    state = states?.FirstOrDefault(); // Get the first state (or null if empty)
                }
                else
                {
                    ModelState.AddModelError("", "Error fetching state data.");
                }
            }

            // If no state data was found, create a new instance
            state ??= new StateModel();

            // Load the country list for the view (assumed method)
            await LoadCountryList();

            return View("AddEditState", state);
        }
        #endregion

        #region SaveState
        [HttpPost]
        public async Task<IActionResult> SaveState(StateModel stateModel)
        {
            try
            {
                // Decrypt the encrypted StateID manually
                string encryptedStateID = Request.Form["StateID"];
                if (!string.IsNullOrEmpty(encryptedStateID))
                {
                    try
                    {
                        string decryptedValue = UrlEncryptor.Decrypt(encryptedStateID);
                        // Ensure the decrypted value is a valid integer
                        if (int.TryParse(decryptedValue, out int stateID))
                        {
                            stateModel.StateID = stateID;  // Assign the decrypted stateID to the model
                        }
                        else
                        {
                            ModelState.AddModelError("StateID", "Invalid State ID format.");
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("StateID", $"Error decrypting State ID: {ex.Message}");
                    }
                }

                if (!ModelState.IsValid)
                {
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            Console.WriteLine(error.ErrorMessage);
                        }
                    }

                    LoadCountryList();
                    return View("AddEditState", stateModel);
                }

                var json = JsonConvert.SerializeObject(stateModel);
                var formData = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (stateModel.StateID == null || stateModel.StateID == 0)
                {
                    // Create new state
                    response = await _Client.PostAsync($"{_Client.BaseAddress}/State", formData);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "State successfully added.";
                        return RedirectToAction("StateList");
                    }
                }
                else
                {
                    // Update existing state
                    response = await _Client.PutAsync($"{_Client.BaseAddress}/State/{stateModel.StateID}", formData);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "State successfully updated.";
                        return RedirectToAction("StateList");
                    }
                }

                TempData["Error"] = $"API Error: {response.ReasonPhrase}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
            }

            return View("AddEditState", stateModel);
        }

        #endregion
    }
}
