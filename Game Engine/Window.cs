
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace Game_Engine
{
    internal class Window : GameWindow
    {
        public Window(int wight, int height, string title) :
            base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                ClientSize = new Vector2i(wight, height),
                Title = title,
                
            })
        {

        }
    }
}
