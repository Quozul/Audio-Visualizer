using System;
using Love;
using NAudio.Wave;

namespace AudioVisualizer
{
    /*
     * Oscilloscope made by saving the WaveBuffer
     * as a class variable
     * Same as oscilloscope but with better precision
     */
    class OscilloscopeImproved : VisualizerWindow
    {
        private WaveBuffer buffer;

        private int Intensity = 2;
        private int Zoom = 8;

        public override void Load()
        {
            WindowTitle = "Audio Osciloscope";
            base.Load();

            // start audio capture
            var capture = new WasapiLoopbackCapture();

            capture.DataAvailable += DataAvailable;

            capture.RecordingStopped += (s, a) =>
            {
                capture.Dispose();
            };

            capture.StartRecording();
        }

        void DataAvailable(object sender, WaveInEventArgs e)
        {
            buffer = new WaveBuffer(e.Buffer); // save the buffer in the class variable
        }

        public override void KeyPressed(KeyConstant key, Scancode scancode, bool isRepeat)
        {
            base.KeyPressed(key, scancode, isRepeat);

            switch (key)
            {
                case KeyConstant.Right:
                    Zoom += 1;
                    break;
                case KeyConstant.Left:
                    Zoom = Math.Max(Zoom - 1, 1);
                    break;
                case KeyConstant.R:
                    Zoom = 8;
                    Intensity = 2;
                    break;
            }
        }

        public override void WheelMoved(int x, int y)
        {
            Intensity = Math.Max(Intensity - y, 1);
        }

        public override void Draw()
        {
            Graphics.SetColor(1, 1, 1);
            if (buffer == null)
            {
                Graphics.Print("No buffer available");
                return;
            }

            Graphics.Print("Controls:\nMouse wheel: Intensity\nLeft/Right arrows: Zoom\nf: Toggle fullscreen\nr: Reset zoom & intensity\nescape: Quit", 0, WindowHeight - 14 * 6);

            int len = buffer.FloatBuffer.Length / Zoom;

            if (Zoom <= 0)
                Graphics.Print("Zoom is invalid");

            float pad = (float)len / WindowWidth; // samples per pixels

            Graphics.Print(
                "Length of buffer: " + buffer.FloatBuffer.Length.ToString() + "\n" +
                "Length: " + len + "\n" +
                "Window width: " + WindowWidth + "\n" +
                "Samples per pixels: " + pad.ToString("N2") + "\n" +
                "Intensity: " + Intensity.ToString() + "\n" +
                "Zoom: " + Zoom.ToString()
            );

            for (int x = 0; x < WindowWidth; x++)
            {
                // current sample
                int i = (int)Math.Round(x * pad);
                float y = buffer.FloatBuffer[i];

                // previous sample
                int x1 = x - 1;
                int i1 = (int)Math.Round((x - 1) * pad);
                float y1 = buffer.FloatBuffer[Math.Max(i1, 0)];

                // render
                Graphics.SetColor(Math.Abs(y), 1f - Math.Abs(y), Math.Abs(y), 1f);
                Graphics.Line(x1, WindowHeight / 2 + y1 * (WindowHeight / (Intensity * 2)), x, WindowHeight / 2 + y * (WindowHeight / (Intensity * 2)));
            }
        }
    }
}
