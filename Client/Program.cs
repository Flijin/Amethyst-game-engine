using Amethyst_game_engine;
using Amethyst_game_engine.Core;
using Amethyst_game_engine.Models.GLBModule;
using System.Diagnostics;

namespace Client;

internal class Program
{
    private static void Main()
    {
        SystemSettings.ShowWindow(SystemSettings.SW_SHOW);
        Stopwatch sw = Stopwatch.StartNew();
        GLBImporter importer = new(@"C:\Users\it_ge\Desktop\Loona\Model 2\loona_helluvaboss.glb");
        sw.Stop();

        Console.WriteLine(sw.ElapsedMilliseconds);

        //Window appWindow = new("Тестовый проект") { Scene = new TestScene() };
        //appWindow.Run();
    }
}