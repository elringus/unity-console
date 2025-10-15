## Description

Allows executing static C# methods via an on-demand console IMGUI.

![](https://github.com/elringus/unity-console/blob/main/preview.gif)

## Installation

Use [UPM](https://docs.unity3d.com/Manual/upm-ui-giturl.html) to install the package via the following Git URL: `https://github.com/elringus/unity-console.git#package`.

![](https://i.gyazo.com/b54e9daa9a483d9bf7f74f0e94b2d38a.gif)

Alternatively, manually copy `Assets/UnityConsole` folder from the repository to the Unity project.

Minimum supported Unity version: 2019.4.

## How to Use

Register commands by adding  `ConsoleCommand` attribute to static C# methods. The attribute has an optional string argument, allowing to assign an alias (shortcut).

```csharp
[UnityConsole.ConsoleCommand("hello")]
public static void PrintHelloWorld () => Debug.Log("Hello World!");

[UnityConsole.ConsoleCommand]
public static void Add (int arg1, int arg2) => Debug.Log(arg1 + arg2);
```

Enable the console at runtime with:

```csharp
UnityConsole.ConsoleGUI.Initialize()
```

Toggle console GUI with `~` key. The key can be overridden via `ConsoleGUI.ToggleKey` public static property. It's also possible to toggle console with a multi-(3 or more) touch on touch screen devices.

In the console, type either method name or alias of a registered command and press `Enter` key to invoke the method. Method arguments are separated with a single whitespace. To specify string arguments with whitespace, wrap them in double or single quotes.

Use `Up` and `Down` to navigate over previously executed commands.

To disable the console at runtime:

```csharp
UnityConsole.ConsoleGUI.Destroy()
```

## Preprocessors

It's possible to inject delegates to modify the console input before it's send for execution, eg:

```csharp
using UnityConsole;
using UnityEngine;

public class TestPreprocessor : MonoBehaviour
{
    private void OnEnable ()
    {
        InputPreprocessor.AddPreprocessor(PreprocessInput);
    }

    private void OnDisable ()
    {
        InputPreprocessor.RemovePreprocessor(PreprocessInput);
    }

    private string PreprocessInput (string input)
    {
        if (input != null && input.StartsWith("@"))
        {
            Debug.Log(input);
            return null;
        }
        return input;
    }
}
```

— will intercept commands starting with `@` and instead log the input. Notice, that when a preprocessor delegate returns `null` it will stop the default console behaviour.

---

<a href="https://naninovel.com">
  <p align="center">The plugin is used in <strong>Naninovel: Visual Novel, Dialogue & Cutscene Storytelling Engine</strong>. Check it out!</p>
  <p align="center"><img alt="naninovel banner" src="https://raw.githubusercontent.com/elringus/cdn/main/naninovel-banner-wide.png"></p>
</a>
