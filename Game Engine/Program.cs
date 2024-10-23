using Game_Engine.Core;
using Game_Engine.Core.Models;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Game_Engine;

internal partial class Program
{
    public const int SW_HIDE = 0b_0000;
    public const int SW_SHOW = 0b_0101;
    public static readonly IntPtr WINDOW_DESCRIPTOR = GetConsoleWindow();

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.SysInt)]
    private static partial IntPtr GetConsoleWindow();

    private static void Main()
    {
        ShowWindow(WINDOW_DESCRIPTOR, SW_SHOW);

        //var test = JSONParser.JSONToObj("{}");

        GLBModel model = new(@"C:\Users\it_ge\Desktop\Loona\Model 2\loona_helluvaboss.glb");
        //using Window appWindow = new(800, 450, "TestLib") { Scene = new TestScene(new Vector2i(800, 450)) };
        //appWindow.Run();
    }
}
