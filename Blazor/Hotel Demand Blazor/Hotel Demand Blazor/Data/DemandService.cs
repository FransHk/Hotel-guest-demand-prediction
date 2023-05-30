using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Hotel_Demand_Blazor.Models;
using Microsoft.AspNetCore.Server.IIS;

namespace Hotel_Demand_Blazor.Data
{
    // Models used for direct JSON deserialization
    #region DTO MODELS
    public class GuestCount_DTO
    {
        public int[] guest_ct { get; set; }
        public string[] date { get; set; }
    }
    public class Demand_DTO
    {
        public float[] guest_ct { get; set; }
    }
    public class FullData_DTO
    {
        public int[] guest_ct { get; set; }
        public string[] date { get; set; }
        public int[] holiday_flag { get; set; }
        public float[] last_7 { get; set; }
        public float[] last_28 { get; set; }
        public float[] temp { get; set; }
        public int[] weekday { get; set; }
        public int[] weekend_flag { get; set; }
    }
    public class Prediction_DTO
    {
        public float[] guest_ct { get; set; }
        public float[] pred { get; set; }
        public string[] date { get; set; }
    }
    #endregion

    // Regular, non-dto models
    #region NON-DTO MODELS
    public class Prediction
    {
        public int GuestCount { get; set; }
        public int Predicted { get; set; }
        public DateTimeHelper Date { get; set; }
        
    }
    public class FullData
    {
        public int GuestCount { get; set; }
        public string Date { get; set; }
        public int HolidayFlag { get; set; }
        public float Last7 { get; set; }
        public float Last28 { get; set; }
        public float Temp { get; set; }
        public int Weekday { get; set; }
        public int Weekend { get; set; }
    }
    public class GuestCount
    {

        public DateTimeHelper Date { get; set; }
        public int Count { get; set; }
        public int PredictedCount { get; set; }
    }
    #endregion


    public class DemandService
    {
        private readonly HttpClient httpClient;

        public DemandService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Get a specific guest count prediction
        /// for an array of input data (temperature, precipatian, etc)
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public async Task<Demand_DTO> GetPredictionAsync(double[] inputData)
        {
            // Build requist (parameters used to predict)
            var payload = new { data = inputData };
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

            // Make the API call
            var response = await httpClient.PostAsync("http://localhost:5000/predict", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Demand_DTO demand = JsonSerializer.Deserialize<Demand_DTO>(jsonResponse);

            return demand;
        }

        /// <summary>
        /// Returns Guest Count data 
        /// </summary>
        /// <param name="daysBack"></param>
        /// <returns></returns>
        /// <exception cref="Microsoft.AspNetCore.Http.BadHttpRequestException"></exception>
        public async Task<List<GuestCount>> GetGuestDataAsync(int daysBack = 2)
        {
            // Construct the URL with the query parameter
            string url = $"http://localhost:5000/guest-hist?days_back={daysBack}";
            HttpResponseMessage response = await httpClient.GetAsync(url);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as JSON
                string jsonResponse = await response.Content.ReadAsStringAsync();
                GuestCount_DTO guestData = JsonSerializer.Deserialize<GuestCount_DTO>(jsonResponse);

                // Access guestData properties
                int[] guestCount = guestData.guest_ct;
                string[] dates = guestData.date;

                // Code that loops over guestCount and dates array and creates a list of GuestCount objects
                List<GuestCount> guestCountList = new List<GuestCount>();
                for (int i = 0; i < guestCount.Length; i++)
                {
                    GuestCount guest = new GuestCount();
                    DateTimeHelper date = new DateTimeHelper(dates[i]);
                    guest.Count = guestCount[i];
                    guest.Date = date;
                    guestCountList.Add(guest);
                }

                return guestCountList;
            }
            else
            {
                throw new Microsoft.AspNetCore.Http.BadHttpRequestException("Something went wrong..");
            }
        }

        /// <summary>
        /// Get the guest predictions for the last n days in the past,
        /// along with the actual guest counts and dates
        /// </summary>
        /// <param name="daysBack"></param>
        /// <returns></returns>
        /// <exception cref="Microsoft.AspNetCore.Http.BadHttpRequestException"></exception>
        public async Task<List<Prediction>> GetLastPredictions(int daysBack = 14)
        {
            // Construct the URL with the query parameter
            string url = $"http://localhost:5000/get-pred-range?days_back={daysBack}";
            HttpResponseMessage response = await httpClient.GetAsync(url);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as JSON
                string jsonResponse = await response.Content.ReadAsStringAsync();
                Prediction_DTO predData = JsonSerializer.Deserialize<Prediction_DTO>(jsonResponse);

                // Access guestData properties
                float[] preds = predData.pred;
                string[] dates = predData.date;
                float[] counts = predData.guest_ct;

                // Code that loops over guestCount and dates array and creates a list of Prediction objects
                // that holds amount of guests, predicted amount and the corresponding date
                List<Prediction> predictionList = new List<Prediction>();
                for (int i = 0; i < preds.Length; i++)
                {
                    Prediction pred = new Prediction();
                    DateTimeHelper date = new DateTimeHelper(dates[i]);
                    pred.Predicted = (int)preds[i];
                    pred.Date = new DateTimeHelper(dates[i]);
                    pred.GuestCount = (int)counts[i];
                    predictionList.Add(pred);
                }

                return predictionList;
            }
            else
            {
                throw new Microsoft.AspNetCore.Http.BadHttpRequestException("Something went wrong..");
            }
        }

        /// <summary>
        /// Get raw subset of the dataset the model is trained on
        /// to present as dataframe in the aggregate panel
        /// </summary>
        /// <param name="daysBack"></param>
        /// <returns></returns>
        /// <exception cref="Microsoft.AspNetCore.Http.BadHttpRequestException"></exception>
        public async Task<List<FullData>> GetFullDataAsync(int daysBack = 2)
        {
            // Construct the URL with the query parameter
            string url = $"http://localhost:5000/data-full?days_back={daysBack}";
            HttpResponseMessage response = await httpClient.GetAsync(url);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as JSON
                string jsonResponse = await response.Content.ReadAsStringAsync();
                FullData_DTO guestData = JsonSerializer.Deserialize<FullData_DTO>(jsonResponse);

                // Code that loops over guestCount and dates array and creates a list of GuestCount objects
                List<FullData> dataList = new List<FullData>();
                for (int i = 0; i < guestData.guest_ct.Length; i++)
                {
                    FullData guest = new FullData();
                    guest.Date = guestData.date[i];
                    guest.GuestCount = guestData.guest_ct[i];
                    guest.HolidayFlag = guestData.holiday_flag[i];
                    guest.Last7 = guestData.last_7[i];
                    guest.Last28 = guestData.last_28[i];
                    guest.Temp = guestData.temp[i];
                    guest.Weekday = guestData.weekday[i];
                    guest.Weekend = guestData.weekend_flag[i];
                    dataList.Add(guest);
                }
                return dataList;        
            }
            else
            {
                throw new Microsoft.AspNetCore.Http.BadHttpRequestException("Something went wrong..");
            }
        }
    }
}


