using Love;
using NAudio.Wave;
using System;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System.Collections.Generic;

namespace AudioVisualizer
{
    enum SmoothType
    {
        horizontal,
        vertical,
        both
    }

    /*
     * Visualizer using frequencies
     */
    class FreqVisualizerMathNet : VisualizerWindow
    {
        private WaveBuffer buffer;

        private static int vertical_smoothness = 4;
        private static int horizontal_smoothness = 3;
        private float size = 10;

        private static SmoothType smoothType = SmoothType.both;

        private List<Complex[]> smooth = new List<Complex[]>();

        private Complex[] values;

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

            int len = buffer.FloatBuffer.Length / 8;

            // fft
            values = new Complex[len];
            for (int i = 0; i < len; i++)
                values[i] = new Complex(buffer.FloatBuffer[i], 0.0);
            Fourier.Forward(values, FourierOptions.Matlab);

            // shift array
            if (smoothType == SmoothType.vertical || smoothType == SmoothType.both)
            {
                smooth.Add(values);
                if (smooth.Count > vertical_smoothness)
                    smooth.RemoveAt(0);
            }
        }

        public override void Draw()
        {
            Graphics.SetColor(1, 1, 1);
            if (buffer == null)
            {
                Graphics.Print("No buffer available");
                return;
            }

            double value = 0;

            if (smoothType == SmoothType.vertical)
            {
                var s = smooth.ToArray();
                // vertical smoothness
                for (int i = 0; i < values.Length; i++)
                {
                    for (int v = 0; v < s.Length; v++)
                        value += Math.Abs(smooth[v] != null ? smooth[v][i].Imaginary : 0.0);
                    value /= s.Length;

                    Graphics.Rectangle(DrawMode.Fill, (i - 1) * size, WindowHeight, size, (float)-value);
                }
            }
            else
            {
                for (int i = 0; i < values.Length; i++)
                {
                    //Graphics.Print(i.ToString() + ": " + values[i].X.ToString("N2") + " i " + (values[i].Y + 0.50f).ToString("N2"), 0, (i + 1) * 16);

                    value = 0;

                    if (smoothType == SmoothType.horizontal)
                    {
                        // horizontal smoothness
                        for (int h = Math.Max(i - horizontal_smoothness, 0); h < Math.Min(i + horizontal_smoothness, values.Length); h++)
                            value += Math.Abs(values[h].Imaginary * ((i - h) / horizontal_smoothness));
                        value /= horizontal_smoothness;
                    }
                    else
                    {
                        var s = smooth.ToArray();

                        for (int h = Math.Max(i - horizontal_smoothness, 0); h < Math.Min(i + horizontal_smoothness, values.Length); h++)
                            for (int v = 0; v < s.Length; v++)
                                value += Math.Abs(s[v] != null ? s[v][h].Imaginary : 0.0);
                        value /= (horizontal_smoothness * s.Length);
                    }

                    Graphics.SetColor(1, 1, 1);
                    Graphics.Rectangle(DrawMode.Fill, (i - 1) * size, WindowHeight, size, (float)-value);
                }
            }
        }
    }
}
