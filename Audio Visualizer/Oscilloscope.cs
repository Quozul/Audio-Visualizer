using System;
using Love;
using NAudio.Wave;

namespace AudioVisualizer
{
    /*
     * Oscilloscope made by saving the WaveBuffer
     * as a class variable
     * Has a better rendering than Oscilloscope3
     */
    class Oscilloscope : VisualizerWindow
    {
        private WaveBuffer buffer;

        private int Intensity = 2;
        private int Zoom = 8;

        public override void Load()
        {
            WindowTitle = "Audio Oscilloscope";
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

            if (key == KeyConstant.Right) Zoom += 1;
            if (key == KeyConstant.Left) Zoom = Math.Max(Zoom - 1, 1);
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

            Graphics.Print("Controls:\nMouse wheel: Intensity\nLeft/Right arrows: Zoom\nf: Toggle fullscreen\nescape: Quit", 0, WindowHeight - 14 * 5);

            int len = buffer.FloatBuffer.Length / Zoom;

            if (len < WindowWidth && Zoom > 0)
            {
                Zoom -= 1;
                Graphics.Print("An error occured, please wait");
                return;
            } else if (Zoom <= 0)
                Graphics.Print("Zoom is invalid");

            int pad = len / WindowWidth; // samples per pixels

            Graphics.Print(
                "Length of buffer: " + buffer.FloatBuffer.Length.ToString() + "\n" +
                "Length: " + len + "\n" +
                "Window width: " + WindowWidth + "\n" +
                "Samples per pixels: " + pad.ToString() + "\n" +
                "Intensity: " + Intensity.ToString() + "\n" +
                "Zoom: " + Zoom.ToString()
            );

            for (int i = 0; i < len; i += pad)
            {
                // current sample
                int x = i;
                float y = buffer.FloatBuffer[i];

                // previous sample
                int x1 = Math.Max(i - pad, 0);
                float y1 = buffer.FloatBuffer[Math.Max(i - pad, 0)];

                // render
                Graphics.SetColor(Math.Abs(y), 1f - Math.Abs(y), Math.Abs(y), 1f);
                Graphics.Line(x1, WindowHeight / 2 + y1 * (WindowHeight / (Intensity * 2)), x, WindowHeight / 2 + y * (WindowHeight / (Intensity * 2)));
            }
        }
    }
}
