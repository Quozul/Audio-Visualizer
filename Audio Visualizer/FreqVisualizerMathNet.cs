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

        private static int vertical_smoothness = 3;
        private static int horizontal_smoothness = 1;
        private float size = 10;

        private int vis_mode = 0;

        private static SmoothType smoothType = SmoothType.both;

        private List<Complex[]> smooth = new List<Complex[]>();

        private Complex[] values;

        private double pre_value = 0;

        private double count = 64;

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
            Fourier.Forward(values, FourierOptions.Default);

            // shift array
            if (smoothType == SmoothType.vertical || smoothType == SmoothType.both)
            {
                smooth.Add(values);
                if (smooth.Count > vertical_smoothness)
                    smooth.RemoveAt(0);
            }
        }
        public override void KeyPressed(KeyConstant key, Scancode scancode, bool isRepeat)
        {
            base.KeyPressed(key, scancode, isRepeat);

            switch (key)
            {
                case KeyConstant.Right:
                    horizontal_smoothness++;
                    break;
                case KeyConstant.Left:
                    if (horizontal_smoothness > 1)
                        horizontal_smoothness--;
                    break;
                case KeyConstant.Down:
                    if (vertical_smoothness > 1)
                    {
                        vertical_smoothness--;
                        for (int i = 0; i < smooth.Count; i++)
                            smooth.RemoveAt(i);
                    }
                    break;
                case KeyConstant.Up:
                    vertical_smoothness++;
                    for (int i = 0; i < smooth.Count; i++)
                        smooth.RemoveAt(i);
                    break;
                case KeyConstant.H:
                    smoothType = SmoothType.horizontal;
                    break;
                case KeyConstant.V:
                    smoothType = SmoothType.vertical;
                    break;
                case KeyConstant.B:
                    smoothType = SmoothType.both;
                    break;
                case KeyConstant.Number1:
                    vis_mode = 0;
                    break;
                case KeyConstant.Number2:
                    vis_mode = 1;
                    break;
                case KeyConstant.Number3:
                    vis_mode = 2;
                    break;
                case KeyConstant.Number4:
                    vis_mode = 3;
                    break;
                case KeyConstant.Number5:
                    vis_mode = 4;
                    break;
                case KeyConstant.Number6:
                    vis_mode = 5;
                    break;
                case KeyConstant.Number7:
                    vis_mode = 6;
                    break;
                case KeyConstant.Number8:
                    vis_mode = 7;
                    break;
                case KeyConstant.Number9:
                    vis_mode = 8;
                    break;
                case KeyConstant.Number0:
                    vis_mode = 9;
                    break;
            }
        }

        public double vSmooth(int i, Complex[][] s)
        {
            double value = 0;

            for (int v = 0; v < s.Length; v++)
                value += Math.Abs(s[v] != null ? s[v][i].Magnitude : 0.0);

            return value / s.Length;
        }

        public double MovingAverage(Complex[] v, int i)
        {
            double value = 0;

            for (int h = Math.Max(i - horizontal_smoothness, 0); h < Math.Min(i + horizontal_smoothness, 64); h++)
                value += v[h].Magnitude;

            return value / ((horizontal_smoothness + 1) * 2);
        }

        public double BothSmooth(int i)
        {
            var s = smooth.ToArray();

            double value = 0;

            for (int h = Math.Max(i - horizontal_smoothness, 0); h < Math.Min(i + horizontal_smoothness, 64); h++)
                value += vSmooth(h, s);

            return value / ((horizontal_smoothness + 1) * 2);
        }

        public double hSmooth(int i)
        {
            if (i > 1) {
                double value = values[i].Magnitude;

                for (int h = i - horizontal_smoothness; h <= i + horizontal_smoothness; h++)
                    value += values[h].Magnitude;

                return value / ((horizontal_smoothness + 1) * 2);
            }

            return 0;
        }

        private void DrawVis(int i, double c, float size, double value)
        {
            float pre_x = 0, pre_y = 0, x = 0, y = 0;

            if (vis_mode == 2 || vis_mode == 3 || vis_mode == 4)
            {
                value *= 100;

                double n = c / (Math.PI * 2);
                double j = (i - 1) / n;
                pre_x = (float)(Math.Cos(j) * pre_value) + WindowWidth / 2;
                pre_y = (float)(Math.Sin(j) * pre_value) + WindowHeight / 2;

                j = i / n;
                x = (float)(Math.Cos(j) * value) + WindowWidth / 2;
                y = (float)(Math.Sin(j) * value) + WindowHeight / 2;
            }
            else if (vis_mode == 6)
            {
                value *= 10;

                double n = c / Math.PI;
                double j = i / n + Math.PI / 2;

                pre_x = (float)(Math.Cos(j) * pre_value) * 10;
                pre_y = (float)(Math.Sin(j) * pre_value) * -10 + WindowHeight / 2;

                j = (i + 1) / n + Math.PI / 2;
                x = (float)(Math.Cos(j) * value) * 10;
                y = (float)(Math.Sin(j) * value) * -10 + WindowHeight / 2;
            }
            else
            {
                value *= WindowHeight / 2;
            }

            value += BothSmooth(i - 1) + BothSmooth(i + 1);
            value /= 3;

            switch (vis_mode)
            {
                case 1:
                    Graphics.Line(i * size - size / 2, (float)(WindowHeight - pre_value), (i + 1) * size - size / 2, (float)(WindowHeight - value));
                    break;
                case 2:
                    Graphics.Circle(DrawMode.Fill, x, y, 1);
                    break;
                case 3:
                    Graphics.Line(pre_x, pre_y, x, y);
                    break;
                case 4:
                    Graphics.SetColor((float)value / 255, (float)value / 255, (float)value / 255);
                    Graphics.Polygon(DrawMode.Fill, pre_x, pre_y, x, y, WindowWidth / 2, WindowHeight / 2);
                    break;
                case 5:
                    Graphics.SetColor(1f - i / 64f, i / 64f, 0, 1);
                    Graphics.Arc(DrawMode.Fill, WindowWidth / 2, WindowHeight / 2, 256, 0, (float)value / 255);
                    break;
                case 6:
                    Graphics.Line(pre_x + WindowWidth / 2, pre_y, x + WindowWidth / 2, y);
                    Graphics.Line(-pre_x + WindowWidth / 2, pre_y, -x + WindowWidth / 2, y);
                    break;
                case 7:
                    for (float l = 0; l < value + size * 0.75f; l += size * 0.75f)
                        Graphics.Rectangle(DrawMode.Fill, i * size, WindowHeight - l, size * 0.95f, size / 2);
                    break;
                case 8:
                    for (float l = 0; l < value; l++)
                    {
                        float u = l / WindowHeight;
                        Graphics.SetColor(u, 1-u, 0);
                        Graphics.Line(i * size, WindowHeight - l, (i + 1) * size, WindowHeight - l);
                    }
                    break;
                default:
                    Graphics.Rectangle(DrawMode.Fill, i * size, WindowHeight, size, (float)-value);
                    break;
            }
            pre_value = value;
        }

        public override void Draw()
        {
            Graphics.SetColor(1, 1, 1);
            if (buffer == null)
            {
                Graphics.Print("No buffer available");
                return;
            }

            Graphics.Print("FPS:" + Timer.GetFPS() + "\n1-8: visualizer mode\nLeft/right arrows: horizontal smoothness strength\n Current: " + horizontal_smoothness + "\nUp/down arrows: vertical smoothness strength\n Current: " + vertical_smoothness, 0, 0);

            size = WindowWidth / 64;

            if (smoothType == SmoothType.vertical)
            {
                var s = smooth.ToArray();
                // vertical smoothness
                for (int i = 0; i < count; i++)
                {
                    double value = 0;
                    for (int v = 0; v < s.Length; v++)
                        value += Math.Abs(s[v] != null ? s[v][i].Magnitude : 0.0);
                    value /= s.Length;

                    DrawVis(i, count, size, value);
                }
            }
            else if (smoothType == SmoothType.horizontal)
            {
                for (int i = 0; i < count; i++)
                {
                    double value = 0;
                    for (int h = Math.Max(i - horizontal_smoothness, 0); h < Math.Min(i + horizontal_smoothness, 64); h++)
                        value += values[h].Magnitude;
                    value /= ((horizontal_smoothness + 1) * 2);

                    DrawVis(i, count, size, value);
                }
            }
            else if (smoothType == SmoothType.both)
            {
                for (int i = 0; i < count; i++)
                {
                    double value = BothSmooth(i);
                    DrawVis(i, count, size, value);
                }
            }
        }
    }
}
