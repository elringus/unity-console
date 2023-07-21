using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        /// Color of the console underlay.
        /// </summary>
        public static Color BackgroundColor { get; set; } = new Color(0, 0, 0, .65f);

        private const string inputControlName = "input";
        private static ConsoleGUI instance;

        private readonly char[] separator = { ' ' };
        private readonly List<string> inputBuffer = new List<string>();
        private OnGUIProxy guiProxy;
        private GUIStyle style;
        private Vector2 scale;
        private Rect consoleRect, execButtonRect, closeButtonRect;
        private bool setFocusPending;
        private string input;
        private int inputBufferIndex;

        public static void Initialize (Dictionary<string, MethodInfo> commands = null)
        {
            if (instance) return;

            CommandDatabase.RegisterCommands(commands);

            var hostObject = new GameObject("UnityConsole");
            hostObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(hostObject);

            instance = hostObject.AddComponent<ConsoleGUI>();
            instance.scale = ScaleByResolution ? new Vector2(Mathf.Max(Screen.width / 1920f, 1), Mathf.Max(Screen.height / 1080f, 1)) : Vector2.one;
            instance.consoleRect = new Rect(0, 0, Screen.width - 125 * instance.scale.x, 25 * instance.scale.y);
            instance.execButtonRect = new Rect(Screen.width - 125 * instance.scale.x, 0, 75 * instance.scale.x, 25 * instance.scale.y);
            instance.closeButtonRect = new Rect(Screen.width - 050 * instance.scale.x, 0, 50 * instance.scale.x, 25 * instance.scale.y);
            instance.style = new GUIStyle {
                normal = new GUIStyleState { background = Texture2D.whiteTexture, textColor = Color.white },
                contentOffset = new Vector2(5, 5) * instance.scale,
                fontSize = Mathf.FloorToInt(14 * (instance.scale.x + instance.scale.y) / 2)
            };

            instance.guiProxy = hostObject.AddComponent<OnGUIProxy>();
            instance.guiProxy.OnGUIDelegate = instance.DrawGUI;
            instance.guiProxy.enabled = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Destroy ()
        {
            if (!instance) return;

            if (Application.isPlaying) Destroy(instance.gameObject);
            else DestroyImmediate(instance.gameObject);
        }

        public static void Show () => instance.guiProxy.enabled = true;

        public static void Hide () => instance.guiProxy.enabled = false;

        public static void Toggle () => instance.guiProxy.enabled = !instance.guiProxy.enabled;

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
            input = GUI.TextField(consoleRect, input, style);
            if (GUI.Button(execButtonRect, "EXECUTE", style)) ExecuteInput();
            if (GUI.Button(closeButtonRect, "HIDE", style)) Hide();

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

        private void ExecuteInput ()
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            var preprocessedInput = InputPreprocessor.PreprocessInput(input);
            if (string.IsNullOrWhiteSpace(preprocessedInput)) return;

            var command = preprocessedInput.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (command.Length == 0) return;
            if (command.Length == 1) CommandDatabase.ExecuteCommand(command[0]);
            else CommandDatabase.ExecuteCommand(command[0], command.ToList().GetRange(1, command.Length - 1).ToArray());
        }
    }
}
