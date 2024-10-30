using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Amethyst_game_engine;

internal static partial class SystemSettings
{
    public const int SW_HIDE = 0b_0000;
    public const int SW_SHOW = 0b_0101;
    public static readonly IntPtr WINDOW_DESCRIPTOR = GetConsoleWindow();
    public static Vector2i SCREEN_RESOLUTION;

    private static bool _wasInitiated = false;

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

    public static bool WasInitiated => _wasInitiated;

    public static void Init()
    {
        var (Width, Height) = GetScreenResolution();
        SCREEN_RESOLUTION = new Vector2i(Width, Height);
        _wasInitiated = true;
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
}
