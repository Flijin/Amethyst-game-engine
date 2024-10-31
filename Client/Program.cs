using Amethyst_game_engine.Core;

namespace Client;

internal class Program
{
    private static void Main()
    {
        Window appWindow = new("Тестовый проект") { Scene = new TestScene() };
        appWindow.Run();
    }
}