using DoubleQoL.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DoubleQoL.QoL.UI.Blueprint {

    internal static class BlueprintService {

        public static async Task<RequestResult> CheckRequestResult(HttpResponseMessage response) {
            try {
                var result = new RequestResult();
                if (response.IsSuccessStatusCode) {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);

                    if (jsonResponse.TryGetValue("success", out var successValue) && successValue is bool success) {
                        result.IsSuccess = success;
                        result.Message = jsonResponse.TryGetValue("message", out var messageValue) ? messageValue?.ToString() : string.Empty;
                    }
                }
                else result.Message = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception ex) {
                Logging.Log.Error($"An unexpected error occurred: {ex.Message}");
                return new RequestResult { IsSuccess = false, Message = string.Empty };
            }
        }

        public static async Task<RequestResult> PostDataRequest(this ServerInfo server, string id, string name, string data) {
            using (HttpClient httpClient = new HttpClient()) {
                var postData = new Dictionary<string, string>(server.AdditionalData)
                {
                    { "id", id },
                    { "name", name },
                    { "data", data }
                };
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                return await CheckRequestResult(await httpClient.PostAsync(server.Url, new FormUrlEncodedContent(postData)));
            }
        }

        public static async Task<Dictionary<string, BlueprintApiData>> ReadAndParseJsonFromUrlAsync(this ServerInfo server) {
            using (HttpClient client = new HttpClient()) {
                try {
                    var queryParameters = new FormUrlEncodedContent(server.AdditionalData);
                    var urlWithAdditionalData = $"{server.Url}?{await queryParameters.ReadAsStringAsync()}";
                    HttpResponseMessage response = await client.GetAsync(urlWithAdditionalData);
                    if (response.IsSuccessStatusCode) {
                        string jsonString = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<Dictionary<string, BlueprintApiData>>(jsonString);
                        return data;
                    }
                    else Logging.Log.Warning($"Failed to retrieve data from {urlWithAdditionalData}. Status code: {response.StatusCode}");
                }
                catch (HttpRequestException ex) {
                    Logging.Log.Warning($"HttpRequestException when calling {server.Url}: {ex.Message}");
                }
                catch (JsonException ex) {
                    Logging.Log.Error($"Error parsing JSON from {server.Url}: {ex.Message}");
                }
                catch (Exception ex) {
                    Logging.Log.Error($"An unexpected error occurred: {ex.Message}");
                }

                return null;
            }
        }

        internal class RequestResult {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
        }

        internal class BlueprintApiData {
            public string Name { get; set; }
            public List<string> Data { get; set; }
        }
    }
}