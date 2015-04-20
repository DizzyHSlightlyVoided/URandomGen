using System;
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

                    Bitmap bmpBitmap = null;

                    string pathBitmap = generator.GetType().Name + ".Bitmap.png";

                    uint maxDelta = uint.MaxValue - max;
                    double avgDelta = Math.Abs(baseMedian - avg);

                    do
                    {
                        Console.WriteLine("Expected maximum: {0:10}", uint.MaxValue);
                        Console.WriteLine("Actual maximum:   {0:10} (delta: {1}, {2}%)", max, maxDelta, (100f * maxDelta) / uint.MaxValue);
                        Console.WriteLine("Expected minimum: 0");
                        Console.WriteLine("Actual minimum:   {0:10} (delta: {1}, {2}%)", min, min, (100f * min) / uint.MaxValue);
                        Console.WriteLine("Expected average: {0:10}", baseMedian);
                        Console.WriteLine("Actual average:   {0:10} (delta: {1}, {2}%)", avg, avgDelta, (100 * avgDelta) / baseMedian);
                        Console.WriteLine();

                        Console.WriteLine("1. Bitmap (saved in working dir as {0})", pathBitmap);
                        Console.WriteLine("X. Cancel");
                        key = ReadKey();

                        switch (key)
                        {
                            case ConsoleKey.D1:
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

                                    bmpBitmap.Save(pathBitmap, ImageFormat.Png);
                                }
                                System.Diagnostics.Process.Start(pathBitmap);

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
    }
}
