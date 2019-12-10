using System;
using Love;
using NAudio.Wave;

namespace AudioVisualizer
{
    /*
     * Osciloscope made by saving the WaveBuffer
     * as a class variable
     */
    class Osciloscope : VisualizerWindow
    {
        public WaveBuffer buffer;

        public int SIZE = 16;

        public override void Load()
        {
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

        public override void Draw()
        {
            if (buffer == null) return;

            int len = buffer.FloatBuffer.Length / 8;
            int pad = len / WindowWidth; // samples per pixels

            for (int index = 0; index < len; index += pad)
            {
                // current sample
                int x = index / pad;
                float y = buffer.FloatBuffer[index];

                // previous sample
                int x1 = Math.Max(index - 1, 0) / pad;
                float y1 = buffer.FloatBuffer[Math.Max(index - 1, 0)];

                // render
                Graphics.SetColor(Math.Abs(y), 1f - Math.Abs(y), Math.Abs(y), 1f);
                Graphics.Line(x1, WindowHeight / 2 + y1 * (WindowHeight / 2), x, WindowHeight / 2 + y * (WindowHeight / 2));
            }
        }
    }
}
