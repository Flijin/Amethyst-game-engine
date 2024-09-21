using System.Runtime.InteropServices;

namespace Game_Engine;

internal class Program
{
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_HIDE = 0b_0000;
    private const int SW_SHOW = 0b_0101;

    private static void Main()
    {
        ShowWindow(GetConsoleWindow(), SW_HIDE);

        using Window appWindow = new(800, 450, "TestLib");
        appWindow.Run();
    }
}