using DoubleQoL.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DoubleQoL.QoL.UI.Blueprint {

    internal static class BlueprintService {

        public static async Task<HttpResponseMessage> PostDataRequest(this ServerInfo server, string id, string name, string data) {
            using (HttpClient httpClient = new HttpClient()) {
                var postData = new Dictionary<string, string>
                {
                    { "id", id },
                    { "name", name },
                    { "data", data }
                };

                var content = new FormUrlEncodedContent(postData);
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                return await httpClient.PostAsync(server.Url, content);
                //HttpResponseMessage response = await httpClient.PostAsync(server.Url, content);
                //if (response.IsSuccessStatusCode) {
                //    string responseBody = await response.Content.ReadAsStringAsync();
                //    Logging.Log.Info(responseBody);
                //}
                //else Logging.Log.Warning($"Error on post {server.Url}: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        public static async Task<Dictionary<string, BlueprintApiData>> ReadAndParseJsonFromUrlAsync(this ServerInfo server) {
            using (HttpClient client = new HttpClient()) {
                try {
                    HttpResponseMessage response = await client.GetAsync(server.Url);

                    if (response.IsSuccessStatusCode) {
                        string jsonString = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<Dictionary<string, BlueprintApiData>>(jsonString);

                        return data;
                    }
                    else {
                        Logging.Log.Warning($"Failed to retrieve data from {server.Url}. Status code: {response.StatusCode}");
                    }
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
    }

    internal class BlueprintApiData {
        public string Name { get; set; }
        public List<string> Data { get; set; }
    }
}