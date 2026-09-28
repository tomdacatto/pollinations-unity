using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Pollinations
{
    /// <summary>
    /// Async client for gen.pollinations.ai text, image, and speech generation.
    /// Pass an API key from https://enter.pollinations.ai/keys for development,
    /// or a player-authorized token from <see cref="PollinationsAuth.Login"/> for production.
    /// </summary>
    public class PollinationsClient
    {
        const string BaseUrl = "https://gen.pollinations.ai";
        readonly string _apiKey;

        public PollinationsClient(string apiKey = null) => _apiKey = apiKey;

        [Serializable] class ChatMessage { public string role; public string content; }
        [Serializable] class ChatRequest { public string model; public ChatMessage[] messages; }
        [Serializable] class ChatChoice { public ChatMessage message; }
        [Serializable] class ChatResponse { public ChatChoice[] choices; }

        /// <summary>Generates text from a single prompt via the OpenAI-compatible chat endpoint.</summary>
        public IEnumerator GenerateText(string prompt, Action<string> onSuccess, Action<string> onError, string model = "openai")
        {
            var body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new ChatRequest
            {
                model = model,
                messages = new[] { new ChatMessage { role = "user", content = prompt } },
            }));

            using var req = new UnityWebRequest($"{BaseUrl}/v1/chat/completions", "POST")
            {
                uploadHandler = new UploadHandlerRaw(body),
                downloadHandler = new DownloadHandlerBuffer(),
            };
            ApplyHeaders(req, "application/json");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"{req.responseCode} {req.error}: {req.downloadHandler.text}");
                yield break;
            }

            var response = JsonUtility.FromJson<ChatResponse>(req.downloadHandler.text);
            if (response?.choices == null || response.choices.Length == 0)
            {
                onError?.Invoke("No choices in response.");
                yield break;
            }
            onSuccess?.Invoke(response.choices[0].message.content);
        }

        /// <summary>Generates an image and returns it as a <see cref="Texture2D"/>. See gen.pollinations.ai/image/models for valid model names.</summary>
        public IEnumerator GenerateImage(
            string prompt,
            Action<Texture2D> onSuccess,
            Action<string> onError,
            string model = "flux",
            int width = 1024,
            int height = 1024)
        {
            var url = $"{BaseUrl}/image/{UnityWebRequest.EscapeURL(prompt)}?model={model}&width={width}&height={height}";
            using var req = UnityWebRequestTexture.GetTexture(url);
            ApplyHeaders(req);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"{req.responseCode} {req.error}");
                yield break;
            }
            onSuccess?.Invoke(DownloadHandlerTexture.GetContent(req));
        }

        /// <summary>Generates speech from text and returns it as an mp3 <see cref="AudioClip"/>. See gen.pollinations.ai/audio/models for valid voice names.</summary>
        public IEnumerator GenerateSpeech(string text, Action<AudioClip> onSuccess, Action<string> onError, string voice = "nova")
        {
            var url = $"{BaseUrl}/audio/{UnityWebRequest.EscapeURL(text)}?voice={voice}";
            using var req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
            ApplyHeaders(req);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"{req.responseCode} {req.error}");
                yield break;
            }
            onSuccess?.Invoke(DownloadHandlerAudioClip.GetContent(req));
        }

        /// <summary>Fetches the live model list for a category ("text", "image", or "audio") as raw JSON.</summary>
        public IEnumerator GetModels(string category, Action<string> onSuccess, Action<string> onError)
        {
            using var req = UnityWebRequest.Get($"{BaseUrl}/{category}/models");
            ApplyHeaders(req);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"{req.responseCode} {req.error}");
                yield break;
            }
            onSuccess?.Invoke(req.downloadHandler.text);
        }

        void ApplyHeaders(UnityWebRequest req, string contentType = null)
        {
            if (contentType != null) req.SetRequestHeader("Content-Type", contentType);
            if (!string.IsNullOrEmpty(_apiKey)) req.SetRequestHeader("Authorization", $"Bearer {_apiKey}");
        }
    }
}
