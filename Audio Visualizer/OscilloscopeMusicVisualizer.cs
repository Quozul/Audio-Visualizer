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
        private WaveBuffer buffer;

        private int Intensity = 2;
        private int Zoom = 8;

        private WasapiLoopbackCapture capture;
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
            byte[] buffer1 = new byte[1024];
            stereo[0].Read(buffer1, 0, buffer1.Length);
            float[] left = new WaveBuffer(buffer1).FloatBuffer;

            byte[] buffer2 = new byte[1024];
            stereo[1].Read(buffer2, 0, buffer2.Length);
            float[] right = new WaveBuffer(buffer2).FloatBuffer;

            for (int i = 0; i < left.Length / 4; i++)
            {
                int j = Math.Max(i - 1, 0);
                Graphics.Line(WindowWidth / 2 + right[j] * 100, WindowHeight / 2 + left[j] * 100, WindowWidth / 2 + right[i] * 100, WindowHeight / 2 + left[i] * 100);
                //Graphics.Print(right[i].ToString() + " " + left[i].ToString(), 0, i * 16);
            }
        }
    }
}
