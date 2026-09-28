using Pollinations;
using UnityEngine;
using UnityEngine.UI;

namespace Pollinations.Samples
{
    /// <summary>
    /// Minimal demo: wire an InputField, a Button, a Text label, and a RawImage
    /// to this script's public fields (see this sample's README), then press
    /// Play and click the button to generate an image from the typed prompt.
    /// </summary>
    public class PollinationsDemo : MonoBehaviour
    {
        [SerializeField] InputField promptField;
        [SerializeField] Button generateButton;
        [SerializeField] Text statusLabel;
        [SerializeField] RawImage resultImage;
        [SerializeField] string apiKey; // paste a key from https://enter.pollinations.ai/keys, or leave blank and call Login()

        PollinationsClient _client;

        void Start()
        {
            _client = new PollinationsClient(apiKey);
            generateButton.onClick.AddListener(OnGenerateClicked);
        }

        /// Call this instead of setting apiKey directly to let the player sign in
        /// with their own Pollinations account and pay with their own Pollen.
        public void Login()
        {
            statusLabel.text = "Opening browser to sign in...";
            StartCoroutine(PollinationsAuth.Login(
                clientId: null,
                onSuccess: token =>
                {
                    _client = new PollinationsClient(token);
                    statusLabel.text = "Signed in.";
                },
                onError: error => statusLabel.text = $"Login failed: {error}",
                onCodeReady: code => statusLabel.text = $"Enter code {code} in the browser that just opened."));
        }

        void OnGenerateClicked()
        {
            var prompt = promptField.text;
            if (string.IsNullOrWhiteSpace(prompt)) return;

            statusLabel.text = "Generating...";
            generateButton.interactable = false;
            StartCoroutine(_client.GenerateImage(
                prompt,
                onSuccess: texture =>
                {
                    resultImage.texture = texture;
                    statusLabel.text = "Done.";
                    generateButton.interactable = true;
                },
                onError: error =>
                {
                    statusLabel.text = $"Error: {error}";
                    generateButton.interactable = true;
                }));
        }
    }
}
