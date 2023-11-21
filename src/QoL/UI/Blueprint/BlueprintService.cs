using DoubleQoL.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DoubleQoL.QoL.UI.Blueprint {

    internal static class BlueprintService {

        internal class RequestResult<T> {
            public bool IsSuccess { get; set; }
            public string Error { get; set; }
            public T Data { get; set; }

            public override string ToString() {
                return $"IsSuccess: {IsSuccess}, Error: {Error}, Data: {Data}";
            }
        }

        internal class EmptyData {
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

        internal class BlueprintData : IComparable<BlueprintData> {

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

            public int CompareTo(BlueprintData other) {
                if (other == null) return 1;
                if (Id != other.Id) return Id.CompareTo(other.Id);
                if (Owner_id != other.Owner_id) return Owner_id.CompareTo(other.Owner_id);
                if (Like_count != other.Like_count) return Like_count.CompareTo(other.Like_count);
                if (Download_count != other.Download_count) return Download_count.CompareTo(other.Download_count);
                return string.Compare(Data, other.Data, StringComparison.Ordinal);
            }

            public string GetRelevantPartRegx() => Regex.Match(Data, @"^[BF]\d+:")?.Value ?? string.Empty;

            public string GetRelevantPartSplit() => Data?.Split(':')?.FirstOrDefault() ?? string.Empty;

            public override string ToString() {
                return $"BlueprintData{{Id={Id}, Owner_id={Owner_id}, Download_count={Download_count}, Like_count={Like_count}, Data='{GetRelevantPartRegx()}'}}";
            }
        }

        internal class LikeData {

            [JsonProperty("liked")]
            public bool Liked { get; set; }

            [JsonProperty("unliked")]
            public string UnLiked { get; set; }
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

        public static async Task<RequestResult<LikeData>> LikeBlueprint(this ServerInfo server, string blueprintId) {
            return await server.PostDataRequest<LikeData>("like", new Dictionary<string, string> { { "blueprintId", blueprintId } });
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