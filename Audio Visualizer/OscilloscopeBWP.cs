using System;
using Love;
using NAudio.Wave;

namespace AudioVisualizer
{
    /* 
     * Oscilloscope using a BufferedWaveProvider
     */
    class OscilloscopeBWP : Scene
    {
        public BufferedWaveProvider bwp;
        private int BUFFERSIZE = 4800;

        public int SIZE = 16;

        public override void Load()
        {
            WindowSettings mode = Window.GetMode();
            mode.resizable = true;
            Window.SetMode(mode);

            // audio stuff
            var capture = new WasapiLoopbackCapture();

            bwp = new BufferedWaveProvider(capture.WaveFormat);
            bwp.BufferLength = BUFFERSIZE * 2;

            bwp.DiscardOnBufferOverflow = true;

            capture.DataAvailable += DataAvailable;

            capture.RecordingStopped += (s, a) =>
            {
                capture.Dispose();
            };

            capture.StartRecording();
        }

        void DataAvailable(object sender, WaveInEventArgs e)
        {
            bwp.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }

        public override void Draw()
        {
            byte[] b = new byte[BUFFERSIZE];
            bwp.Read(b, 0, BUFFERSIZE);

            WaveBuffer buffer = new WaveBuffer(b);

            int width = Graphics.GetWidth() * 2;
            int height = Graphics.GetHeight();
            int len = BUFFERSIZE;
            int pad = len / width;

            for (int index = 0; index < len; index += pad)
            {
                int x = index / pad;
                float y = buffer.FloatBuffer[index];

                int x1 = Math.Max(index - 1, 0) / pad;
                float y1 = buffer.FloatBuffer[Math.Max(index - 1, 0)];

                Graphics.SetColor(Math.Abs(y), 1f - Math.Abs(y), Math.Abs(y), 1f);
                Graphics.Line(x1, height / 2 + y1 * (height / 2), x, height / 2 + y * (height / 2));
            }
        }
    }
}
