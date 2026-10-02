# Pollinations for Unity

Generate text, images, and speech in your game with the [Pollinations API](https://gen.pollinations.ai) — no backend server required.

Built for [pollinations/pollinations#15580](https://github.com/pollinations/pollinations/issues/15580).

## Install

Unity Package Manager → **Add package from git URL...**:

```
https://github.com/tomdacatto/pollinations-unity.git
```

Or add directly to `Packages/manifest.json`:

```json
"ai.pollinations.unity": "https://github.com/tomdacatto/pollinations-unity.git"
```

Then import the **Basic Demo** sample from the package's **Samples** tab in the Package Manager window to see a working scene wiring (see [Samples~/BasicDemo/README.md](Samples~/BasicDemo/README.md)).

## Usage

```csharp
using Pollinations;
using UnityEngine;

var client = new PollinationsClient(apiKey: "pk_your_key_here"); // https://enter.pollinations.ai/keys

// Text
StartCoroutine(client.GenerateText(
    "Write a one-line greeting from a tavern keeper.",
    onSuccess: reply => Debug.Log(reply),
    onError: error => Debug.LogError(error)));

// Image
StartCoroutine(client.GenerateImage(
    "a cozy pixel-art tavern interior",
    onSuccess: texture => myRawImage.texture = texture,
    onError: error => Debug.LogError(error)));

// Speech
StartCoroutine(client.GenerateSpeech(
    "Welcome, traveler.",
    onSuccess: clip => audioSource.PlayOneShot(clip),
    onError: error => Debug.LogError(error)));

// Live model list, so you never hardcode a stale model name
StartCoroutine(client.GetModels("image", onSuccess: json => Debug.Log(json), onError: Debug.LogError));
```

All three calls are Unity coroutines — run them with `StartCoroutine` from any `MonoBehaviour`.

## Letting players pay with their own Pollen

Don't ship your development API key in a build. Instead, let each player sign in with their own Pollinations account via the [device flow](https://github.com/pollinations/pollinations/blob/main/BRING_YOUR_OWN_POLLEN.md#%EF%B8%8F-clis--headless-apps-device-flow):

```csharp
StartCoroutine(PollinationsAuth.Login(
    clientId: null, // your pk_... key, for attribution only
    onSuccess: token =>
    {
        var client = new PollinationsClient(token);
        PlayerPrefs.SetString("PollinationsToken", token); // persist for next launch
    },
    onError: error => Debug.LogError(error),
    onCodeReady: code => Debug.Log($"Ask the player to enter {code} in the browser that just opened.")));
```

`Login` opens the approval page in the platform's default browser (`Application.OpenURL`) and polls until the player approves. The player-authorized token defaults to expiring after 7 days; call `Login` again when a request returns 401.

## API

| Method | Returns |
|---|---|
| `PollinationsClient(apiKey)` | — |
| `GenerateText(prompt, onSuccess, onError, model = "openai")` | `string` reply |
| `GenerateImage(prompt, onSuccess, onError, model = "flux", width = 1024, height = 1024)` | `Texture2D` |
| `GenerateSpeech(text, onSuccess, onError, voice = "nova")` | `AudioClip` (mp3) |
| `GetModels(category, onSuccess, onError)` | raw JSON `string`, `category` is `"text"`, `"image"`, or `"audio"` |
| `PollinationsAuth.Login(clientId, onSuccess, onError, onCodeReady, timeoutSeconds = 300)` | `string` access token |

## What goes over the wire

Every call is a plain HTTPS request to `https://gen.pollinations.ai` with `Authorization: Bearer <key>` (see [`Runtime/PollinationsClient.cs`](Runtime/PollinationsClient.cs)):

| Method | Request |
|---|---|
| `GenerateText` | `POST /v1/chat/completions` with `{"model": "openai", "messages": [...]}` |
| `GenerateImage` | `GET /image/{prompt}?model=flux&width=1024&height=1024` |
| `GenerateSpeech` | `GET /audio/{text}?voice=nova` |
| `GetModels` | `GET /text/models`, `/image/models`, `/audio/models` |
| `PollinationsAuth.Login` | device flow against `https://enter.pollinations.ai` |

## Requirements

Unity 2021.3 LTS or newer. No third-party dependencies — built entirely on `UnityEngine.Networking`.

## Status

Built and reviewed against the [Pollinations API reference](https://gen.pollinations.ai/docs) and the Unity `UnityWebRequest` manual; not yet run inside the Unity Editor (no Unity install in the build environment). [Issues](https://github.com/tomdacatto/pollinations-unity/issues) and PRs welcome.

## License

MIT
