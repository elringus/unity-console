using System;
using System.Collections.Generic;
using System.Linq;
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

        private const int height = 25;
        private const string inputControlName = "input";

        private static ConsoleGUI instance;

        private readonly char[] separator = new[] { ' ' };
        private readonly List<string> inputBuffer = new List<string>();
        private OnGUIProxy guiProxy;
        private GUIStyle style;
        private bool setFocusPending;
        private string input;
        private int inputBufferIndex = 0;

        public static void Initialize ()
        {
            if (instance) return;

            CommandDatabase.RegisterCommands();

            var hostObject = new GameObject("UnityConsole");
            hostObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(hostObject);

            instance = hostObject.AddComponent<ConsoleGUI>();
            instance.style = new GUIStyle {
                normal = new GUIStyleState { background = Texture2D.whiteTexture, textColor = Color.white },
                contentOffset = new Vector2(5, 5),
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

            GUI.backgroundColor = new Color(0, 0, 0, .65f);

            GUI.SetNextControlName(inputControlName);
            input = GUI.TextField(new Rect(0, 0, Screen.width - 125, height), input, style);
            if (GUI.Button(new Rect(Screen.width - 125, 0, 75, height), "EXECUTE", style)) ExecuteInput();
            if (GUI.Button(new Rect(Screen.width - 050, 0, 50, height), "HIDE", style)) Hide();

            if (setFocusPending)
            {
                GUI.FocusControl(inputControlName);
                setFocusPending = false;
            }

            if (GUI.GetNameOfFocusedControl() == inputControlName) HandleGUIInput();
        }

        private void HandleGUIInput ()
        {
            if (Event.current.isKey && Event.current.keyCode == KeyCode.UpArrow)
            {
                inputBufferIndex--;
                if (inputBufferIndex < 0) inputBufferIndex = inputBuffer.Count - 1;
                input = inputBuffer[inputBufferIndex];
            }

            if (Event.current.isKey && Event.current.keyCode == KeyCode.DownArrow)
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
            if (command == null || command.Length == 0) return;
            if (command.Length == 1) CommandDatabase.ExecuteCommand(command[0]);
            else CommandDatabase.ExecuteCommand(command[0], command.ToList().GetRange(1, command.Length - 1).ToArray());
        }
    }
}
