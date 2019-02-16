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
