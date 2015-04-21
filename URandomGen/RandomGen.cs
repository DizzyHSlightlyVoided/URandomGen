#region BSD license
/*
Copyright © 2015, KimikoMuffin.
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.
3. The names of its contributors may not be used to endorse or promote 
   products derived from this software without specific prior written 
   permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace URandomGen
{
    /// <summary>
    /// Base class for random number generators which use this schema; includes utility methods used by all classes.
    /// </summary>
    public abstract class RandomGen : Random
    {
        /// <summary>
        /// Returns an array containing a set of default seed values.
        /// </summary>
        /// <returns>An array containing four elements: <see cref="Environment.TickCount"/>, the upper 32 bits of the <see cref="DateTime.Ticks"/>
        /// property of <see cref="DateTime.Now"/>, the lower 32 bits thereof, and the sum of all previous values, all converted to <see cref="UInt32"/>
        /// values.</returns>
        public static uint[] DefaultSeeds()
        {
            uint tickCount = (uint)Environment.TickCount;
            ulong nowTicks = (ulong)DateTime.Now.Ticks;

            uint[] seeds = new uint[]
            {
                (uint)tickCount,
                (uint)(nowTicks >> 32),
                (uint)nowTicks,
                0
            };

            seeds[3] = seeds[0] + seeds[1] + seeds[2];

            return seeds;
        }

        /// <summary>
        /// Returns a single default seed.
        /// </summary>
        /// <returns>The lower 32 bits of the <see cref="DateTime.Ticks"/> property of <see cref="DateTime.Now"/>.</returns>
        public static uint DefaultSingleSeed()
        {
            return (uint)DateTime.Now.Ticks;
        }

        internal static bool IsAllZero(IEnumerable<uint> seeds)
        {
            using (IEnumerator<uint> enumerator = seeds.Where(i => i != 0).GetEnumerator())
                return !enumerator.MoveNext();
        }

        /// <summary>
        /// When overridden in a derived class, this method is used by other methods to generate random 32-bit numbers.
        /// </summary>
        /// <returns>A 32-bit unsigned integer which is greater than or equal to 0 and less than or equal to <see cref="UInt32.MaxValue"/>.</returns>
        protected abstract uint SampleUInt32();

        /// <summary>
        /// This method is used by other methods to generate random 64-bit numbers.
        /// </summary>
        /// <returns>A 32-bit unsigned integer which is greater than or equal to 0 and less than or equal to <see cref="UInt64.MaxValue"/>.</returns>
        protected virtual ulong SampleUInt64()
        {
            return ((ulong)SampleUInt32() << 32) | SampleUInt32();
        }

        /// <summary>
        /// Return a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>A random number which is greater than or equal to 0.0 and which is less than 1.0.</returns>
        protected sealed override double Sample()
        {
            const double max = max32;

            return SampleUInt32() / max;
        }

        private static long _sampleValue(RandomGen generator, long length)
        {
            return ((length * generator.SampleUInt32()) / max32);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random value.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>A signed 32-bit integer which is greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than <paramref name="minValue"/>.
        /// </exception>
        public override int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
                base.Next(minValue, maxValue); //Throw ArgumentOutOfRangeException according to default form.
            Contract.Ensures(Contract.Result<int>() >= minValue);
            Contract.Ensures(Contract.Result<int>() < maxValue);
            Contract.EndContractBlock();

            long length = maxValue - (long)minValue;

            return (int)(minValue + _sampleValue(this, length));
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>A signed 32-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public override int Next(int maxValue)
        {
            if (maxValue < 0)
                base.Next(maxValue); //Throw ArgumentOutOfRangeException according to default form.
            Contract.Ensures(Contract.Result<int>() >= 0);
            Contract.Ensures(Contract.Result<int>() < maxValue);
            Contract.EndContractBlock();

            return (int)_sampleValue(this, maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>A signed 32-bit integer which is greater than or equal to 0 and less than <see cref="Int32.MaxValue"/>.</returns>
        public override int Next()
        {
            return Next(int.MaxValue);
        }


        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random value.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 32-bit integer which is greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than <paramref name="minValue"/>.
        /// </exception>
        public uint NextUInt32(uint minValue, uint maxValue)
        {
            if (minValue > maxValue)
                base.Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
            Contract.Ensures(Contract.Result<uint>() >= minValue);
            Contract.Ensures(Contract.Result<uint>() < maxValue);
            Contract.EndContractBlock();

            long length = maxValue - (long)minValue;

            return (uint)(minValue + _sampleValue(this, length));
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 32-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public uint NextUInt32(uint maxValue)
        {
            Contract.Ensures(Contract.Result<uint>() >= 0);
            Contract.Ensures(Contract.Result<uint>() < maxValue);
            Contract.EndContractBlock();

            return (uint)_sampleValue(this, maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>An unsigned 32-bit integer which is greater than or equal to 0 and less than <see cref="Int32.MaxValue"/>.</returns>
        public uint NextUInt32()
        {
            return NextUInt32(uint.MaxValue);
        }

        private const int max16 = 1 << 16;
        private const long max32 = 1L << 32;
        private const long max48 = 1L << 48;
        private const decimal max64 = ulong.MaxValue + 1.0m;

        private static uint _nextUInt32(Random generator, uint minValue, uint maxValue)
        {
            long length = maxValue - (long)minValue;
            if (generator is RandomGen)
                return (uint)(minValue + _sampleValue((RandomGen)generator, length));

            if (length <= int.MaxValue)
                return (uint)(generator.Next((int)length) + minValue);

            uint result = ((uint)generator.Next(max16) << 16) | (uint)generator.Next(max16);

            return (uint)((length * result) / max32);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="minValue">The inclusive lower bound of the random value.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 32-bit integer which is greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than <paramref name="minValue"/>.
        /// </exception>
        public static uint NextUInt32(Random generator, uint minValue, uint maxValue)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            if (minValue > maxValue)
                generator.Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
            Contract.Ensures(Contract.Result<uint>() >= minValue);
            Contract.Ensures(Contract.Result<uint>() < maxValue);
            Contract.EndContractBlock();

            return _nextUInt32(generator, minValue, maxValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 32-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public static uint NextUInt32(Random generator, uint maxValue)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            if (maxValue < 0)
                generator.Next(-1); //Throw ArgumentOutOfRangeException according to default form.
            Contract.Ensures(Contract.Result<uint>() >= 0);
            Contract.Ensures(Contract.Result<uint>() < maxValue);
            Contract.EndContractBlock();

            return _nextUInt32(generator, 0, maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <returns>An unsigned 32-bit integer which is greater than or equal to 0 and less than <see cref="UInt32.MaxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        public static uint NextUInt32(Random generator)
        {
            return NextUInt32(generator, uint.MaxValue);
        }


        private static decimal _sampleValue(RandomGen generator, decimal length)
        {
            if (length == max32)
                return generator.SampleUInt32();
            if (length <= max32)
                return (length * generator.SampleUInt32()) / max32;

            return (length * generator.SampleUInt64()) / max64;
        }

        private static decimal _next64(Random generator, decimal minValue, decimal maxValue)
        {
            decimal length = maxValue - minValue;

            if (generator is RandomGen)
                return minValue + _sampleValue((RandomGen)generator, length);

            if (length < max32)
                return minValue + generator.Next((int)length);

            ulong result = ((uint)generator.Next(max16) << 16) | (uint)generator.Next(max16);

            if (result <= uint.MaxValue)
                return (length * result) / max32;

            result |= (ulong)generator.Next(max16) << 32;

            if (result < max48)
                return (length * result) / max48;

            result |= (ulong)generator.Next(max16) << 48;

            return length * result / max64;
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random value.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>A signed 64-bit integer which is greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than <paramref name="minValue"/>.
        /// </exception>
        public long Next64(long minValue, long maxValue)
        {
            if (minValue > maxValue)
                base.Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
            Contract.Ensures(Contract.Result<long>() >= minValue);
            Contract.Ensures(Contract.Result<long>() < maxValue);
            Contract.EndContractBlock();

            decimal length = (decimal)maxValue - minValue;

            return (long)(_sampleValue(this, length) + minValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>A signed 64-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public long Next64(long maxValue)
        {
            if (maxValue < 0)
                base.Next(-1); //Throw ArgumentOutOfRangeException according to default form.
            Contract.Ensures(Contract.Result<long>() >= 0);
            Contract.Ensures(Contract.Result<long>() < maxValue);
            Contract.EndContractBlock();

            return (long)_sampleValue(this, (decimal)maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>A signed 64-bit integer which is greater than or equal to 0 and less than <see cref="Int32.MaxValue"/>.</returns>
        public long Next64()
        {
            return Next64(long.MaxValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="minValue">The inclusive lower bound of the random value.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>A signed 64-bit integer which is greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than <paramref name="minValue"/>.
        /// </exception>
        public static long Next64(Random generator, long minValue, long maxValue)
        {
            if (minValue > maxValue)
                generator.Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
            Contract.Ensures(Contract.Result<long>() >= minValue);
            Contract.Ensures(Contract.Result<long>() < maxValue);
            Contract.EndContractBlock();

            return (long)_next64(generator, minValue, maxValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>A signed 64-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public static long Next64(Random generator, long maxValue)
        {
            if (maxValue < 0)
                generator.Next(-1); //Throw ArgumentOutOfRangeException according to default form.
            Contract.Ensures(Contract.Result<long>() >= 0);
            Contract.Ensures(Contract.Result<long>() < maxValue);
            Contract.EndContractBlock();

            return (long)_next64(generator, 0, maxValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <returns>A signed 64-bit integer which is greater than or equal to 0 and less than <see cref="Int64.MaxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        public static long Next64(Random generator)
        {
            return Next64(generator, long.MaxValue);
        }


        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random value.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 64-bit integer which is greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than <paramref name="minValue"/>.
        /// </exception>
        public ulong NextUInt64(ulong minValue, ulong maxValue)
        {
            if (minValue > maxValue)
                base.Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
            Contract.Ensures(Contract.Result<ulong>() >= minValue);
            Contract.Ensures(Contract.Result<ulong>() < maxValue);
            Contract.EndContractBlock();

            decimal length = (decimal)maxValue - minValue;

            return (ulong)(_sampleValue(this, length) + minValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 64-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public ulong NextUInt64(ulong maxValue)
        {
            Contract.Ensures(Contract.Result<ulong>() >= 0);
            Contract.Ensures(Contract.Result<ulong>() < maxValue);
            Contract.EndContractBlock();

            return (ulong)_sampleValue(this, (decimal)maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>An unsigned 64-bit integer which is greater than or equal to 0 and less than <see cref="Int64.MaxValue"/>.</returns>
        public ulong NextUInt64()
        {
            return NextUInt64(ulong.MaxValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="minValue">The inclusive lower bound of the random value.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 64-bit integer which is greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than <paramref name="minValue"/>.
        /// </exception>
        public static ulong NextUInt64(Random generator, ulong minValue, ulong maxValue)
        {
            if (minValue > maxValue)
                generator.Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
            Contract.Ensures(Contract.Result<ulong>() >= minValue);
            Contract.Ensures(Contract.Result<ulong>() < maxValue);
            Contract.EndContractBlock();

            return (ulong)_next64(generator, minValue, maxValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 64-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public static ulong NextUInt64(Random generator, ulong maxValue)
        {
            if (maxValue < 0)
                generator.Next(-1); //Throw ArgumentOutOfRangeException according to default form.
            Contract.Ensures(Contract.Result<ulong>() >= 0);
            Contract.Ensures(Contract.Result<ulong>() < maxValue);
            Contract.EndContractBlock();
            var value = _next64(generator, 0, maxValue);
            return (ulong)value;
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <returns>An unsigned 64-bit integer which is greater than or equal to 0 and less than <see cref="UInt64.MaxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        public static ulong NextUInt64(Random generator)
        {
            return NextUInt64(generator, ulong.MaxValue);
        }


        /// <summary>
        /// Fills the elements of the specified array of bytes with random numbers between 0 and 255 inclusive.
        /// </summary>
        /// <param name="buffer">A byte array to fill with random numbers.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is <c>null</c>.
        /// </exception>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");

            Contract.EndContractBlock();

            const int maxBytes = byte.MaxValue + 1;

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)(SampleUInt32() % maxBytes);
        }

        /// <summary>
        /// Fills the elements of the specified array of bytes with random numbers between 1 and 255 inclusive.
        /// </summary>
        /// <param name="buffer">A byte array to fill with random numbers.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is <c>null</c>.
        /// </exception>
        public void NextNonZeroBytes(byte[] buffer)
        {
            NextNonZeroBytes(this, buffer);
        }

        /// <summary>
        /// Fills the elements of the specified array of bytes with random numbers between 1 and 255 inclusive.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="buffer">A byte array to fill with random numbers.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is <c>null</c>.
        /// </exception>
        public static void NextNonZeroBytes(Random generator, byte[] buffer)
        {
            if (generator == null) throw new ArgumentNullException("buffer");
            if (buffer == null) throw new ArgumentNullException("buffer");

            Contract.Ensures(Array.IndexOf(Contract.ValueAtReturn(out buffer), 0) < 0);
            Contract.EndContractBlock();

            const int maxBytes = byte.MaxValue + 1;
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)generator.Next(1, maxBytes);
        }
    }
}
