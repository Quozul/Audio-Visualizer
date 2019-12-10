using Love;
using NAudio.Wave;
using System;

namespace AudioVisualizer
{
    /*
     * Visualizer using frequencies
     */
    class FreqVisualizerNAudio : VisualizerWindow
    {
        private WaveBuffer buffer;

        private int M = 6;

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

            // fft
            NAudio.Dsp.Complex[] values = new NAudio.Dsp.Complex[len];
            for (int i = 0; i < len; i++)
            {
                values[i].Y = 0;
                values[i].X = buffer.FloatBuffer[i];
            }
            NAudio.Dsp.FastFourierTransform.FFT(true, M, values);

            float size = (float)WindowWidth / ((float)Math.Pow(2, M) / 2);

            for (int i = 1; i < Math.Pow(2, M) / 2; i++)
            {
                //Graphics.Print(i.ToString() + ": " + values[i].X.ToString("N2") + " i " + (values[i].Y + 0.50f).ToString("N2"), 0, (i + 1) * 16);
                Graphics.Rectangle(DrawMode.Fill, (i - 1) * size, WindowHeight / 2, size, -Math.Abs(values[i].X) * (WindowHeight / 2) * 10);
            }
        }
    }
}
