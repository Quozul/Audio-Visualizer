using Love;
using NAudio.Wave;
using System;
using System.Numerics;
using Accord.Math;

namespace AudioVisualizer
{
    /*
     * Visualizer using frequencies
     */
    class FreqVisualizerAccord : VisualizerWindow
    {
        private WaveBuffer buffer;

        private int Size = 2048;

        private int Intensity = 2;

        public override void Load()
        {
            WindowTitle = "Frequency Visualizer";
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

        public void DataAvailable(object sender, WaveInEventArgs e)
        {
            buffer = new WaveBuffer(e.Buffer); // save the buffer in the class variable
        }

        public override void Draw()
        {
            Graphics.SetColor(1, 1, 1);
            if (buffer == null)
            {
                Graphics.Print("No buffer available");
                return;
            }

            int len = buffer.FloatBuffer.Length / 8;
            float pad = (float)len / WindowWidth; // samples per pixels

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

            // fft
            Complex[] values = new Complex[Size];
            for (int i = 0; i < values.Length; i++)
                values[i] = new Complex(buffer.FloatBuffer[i], 0.0);
            FourierTransform.FFT(values, FourierTransform.Direction.Forward);

            for (int i = 0; i < Size; i++)
            {
                float v = (float)(values[i].Magnitude);
                //Graphics.Print(v.ToString(), 0, (i + 1) * 16);
                Graphics.SetColor(Math.Abs(v), 1f - Math.Abs(v), 1f - Math.Abs(v), 1f);
                Graphics.Rectangle(DrawMode.Fill, i * 16, WindowHeight, 16, -v * 10 * WindowHeight - 1);

                /*int j = Math.Max(i - 1, 0);
                float w = (float)(values[j].Magnitude);
                Graphics.Line(j, w * WindowHeight, i, v * WindowHeight);*/
            }
        }
    }
}
