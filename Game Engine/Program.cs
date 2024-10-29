using Game_Engine.Core;
using Game_Engine.Core.Models.GLBModule;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Game_Engine;

internal partial class Program
{
    public const int SW_HIDE = 0b_0000;
    public const int SW_SHOW = 0b_0101;
    public static readonly IntPtr WINDOW_DESCRIPTOR = GetConsoleWindow();
    public static readonly Vector2 SCREEN_RESOLUTION;

    #region Window libs

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.I4)]
    private static partial int GetSystemMetrics([MarshalAs(UnmanagedType.I4)] int nIndex);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow([MarshalAs(UnmanagedType.SysInt)] IntPtr hWnd,
                                           [MarshalAs(UnmanagedType.I4)] int nCmdShow);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.SysInt)]
    private static partial IntPtr GetConsoleWindow();

    #endregion

    #region MacOS lib

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    [return: MarshalAs(UnmanagedType.SysInt)]
    private static partial IntPtr CGMainDisplayID();

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    [return: MarshalAs(UnmanagedType.U4)]
    private static partial uint CGDisplayPixelsWide([MarshalAs(UnmanagedType.SysInt)] IntPtr display);

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    [return: MarshalAs(UnmanagedType.U4)]
    private static partial uint CGDisplayPixelsHigh([MarshalAs(UnmanagedType.SysInt)] IntPtr display);

    #endregion

    #region Linux lib

    [LibraryImport("libX11")]
    [return: MarshalAs(UnmanagedType.SysInt)]
    private static partial IntPtr XOpenDisplay([MarshalAs(UnmanagedType.SysInt)] IntPtr display);

    [LibraryImport("libX11")]
    [return: MarshalAs(UnmanagedType.SysInt)]
    private static partial IntPtr XDefaultRootWindow([MarshalAs(UnmanagedType.SysInt)] IntPtr display);

    [LibraryImport("libX11")]
    [return: MarshalAs(UnmanagedType.I4)]
    private static partial int XGetWindowAttributes([MarshalAs(UnmanagedType.SysInt)] IntPtr display,
                                                    [MarshalAs(UnmanagedType.SysInt)] IntPtr window,
                                                    out XWindowAttributes attributes);

    #endregion

    [StructLayout(LayoutKind.Sequential)]
    private struct XWindowAttributes
    {
        [MarshalAs(UnmanagedType.I4)]
        public int x, y;

        [MarshalAs(UnmanagedType.I4)]
        public int width, height;

        [MarshalAs(UnmanagedType.I4)]
        public int border_width, depth;
    }

    public static bool ShowWindow(int nCmdShow)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return ShowWindow(WINDOW_DESCRIPTOR, nCmdShow);
        else
            return false;
    }

    private static (int Width, int Height) GetWindowsResolution() => (GetSystemMetrics(0), GetSystemMetrics(1));

    private static (int Width, int Height) GetScreenResolution()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GetWindowsResolution();
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return GetMacResolution();
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return GetLinuxResolution();
        else
            throw new PlatformNotSupportedException("Error. Unsupported OS");
    }

    private static (int Width, int Height) GetMacResolution()
    {
        var mainDisplay = CGMainDisplayID();
        var width = CGDisplayPixelsWide(mainDisplay);
        var height = CGDisplayPixelsHigh(mainDisplay);

        return ((int)width, (int)height);
    }

    private static (int Width, int Height) GetLinuxResolution()
    {
        IntPtr display = XOpenDisplay(IntPtr.Zero);
        IntPtr root = XDefaultRootWindow(display);
        XGetWindowAttributes(display, root, out XWindowAttributes attributes);

        return (attributes.width, attributes.height);
    }

    private static void Main()
    {
        ShowWindow(WINDOW_DESCRIPTOR, SW_SHOW);

        Console.WriteLine(GetScreenResolution());

        Stopwatch sw = Stopwatch.StartNew();
        GLBImporter model = new(@"C:\Users\it_ge\Desktop\Loona\Model 2\loona_helluvaboss.glb");
        sw.Stop();

        Console.WriteLine(sw.ElapsedMilliseconds);
        using Window appWindow = new(800, 450, "TestLib") { Scene = new TestScene(new Vector2i(800, 450)) };
        appWindow.Run();
    }
}
