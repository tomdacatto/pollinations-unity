# Basic Demo

A minimal scene wiring `PollinationsDemo.cs` to generate an image from typed text.

## Build the scene (about a minute)

1. Create a new empty scene.
2. **UI → Canvas**, then under it add:
   - **UI → Input Field (Legacy)** — the prompt box.
   - **UI → Button (Legacy)** — the generate button.
   - **UI → Text (Legacy)** — a status label.
   - **UI → Raw Image** — where the generated image is shown.
3. Add an empty GameObject named `PollinationsDemo`, and attach `PollinationsDemo.cs` to it.
4. Drag the four UI objects into the matching fields on the `PollinationsDemo` component, and paste an API key from [enter.pollinations.ai/keys](https://enter.pollinations.ai/keys) into the `Api Key` field (or leave it blank and call `Login()` from a second button to use the device flow instead).
5. Press Play, type a prompt, click the button.
