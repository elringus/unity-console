using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInitialize : MonoBehaviour
{
    private void Awake ()
    {
        UnityConsole.ConsoleGUI.Initialize();
    }
}
