using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityConsole
{
    /// <summary>
    /// Allows injecting delegates to modify the console input before it's send for execution to the <see cref="CommandDatabase"/>.
    /// </summary>
    public static class InputPreprocessor
    {
        private static readonly HashSet<Func<string, string>> preprocessors = new HashSet<Func<string, string>>();

        /// <summary>
        /// Executes all the added preprocessors in order on the provided input string and returns the resulting string.
        /// In case null is returned from a processor, the input would not be processed further.
        /// </summary>
        public static string PreprocessInput (string input)
        {
            if (input is null) return null;

            var result = input;
            foreach (var preprocessor in preprocessors)
            {
                result = preprocessor.Invoke(result);
                if (result is null) return null;
            }

            return result;
        }

        /// <summary>
        /// Adds the provided delegate as the input preprocessor.
        /// The delegate will be invoked before processing the console input.
        /// The only argument is the console input string. The return is the result of the preprocessing.
        /// When null is returned, the input won't be processed further.
        /// </summary>
        public static bool AddPreprocessor (Func<string, string> preprocessor)
        {
            return preprocessors.Add(preprocessor);
        }

        /// <summary>
        /// Removes the provided preprocessor from the preprocessors list.
        /// </summary>
        public static bool RemovePreprocessor (Func<string, string> preprocessor)
        {
            return preprocessors.Remove(preprocessor);
        }

        /// <summary>
        /// Removes all the added preprocessors from the preprocessors list.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetPreprocessor () => preprocessors.Clear();
    }
}
