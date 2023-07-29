using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityConsole
{
    internal static class CompleteGUI
    {
        private struct Option
        {
            public string Label, MethodName;
        }

        private static readonly List<Option> options = new List<Option>();
        private static string cachedInput;
        private static float maxWidth;

        public static void Draw (ref string input)
        {
            if (string.IsNullOrEmpty(input)) return;
            if (!string.Equals(input, cachedInput, StringComparison.OrdinalIgnoreCase))
                RebuildOptions(input);
            foreach (var option in options)
                DrawOption(option, ref input);
            HandleInput(ref input);
        }

        private static void RebuildOptions (string input)
        {
            options.Clear();
            cachedInput = input;
            maxWidth = 0;
            foreach (var kv in CommandDatabase.Registered)
                if (input.Length == 1 && char.IsWhiteSpace(input[0]) ||
                    kv.Key.StartsWith(ExtractMethodName(input), StringComparison.OrdinalIgnoreCase))
                    options.Add(BuildOption(kv.Key, kv.Value));
        }

        private static string ExtractMethodName (string input)
        {
            var trimmed = input.Trim();
            var firstSpaceIndex = input.IndexOf(' ');
            return firstSpaceIndex > 0 ? trimmed.Substring(0, firstSpaceIndex - 1) : trimmed;
        }

        private static Option BuildOption (string methodName, MethodInfo info)
        {
            var parameters = info.GetParameters().Select(p => $"{p.Name}: {p.ParameterType.Name}");
            var label = $"{methodName} <color=#aaa>({string.Join(", ", parameters)})</color>";
            return new Option { Label = label, MethodName = methodName };
        }

        private static void DrawOption (Option option, ref string input)
        {
            var label = new GUIContent(option.Label);
            var width = ConsoleGUI.Style.CalcSize(label).x + ConsoleGUI.Style.contentOffset.x * 2;
            if (width > maxWidth) maxWidth = width;
            var height = ConsoleGUI.Size.y;
            var y = ConsoleGUI.Size.y + options.IndexOf(option) * height;
            var rect = new Rect(0, y, maxWidth, height);
            if (GUI.Button(rect, label, ConsoleGUI.Style) && !input.StartsWith(option.MethodName, StringComparison.OrdinalIgnoreCase))
                input = option.MethodName;
        }

        private static void HandleInput (ref string input)
        {
            if (!Event.current.isKey) return;
            if (options.Count > 0 && Event.current.keyCode == KeyCode.Tab)
                input = options[0].MethodName;
        }
    }
}
