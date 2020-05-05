using Love;
using NAudio.Wave;
using System;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

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

        private static int vertical_smoothness = 50;
        private static int horizontal_smoothness = 5;
        private float size = 10;

        private static SmoothType smoothType = SmoothType.vertical;

        private Complex[][] smooth = new Complex[vertical_smoothness][];

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
                for (int i = 1; i < vertical_smoothness; i++)
                    if (smooth[i-1] != null)
                    {
                        Console.WriteLine(i);
                        smooth[i] = smooth[i - 1];
                    }
                smooth[0] = values;
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

            for (int i = 0; i < values.Length; i++)
            {
                //Graphics.Print(i.ToString() + ": " + values[i].X.ToString("N2") + " i " + (values[i].Y + 0.50f).ToString("N2"), 0, (i + 1) * 16);
                double value = 0;

                if (smoothType == SmoothType.vertical)
                {
                    // vertical smoothness
                    for (int v = 0; v < vertical_smoothness; v++)
                        value += Math.Abs(smooth[v] != null ? smooth[v][i].Imaginary : 0.0);
                    value /= vertical_smoothness;
                }
                else if (smoothType == SmoothType.horizontal)
                {
                    // horizontal smoothness
                    for (int h = Math.Max(i - horizontal_smoothness, 0); h < Math.Min(i + horizontal_smoothness, values.Length); h++)
                        value += Math.Abs(values[h].Imaginary);
                    value /= horizontal_smoothness;
                }
                else
                {
                    for (int h = Math.Max(i - horizontal_smoothness, 0); h < Math.Min(i + horizontal_smoothness, values.Length); h++)
                        for (int v = 0; v < vertical_smoothness; v++)
                            value += Math.Abs(smooth[v] != null ? smooth[v][h].Imaginary : 0.0);
                    value /= (horizontal_smoothness * vertical_smoothness);
                }

                Graphics.Rectangle(DrawMode.Fill, (i - 1) * size, WindowHeight, size, (float)-value);
            }
        }
    }
}
