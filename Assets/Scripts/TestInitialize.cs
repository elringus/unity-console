using UnityConsole;
using UnityEngine;

public class TestInitialize : MonoBehaviour
{
    public Font Font;
    public Color BackgroundColor = new Color(0, 0, 0, .65f);

    private void Awake ()
    {
        ConsoleGUI.Font = Font;
        ConsoleGUI.BackgroundColor = BackgroundColor;
        ConsoleGUI.Initialize();
    }
}
