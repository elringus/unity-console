using UnityEngine;

public static class TestCommands
{
    [UnityConsole.Command("hello")]
    public static void PrintHelloWorld () => Debug.Log("Hello World!");

    [UnityConsole.Command]
    public static void Print (string text) => Debug.Log(text);

    [UnityConsole.Command]
    public static void Add (int arg1, int arg2) => Debug.Log(arg1 + arg2);
}
