using UnityEngine;

public static class TestCommands
{
    [UnityConsole.ConsoleCommand("hello")]
    public static void PrintHelloWorld () => Debug.Log("Hello World!");

    [UnityConsole.ConsoleCommand]
    public static void Print (string text) => Debug.Log(text);

    [UnityConsole.ConsoleCommand]
    public static void Add (int arg1, int arg2) => Debug.Log(arg1 + arg2);
}
