using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace URandomGen.Tests
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ConsoleKey key;

            do
            {
                const int _imgSize = 640;
                const int _maxVals = _imgSize * _imgSize / 32;

                Console.WriteLine("Each test involves the generation of " + _maxVals + " random numbers; however, not all values will be used for all tests.");
                Console.WriteLine("1. RandomCMWC");
                Console.WriteLine("2. RandomMersenne");
                Console.WriteLine("3. RandomXorShift");
                Console.WriteLine("4. System.Random, for comparison");
                Console.WriteLine("X. Exit");

                key = ReadKey();

                Random generator = null;

                switch (key)
                {
                    case ConsoleKey.D1:
                        generator = new RandomCMWC();
                        break;
                    case ConsoleKey.D2:
                        generator = new RandomMersenne();
                        break;
                    case ConsoleKey.D3:
                        generator = new RandomXorshift();
                        break;
                    case ConsoleKey.D4:
                        generator = new Random();
                        break;
                }

                if (generator != null)
                {
                    Console.Write("Generating ... ");

                    uint[] results = new uint[_maxVals];

                    const ulong genMax = uint.MaxValue + 1UL;

                    uint min = uint.MaxValue, max = 0;

                    for (int i = 0; i < _maxVals; i++)
                    {
                        uint curVal = results[i] = (uint)RandomGen.NextUInt64(generator, genMax);
                        min = Math.Min(curVal, min);
                        max = Math.Max(curVal, max);
                    }

                    double avg = results.Average(i => i);

                    Console.WriteLine("done.");

                    const uint baseMedian = uint.MaxValue / 2;

                    Bitmap bmpBitmap = null, bmpGraphs = null, bmpGraphsBig = null;

                    string pathBitmap = generator.GetType().Name + ".Bitmap.png";
                    string pathGraphs = generator.GetType().Name + ".Graphs.png";
                    string pathGraphsBig = generator.GetType().Name + ".GraphsBig.png";

                    uint maxDelta = uint.MaxValue - max;
                    double avgDelta = Math.Abs(baseMedian - avg);

                    do
                    {
                        Console.WriteLine(generator.GetType().Name);
                        Console.WriteLine("Expected maximum: {0:10}", uint.MaxValue);
                        Console.WriteLine("Actual maximum:   {0:10} (delta: {1}, {2}%)", max, maxDelta, (100f * maxDelta) / uint.MaxValue);
                        Console.WriteLine("Expected minimum: 0");
                        Console.WriteLine("Actual minimum:   {0:10} (delta: {1}, {2}%)", min, min, (100f * min) / uint.MaxValue);
                        Console.WriteLine("Expected average: {0:10}", baseMedian);
                        Console.WriteLine("Actual average:   {0:10} (delta: {1}, {2}%)", avg, avgDelta, (100 * avgDelta) / baseMedian);
                        Console.WriteLine();

                        Console.WriteLine("1. Bitmap (saved in working dir as {0})", pathBitmap);
                        Console.WriteLine("2. Graphs, Scatterplot, and Histograms for first 1000 (saved in working dir as {0})", pathGraphs);
                        Console.WriteLine("3. Graphs, Scatterplot, and Histograms for All " + _maxVals + " (saved in working dir as {0})", pathGraphsBig);
                        Console.WriteLine("X. Cancel");
                        key = ReadKey();

                        switch (key)
                        {
                            case ConsoleKey.D1:
                                if (bmpBitmap == null || !File.Exists(pathBitmap))
                                {
                                    if (bmpBitmap == null)
                                    {
                                        const PixelFormat pixelFormat = PixelFormat.Format1bppIndexed;
                                        bmpBitmap = new Bitmap(_imgSize, _imgSize, pixelFormat);

                                        var bitLock = bmpBitmap.LockBits(new Rectangle(Point.Empty, bmpBitmap.Size), ImageLockMode.WriteOnly, pixelFormat);

                                        unsafe
                                        {
                                            uint* pStart = (uint*)bitLock.Scan0;

                                            for (int i = 0; i < _maxVals; i++)
                                                pStart[i] = results[i];
                                        }
                                    }
                                    bmpBitmap.Save(pathBitmap, ImageFormat.Png);
                                }
                                System.Diagnostics.Process.Start(pathBitmap);

                                break;
                            case ConsoleKey.D2:
                                buildGraph(results.Take(1000), pathGraphs, ref bmpGraphs);
                                break;
                            case ConsoleKey.D3:
                                buildGraph(results, pathGraphsBig, ref bmpGraphsBig);
                                break;
                        }
                    }
                    while (key != ConsoleKey.X);
                    key = 0;

                    if (bmpBitmap != null)
                        bmpBitmap.Dispose();
                }

            }
            while (key != ConsoleKey.X);
        }

        private static ConsoleKey ReadKey()
        {
            Console.Write("> ");
            ConsoleKeyInfo info = Console.ReadKey();

            Console.WriteLine(Environment.NewLine);
            return info.Key;
        }

        private static void buildGraph(IEnumerable<uint> resultCollect, string path, ref Bitmap bmp)
        {
            if (bmp == null || !File.Exists(path))
            {
                if (bmp == null)
                {
                    const long maxDiv = uint.MaxValue + 1L;
                    uint[] results = resultCollect is uint[] ? (uint[])resultCollect : resultCollect.ToArray();

                    const long histLength = 10;
                    int[] histogram = new int[histLength];

                    foreach (uint curVal in results)
                        histogram[curVal * histLength / maxDiv]++;

                    const PixelFormat pixelFormat = PixelFormat.Format24bppRgb;
                    const int margin = 16;
                    const int totalMargin = margin * 4;
                    const int leftMargin = margin * 3;
                    int width = totalMargin + results.Length;

                    bmp = new Bitmap(width, 496 + margin + margin, pixelFormat);

                    using (Graphics g = Graphics.FromImage(bmp))
                    using (Font f = new Font(FontFamily.GenericMonospace, 6, FontStyle.Regular))
                    {
                        const int leftBorderX = leftMargin - 2;
                        const int firstTopY = margin, firstBottomY = firstTopY + 102;
                        const int secondTopY = firstBottomY + leftMargin, secondBottomY = secondTopY + 102;
                        const int thirdTopY = secondBottomY + leftMargin;
                        int rightBorderX = leftMargin + results.Length;

                        g.FillRectangle(Brushes.White, new Rectangle(Point.Empty, bmp.Size));

                        var hundreds = results.Select(i => (int)(i * 100L / maxDiv));

                        int? prevVal = null;
                        int curDex = leftMargin + 1;
                        foreach (int curVal in hundreds)
                        {
                            if (prevVal != null)
                            {
                                int nextDex = curDex + 1;
                                g.DrawLine(Pens.Blue, new Point(curDex, firstTopY + prevVal.Value),
                                    new Point(nextDex, firstTopY + curVal));
                                curDex = nextDex;
                            }
                            prevVal = curVal;
                        }
                        int prevDex = leftMargin;
                        curDex = leftMargin + 1;
                        foreach (int curVal in hundreds)
                        {
                            int nextDex = curDex + 1;
                            Rectangle top = new Rectangle(prevDex, curVal + firstTopY - 1, 2, 2), bottom = new Rectangle(prevDex, curVal + secondTopY - 1, 2, 2);

                            g.FillRectangle(Brushes.Cyan, top);
                            g.FillRectangle(Brushes.Cyan, bottom);
                            g.DrawRectangle(Pens.Blue, top);
                            g.DrawRectangle(Pens.Blue, bottom);

                            prevDex = curDex;
                            curDex = nextDex;
                        }
                        for (int i = 0; i < histogram.Length; i++)
                        {
                            var curVal = histogram[i];
                            int valWidth = curVal * 2;
                            const double histOffset = 1.0 / histLength;
                            const int lineHeight = 200 / (int)histLength;

                            Rectangle curRect = new Rectangle(leftBorderX, (lineHeight * i) + thirdTopY, valWidth, lineHeight);
                            g.FillRectangle(Brushes.Cyan, curRect);
                            g.DrawRectangle(Pens.Black, curRect);

                            string s = string.Format("{0:F2}-{1:F2}", histOffset * i, histOffset * (i + 1));
                            int textY = (lineHeight * i) + (thirdTopY + 1);

                            g.DrawString(s, f, Brushes.Black, leftBorderX - g.MeasureString(s, f).Width, textY);
                            g.DrawString(curVal.ToString(), f, Brushes.Black, (leftBorderX + 1) + valWidth, textY);
                        }

                        g.DrawLine(Pens.Black, new Point(leftBorderX, firstTopY), new Point(leftBorderX, firstBottomY));
                        g.DrawLine(Pens.Black, new Point(leftBorderX, firstBottomY), new Point(rightBorderX, firstBottomY));

                        g.DrawLine(Pens.Black, new Point(leftBorderX, secondTopY), new Point(leftBorderX, secondBottomY));
                        g.DrawLine(Pens.Black, new Point(leftBorderX, secondBottomY), new Point(rightBorderX, secondBottomY));

                        for (int i = 0; i < 10; i++)
                        {
                            int y = (i * 10);

                            const int leftBorderBegin = leftBorderX - 5;

                            int y1 = y + firstTopY;
                            int y2 = y + secondTopY;

                            g.DrawLine(Pens.Black, new Point(leftBorderBegin, y1), new Point(leftBorderX, y1));
                            g.DrawLine(Pens.Black, new Point(leftBorderBegin, y2), new Point(leftBorderX, y2));

                            if ((i & 1) == 0)
                            {
                                string s = ((10 - i) * 0.1).ToString();

                                float xStr = leftBorderBegin - g.MeasureString(s, f).Width;

                                g.DrawString(s, f, Brushes.Black, xStr, y1 - 5);
                                g.DrawString(s, f, Brushes.Black, xStr, y2 - 5);
                            }
                        }
                    }
                }
                bmp.Save(path, ImageFormat.Png);
            }
            System.Diagnostics.Process.Start(path);
        }
    }
}
