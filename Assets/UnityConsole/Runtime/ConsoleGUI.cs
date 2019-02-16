using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

namespace UnityConsole
{
    public class ConsoleGUI : MonoBehaviour
    {
        /// <summary>
        /// Whether to automatically spawn a hidden persistent gameobject with the console component when application starts.
        /// </summary>
        public static bool AutoInitialize { get; set; } = true;
        /// <summary>
        /// The key to toggle console visibility.
        /// </summary>
        public static KeyCode ToggleKey { get; set; } = KeyCode.BackQuote;
        /// <summary>
        /// Whether to toggle console when multi-(3 or more) touch is detected.
        /// </summary>
        public static bool ToggleByMultitouch { get; set; } = true;

        private const int height = 25;
        private const int buttonWidth = 100;
        private const string inputControlName = "input";
        private static char[] separator = new[] { ' ' };
        private static GUIStyle style;
        private static bool isVisible;
        private static bool setFocusPending;
        private static string input;
        private static List<string> inputBuffer = new List<string> { string.Empty };
        private static int inputBufferIndex = 0;

        public static void Show () => isVisible = true;

        public static void Hide () => isVisible = false;

        public static void Toggle () => isVisible = !isVisible;

        private void Awake ()
        {
            style = new GUIStyle {
                normal = new GUIStyleState { background = Texture2D.whiteTexture, textColor = Color.white },
                contentOffset = new Vector2(5, 5),
            };
        }

        private void Update ()
        {
            if (!isVisible && Application.isPlaying)
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

        private void OnGUI ()
        {
            if (!isVisible) return;

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

        private void OnApplicationQuit () => Hide();

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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize ()
        {
            if (!AutoInitialize) return;

            CommandDatabase.RegisterCommands();

            var hostObject = new GameObject("UnityConsole");
            hostObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(hostObject);
            hostObject.AddComponent<ConsoleGUI>();
        }
    }
}
