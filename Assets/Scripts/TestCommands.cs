using System;
using UnityConsole;
using static UnityEngine.Debug;

public static class TestCommands
{
    [ConsoleCommand]
    public static void Print (string text) => Log(text);

    [ConsoleCommand("hello")]
    public static void PrintHello () => Log("Hello!");

    [ConsoleCommand]
    public static void Add (int a, int b) => Log(a + b);

    [ConsoleCommand]
    public static void Power2 (int x) => Log(Math.Pow(x, 2));
}
