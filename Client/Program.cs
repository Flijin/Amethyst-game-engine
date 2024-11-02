using Amethyst_game_engine;
using Amethyst_game_engine.Core;
using Amethyst_game_engine.Models.GLBModule;
using System.Diagnostics;

namespace Client;

internal class Program
{
    private static void Main()
    {
        float[,] m1 =
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 9 },
        };

        float[,] m2 =
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 9 },
        };

        SystemSettings.ShowWindow(SystemSettings.SW_SHOW);
        Stopwatch sw = Stopwatch.StartNew();
        GLBImporter importer = new(@"C:\Users\it_ge\Desktop\Loona\Model 2\loona_helluvaboss.glb");
        sw.Stop();
        Console.WriteLine("Время выполнения: " + sw.ElapsedMilliseconds + " ms");
        //Console.WriteLine($"Data length: {test.data.Length}");

        Window appWindow = new("Тестовый проект") { Scene = new ExampleScene() };
        appWindow.Run();
    }
}