using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnityConsole
{
    public class ConsoleGUI : MonoBehaviour
    {
        // To prevent garbage when the console is hidden.
        private class OnGUIProxy : MonoBehaviour
        {
            public Action OnGUIDelegate;
            private void OnGUI () => OnGUIDelegate();
        }

        /// <summary>
        /// The key to toggle console visibility.
        /// </summary>
        public static KeyCode ToggleKey { get; set; } = KeyCode.BackQuote;
        /// <summary>
        /// Whether to toggle console when multi-(3 or more) touch is detected.
        /// </summary>
        public static bool ToggleByMultitouch { get; set; } = true;
        /// <summary>
        /// Whether to scale the console UI based on screen resolution.
        /// </summary>
        public static bool ScaleByResolution { get; set; } = true;
        /// <summary>
        /// The font to use throughout the console GUI.
        /// </summary>
        public static Font Font { get; set; }
        /// <summary>
        /// Color of the console underlay.
        /// </summary>
        public static Color BackgroundColor { get; set; } = new Color(0, 0, 0, .65f);
        /// <summary>
        /// Whether to show auto-complete list of available commands when typing in console.
        /// </summary>
        public static bool ShowCompleteList { get; set; } = true;

        internal static Vector2 Size { get; private set; }
        internal static Vector2 Scale { get; private set; }
        internal static GUIStyle Style { get; private set; }

        private const string inputControlName = "input";
        private static ConsoleGUI instance;
        private static readonly Regex regex = new Regex("'(.+?)'|\"(.+?)\"|([^ ]+)", RegexOptions.Compiled);
        private static readonly List<string> inputBuffer = new List<string>();
        private static OnGUIProxy guiProxy;
        private static Rect consoleRect, execButtonRect, closeButtonRect;
        private static bool setFocusPending;
        private static string input;
        private static int inputBufferIndex;

        public static void Initialize (Dictionary<string, MethodInfo> commands = null)
        {
            if (instance) return;

            CommandDatabase.RegisterCommands(commands);

            var hostObject = new GameObject("UnityConsole");
            hostObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(hostObject);

            instance = hostObject.AddComponent<ConsoleGUI>();
            Scale = ScaleByResolution ? new Vector2(Mathf.Max(Screen.width / 1920f, 1), Mathf.Max(Screen.height / 1080f, 1)) : Vector2.one;
            Size = new Vector2(Screen.width - 125 * Scale.x, 25 * Scale.y);
            consoleRect = new Rect(0, 0, Size.x, Size.y);
            execButtonRect = new Rect(Size.x, 0, 75 * Scale.x, Size.y);
            closeButtonRect = new Rect(Screen.width - 050 * Scale.x, 0, 50 * Scale.x, Size.y);
            Style = new GUIStyle {
                richText = true,
                normal = new GUIStyleState { background = Texture2D.whiteTexture, textColor = Color.white },
                contentOffset = new Vector2(5, 5) * Scale,
                fontSize = Mathf.FloorToInt(14 * (Scale.x + Scale.y) / 2),
                font = Font
            };

            guiProxy = hostObject.AddComponent<OnGUIProxy>();
            guiProxy.OnGUIDelegate = instance.DrawGUI;
            guiProxy.enabled = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Destroy ()
        {
            if (!instance) return;
            if (Application.isPlaying) Destroy(instance.gameObject);
            else DestroyImmediate(instance.gameObject);
        }

        public static void Show () => guiProxy.enabled = true;
        public static void Hide () => guiProxy.enabled = false;
        public static void Toggle () => guiProxy.enabled = !guiProxy.enabled;
        private void OnApplicationQuit () => Destroy();

        #if ENABLE_LEGACY_INPUT_MANAGER
        private void Update ()
        {
            if (!Application.isPlaying) return;

            if (Input.GetKeyUp(ToggleKey) || MultitouchDetected())
            {
                Toggle();
                setFocusPending = true;
            }
        }

        private bool MultitouchDetected ()
        {
            if (!ToggleByMultitouch) return false;
            return Input.touchCount > 2 && Input.touches.Any(touch => touch.phase == TouchPhase.Began);
        }
        #endif

        private void DrawGUI ()
        {
            if (Event.current.isKey && Event.current.keyCode == ToggleKey)
            {
                Hide();
                return;
            }

            GUI.backgroundColor = BackgroundColor;
            GUI.SetNextControlName(inputControlName);
            input = GUI.TextField(consoleRect, input, Style);
            if (GUI.Button(execButtonRect, "EXECUTE", Style)) ExecuteInput();
            if (GUI.Button(closeButtonRect, "HIDE", Style)) Hide();
            if (ShowCompleteList) CompleteGUI.Draw(ref input);

            if (setFocusPending)
            {
                GUI.FocusControl(inputControlName);
                setFocusPending = false;
            }

            if (GUI.GetNameOfFocusedControl() == inputControlName) HandleGUIInput();
        }

        private void HandleGUIInput ()
        {
            if (inputBuffer.Count > 0 && Event.current.isKey && Event.current.keyCode == KeyCode.UpArrow)
            {
                inputBufferIndex--;
                if (inputBufferIndex < 0) inputBufferIndex = inputBuffer.Count - 1;
                input = inputBuffer[inputBufferIndex];
            }

            if (inputBuffer.Count > 0 && Event.current.isKey && Event.current.keyCode == KeyCode.DownArrow)
            {
                inputBufferIndex++;
                if (inputBufferIndex >= inputBuffer.Count) inputBufferIndex = 0;
                input = inputBuffer[inputBufferIndex];
            }

            if (Event.current.isKey && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                ExecuteInput();
                inputBuffer.Add(input);
                inputBufferIndex = 0;
                input = string.Empty;
                Hide();
            }
        }

        private static void ExecuteInput ()
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            var preprocessedInput = InputPreprocessor.PreprocessInput(input);
            if (string.IsNullOrWhiteSpace(preprocessedInput)) return;

            var parts = new List<string>();
            foreach (Match match in regex.Matches(preprocessedInput))
                for (int i = 1; i < match.Groups.Count; i++)
                    if (match.Groups[i].Success)
                    {
                        parts.Add(match.Groups[i].Value);
                        break;
                    }

            if (parts.Count == 0) return;
            if (parts.Count == 1) CommandDatabase.ExecuteCommand(parts[0]);
            else CommandDatabase.ExecuteCommand(parts[0], parts.GetRange(1, parts.Count - 1).ToArray());
        }
    }
}
