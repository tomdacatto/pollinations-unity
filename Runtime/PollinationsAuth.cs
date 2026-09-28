using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Pollinations
{
    /// <summary>
    /// Device-flow login against enter.pollinations.ai, so a player can authorize
    /// the game with their own Pollinations account and pay with their own Pollen.
    /// See https://github.com/pollinations/pollinations/blob/main/BRING_YOUR_OWN_POLLEN.md
    /// </summary>
    public static class PollinationsAuth
    {
        const string AuthBase = "https://enter.pollinations.ai";

        [Serializable] class DeviceCodeRequest { public string client_id; }
        [Serializable] class DeviceCodeResponse { public string device_code; public string user_code; public string verification_uri; }
        [Serializable] class DeviceTokenRequest { public string device_code; }
        [Serializable] class DeviceTokenResponse { public string access_token; public string token_type; public string error; }

        /// <summary>
        /// Starts the device-flow login: requests a code, opens the approval page in
        /// the player's browser, then polls until they approve or <paramref name="timeoutSeconds"/> elapses.
        /// </summary>
        /// <param name="clientId">Your app's publishable key (pk_...), used only for attribution. May be null.</param>
        /// <param name="onCodeReady">
        /// Called with the short approval code as soon as it's known, so you can show it
        /// on screen in case the platform can't reliably auto-open a browser (e.g. consoles).
        /// </param>
        public static IEnumerator Login(
            string clientId,
            Action<string> onSuccess,
            Action<string> onError,
            Action<string> onCodeReady = null,
            float timeoutSeconds = 300f)
        {
            var codeBody = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new DeviceCodeRequest { client_id = clientId ?? "" }));
            using var codeRequest = new UnityWebRequest($"{AuthBase}/api/device/code", "POST")
            {
                uploadHandler = new UploadHandlerRaw(codeBody),
                downloadHandler = new DownloadHandlerBuffer(),
            };
            codeRequest.SetRequestHeader("Content-Type", "application/json");
            yield return codeRequest.SendWebRequest();

            if (codeRequest.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Could not start login: {codeRequest.error}");
                yield break;
            }

            var code = JsonUtility.FromJson<DeviceCodeResponse>(codeRequest.downloadHandler.text);
            onCodeReady?.Invoke(code.user_code);
            Application.OpenURL($"{AuthBase}{code.verification_uri}?user_code={code.user_code}");

            for (float elapsed = 0f; elapsed < timeoutSeconds; elapsed += 5f)
            {
                yield return new WaitForSeconds(5f);

                var tokenBody = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new DeviceTokenRequest { device_code = code.device_code }));
                using var tokenRequest = new UnityWebRequest($"{AuthBase}/api/device/token", "POST")
                {
                    uploadHandler = new UploadHandlerRaw(tokenBody),
                    downloadHandler = new DownloadHandlerBuffer(),
                };
                tokenRequest.SetRequestHeader("Content-Type", "application/json");
                yield return tokenRequest.SendWebRequest();

                var token = JsonUtility.FromJson<DeviceTokenResponse>(tokenRequest.downloadHandler.text);
                if (!string.IsNullOrEmpty(token?.access_token))
                {
                    onSuccess?.Invoke(token.access_token);
                    yield break;
                }
                if (!string.IsNullOrEmpty(token?.error) && token.error != "authorization_pending")
                {
                    onError?.Invoke($"Login failed: {token.error}");
                    yield break;
                }
            }
            onError?.Invoke("Login timed out. Ask the player to try again.");
        }
    }
}
