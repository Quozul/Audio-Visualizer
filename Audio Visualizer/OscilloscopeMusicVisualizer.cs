using Love;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioVisualizer
{
    class OscilloscopeMusicVisualizer : VisualizerWindow
    {
        private int Zoom = 100;
        private int Resolution = 2048;

        private BufferedWaveProvider[] stereo = new BufferedWaveProvider[2];

        public override void Load()
        {
            WindowTitle = "Audio Oscilloscope";
            base.Load();

            // start audio capture
            var capture = new WasapiLoopbackCapture();
            for (int i = 0; i < 2; i++)
            {
                stereo[i] = new BufferedWaveProvider(capture.WaveFormat);
                stereo[i].BufferLength = 2048;
                stereo[i].DiscardOnBufferOverflow = true;
            }

            capture.DataAvailable += DataAvailable;

            capture.RecordingStopped += (s, a) =>
            {
                capture.Dispose();
            };

            capture.StartRecording();
        }

        public void DataAvailable(object sender, WaveInEventArgs e)
        {
            if (e.BytesRecorded != 0)
            {
                int offset = 0;
                while (offset < e.BytesRecorded)
                {
                    for (int n = 0; n < ((WasapiLoopbackCapture)sender).WaveFormat.Channels; n++)
                    {
                        stereo[n].AddSamples(e.Buffer, offset, 4);
                        offset += 4;
                    }
                }
            }
        }

        public override void WheelMoved(int x, int y)
        {
            Zoom = Math.Max(Zoom + y, 1);
        }

        public override void Draw()
        {
            Graphics.Print("Zoom: " + Zoom.ToString());

            byte[] buffer1 = new byte[Resolution];
            stereo[0].Read(buffer1, 0, buffer1.Length);
            float[] left = new WaveBuffer(buffer1).FloatBuffer;

            byte[] buffer2 = new byte[Resolution];
            stereo[1].Read(buffer2, 0, buffer2.Length);
            float[] right = new WaveBuffer(buffer2).FloatBuffer;

            for (int i = 0; i < Resolution / 4; i++)
            {
                int j = Math.Max(i - 1, 0);
                Graphics.Line(WindowWidth / 2 + left[j] * Zoom, WindowHeight / 2 + right[j] * -Zoom, WindowWidth / 2 + left[i] * Zoom, WindowHeight / 2 + right[i] * -Zoom);
            }
        }
    }
}
