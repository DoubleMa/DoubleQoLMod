using Mafi.Unity.UiFramework.Components;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DoubleQoL.QoL.UI.Blueprint {

    internal static class ImageLoader {

        public static async void LoadImageFromWeb(this IconContainer iconContainer, string imageUrl, bool preserveAspect = true) {
            Texture2D texture = await LoadImageAsync(imageUrl);
            if (texture == null) return;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            iconContainer.SetIcon(sprite, preserveAspect);
        }

        private static async Task<Texture2D> LoadImageAsync(string imageUrl) {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl)) {
                var asyncOperation = www.SendWebRequest();
                while (!asyncOperation.isDone) await Task.Yield();
                if (www.result == UnityWebRequest.Result.Success) return DownloadHandlerTexture.GetContent(www);
                else Logging.Log.Error($"Failed to load image from web. Error: {www.error}");
                return null;
            }
        }
    }
}