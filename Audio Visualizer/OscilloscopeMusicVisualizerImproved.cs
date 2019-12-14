using Love;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioVisualizer
{
    class OscilloscopeMusicVisualizerImproved : VisualizerWindow
    {
        private int Zoom = 100;
        private int Resolution = 2048;

        private int BufferLife = 1;

        private BufferedWaveProvider[] stereo = new BufferedWaveProvider[2];
        private List<WaveBuffer[]> audio = new List<WaveBuffer[]>();
        private List<float> BufferTimes = new List<float>();

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

                WaveBuffer[] a = new WaveBuffer[2];

                byte[] buffer1 = new byte[Resolution];
                byte[] buffer2 = new byte[Resolution];

                stereo[0].Read(buffer1, 0, buffer1.Length);
                stereo[1].Read(buffer2, 0, buffer2.Length);

                a[0] = new WaveBuffer(buffer1);
                a[1] = new WaveBuffer(buffer2);

                audio.Add(a);
            }
        }

        public override void Update(float dt)
        {
            audio.ForEach((a) =>
            {
                int index = audio.IndexOf(a);

                Console.WriteLine(BufferTimes[index]);

                BufferTimes[index] += dt;

                if (BufferTimes[index] >= BufferLife)
                {
                    audio.RemoveAt(index);
                    BufferTimes[index] = 0;
                }
            });
        }

        public override void WheelMoved(int x, int y)
        {
            Zoom = Math.Max(Zoom + y, 1);
        }

        public override void Draw()
        {
            Graphics.Print("Zoom: " + Zoom.ToString());

            audio.ForEach((a) =>
            {
                int index = audio.IndexOf(a);
                float color = (index - audio.Count) / audio.Count;

                //Graphics.Print("Index: " + index.ToString() + "\nLife: " + BufferTimes[index].ToString("N2"), 0, (index + 1) * 14);

                for (int i = 0; i < a[0].FloatBuffer.Length / 4; i++)
                {
                    int j = Math.Max(i - 1, 0);
                    Graphics.Line(WindowWidth / 2 + a[0].FloatBuffer[j] * Zoom, WindowHeight / 2 + a[1].FloatBuffer[j] * Zoom, WindowWidth / 2 + a[0].FloatBuffer[i] * Zoom, WindowHeight / 2 + a[1].FloatBuffer[i] * Zoom);
                }
            });
        }
    }
}
