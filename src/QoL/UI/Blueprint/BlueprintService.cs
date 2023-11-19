using DoubleQoL.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DoubleQoL.QoL.UI.Blueprint {

    internal static class BlueprintService {

        internal class RequestResult<T> {
            public bool IsSuccess { get; set; }
            public string Error { get; set; }
            public T Data { get; set; }
        }

        internal class LoginData {

            [JsonProperty("token")]
            public string Token { get; set; }

            [JsonProperty("userId")]
            public string UserId { get; set; }
        }

        internal class BlueprintsData {

            [JsonProperty("blueprints")]
            public List<BlueprintData> Blueprints { get; set; }

            [JsonProperty("owners")]
            public Dictionary<string, string> Owners { get; set; }
        }

        internal class BlueprintData {

            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("owner_id")]
            public int Owner_id { get; set; }

            [JsonProperty("download_count")]
            public int Download_count { get; set; }

            [JsonProperty("like_count")]
            public int Like_count { get; set; }

            [JsonProperty("data")]
            public string Data { get; set; }

            public override string ToString() {
                return $"ID: {Id}, Owner ID: {Owner_id}, Download Count: {Download_count}, Like Count: {Like_count}, Data: {Data}";
            }

            public string ToStringData() {
                return $"ID:{Id},OwnerID:{Owner_id},DownloadCount:{Download_count},LikeCount:{Like_count}";
            }
        }

        internal class EmptyData {
        }

        public static async Task<RequestResult<T>> CheckRequestResult<T>(HttpResponseMessage response) {
            try {
                var result = new RequestResult<T>();

                if (response.IsSuccessStatusCode) {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
                    if (jsonResponse.TryGetValue("success", out var successValue) && successValue is bool success) result.IsSuccess = success;
                    if (jsonResponse.TryGetValue("data", out var data)) result.Data = JsonConvert.DeserializeObject<T>(data?.ToString());
                    if (jsonResponse.TryGetValue("error", out var error)) result.Error = error?.ToString();
                }
                else {
                    result.IsSuccess = false;
                    result.Error = await response.Content.ReadAsStringAsync();
                }
                return result;
            }
            catch (Exception ex) {
                Logging.Log.Error($"An unexpected error occurred: {ex.Message}");
                return new RequestResult<T> { IsSuccess = false, Error = string.Empty };
            }
        }

        public static async Task<RequestResult<T>> PostDataRequest<T>(this ServerInfo server, string action, Dictionary<string, string> additionalData = null) {
            if (string.IsNullOrEmpty(server.Token)) await server.Login();
            using (HttpClient httpClient = new HttpClient()) {
                var postData = new Dictionary<string, string>(server.AdditionalData);
                if (additionalData != null) foreach (var kvp in additionalData) postData[kvp.Key] = kvp.Value;
                postData["action"] = action;
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                httpClient.DefaultRequestHeaders.Add("Authorization", server.Token);
                var response = await httpClient.PostAsync(server.Url, new FormUrlEncodedContent(postData));
                return await CheckRequestResult<T>(response);
            }
        }

        public static async Task<RequestResult<BlueprintsData>> GetAllBlueprints(this ServerInfo server) {
            return await server.PostDataRequest<BlueprintsData>("GetAllBlueprints");
        }

        public static async Task<RequestResult<EmptyData>> LikeBlueprint(this ServerInfo server, string blueprintId) {
            return await server.PostDataRequest<EmptyData>("like", new Dictionary<string, string> { { "blueprintId", blueprintId } });
        }

        public static async Task<RequestResult<EmptyData>> DownloadBlueprint(this ServerInfo server, string blueprintId) {
            return await server.PostDataRequest<EmptyData>("download", new Dictionary<string, string> { { "blueprintId", blueprintId } });
        }

        public static async Task<RequestResult<EmptyData>> ModifyBlueprint(this ServerInfo server, string blueprintId, string data) {
            return await server.PostDataRequest<EmptyData>("modify", new Dictionary<string, string> { { "blueprintId", blueprintId }, { "data", data } });
        }

        public static async Task<RequestResult<EmptyData>> DeleteBlueprint(this ServerInfo server, string blueprintId) {
            return await server.PostDataRequest<EmptyData>("delete", new Dictionary<string, string> { { "blueprintId", blueprintId } });
        }

        public static async Task<RequestResult<EmptyData>> UploadBlueprint(this ServerInfo server, string data) {
            return await server.PostDataRequest<EmptyData>("upload", new Dictionary<string, string> { { "data", data } });
        }
    }
}