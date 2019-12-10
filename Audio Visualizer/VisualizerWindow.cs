using System;
using Love;

namespace AudioVisualizer
{
    /*
     * Just a base window using Love2D to use
     * with the visualizer
     */
    class VisualizerWindow : Scene
    {
        public int WindowWidth;
        public int WindowHeight;

        public string WindowTitle;

        public override void Load()
        {
            WindowSettings mode = Window.GetMode();
            mode.resizable = true;
            Window.SetMode(mode);

            Window.SetTitle(WindowTitle);

            WindowWidth = Graphics.GetWidth();
            WindowHeight = Graphics.GetHeight();
        }

        public override void WindowResize(int w, int h)
        {
            WindowWidth = w;
            WindowHeight = h;
        }

        public override void KeyPressed(KeyConstant key, Scancode scancode, bool isRepeat)
        {
            if (key == KeyConstant.F) Window.SetFullscreen(!Window.GetFullscreen());
            if (key == KeyConstant.Escape) Event.Quit();
        }
    }
}
