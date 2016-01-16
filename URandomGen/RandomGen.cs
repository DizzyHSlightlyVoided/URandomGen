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
using System.Collections.Generic;
#if !NOCONTRACT
using System.Diagnostics.Contracts;
#endif
#if !NOLINQ
using System.Linq;
#endif
using BigValue =
#if NOBIGINT
    System.Decimal;
#else
    System.Numerics.BigInteger;
#endif

namespace URandomGen
{
    /// <summary>
    /// Base class for random number generators which use this schema; includes utility methods used by all classes.
    /// </summary>
    public abstract class RandomGen : Random
    {
        /// <summary>
        /// Returns an array containing a set of default time-based seed values.
        /// </summary>
        /// <returns>An array containing four elements: <see cref="Environment.TickCount"/>, the <see cref="DateTime.Ticks"/>
        /// property of <see cref="DateTime.Now"/> divided into two 32-bit values with the upper 32 bits first, and the sum of
        /// all previous values, all converted to <see cref="uint"/> values.</returns>
        public static uint[] DefaultSeeds()
        {
            uint tickCount = (uint)Environment.TickCount;
            ulong nowTicks = (ulong)DateTime.Now.Ticks;

            uint[] seeds = new uint[]
            {
                tickCount,
                (uint)(nowTicks >> 32),
                (uint)nowTicks,
                0
            };

            seeds[3] = seeds[0] + seeds[1] + seeds[2];

            return seeds;
        }

        /// <summary>
        /// Returns a single time-based default seed.
        /// </summary>
        /// <returns>The lower 32 bits of the <see cref="DateTime.Ticks"/> property of <see cref="DateTime.Now"/>.</returns>
        public static uint DefaultSingleSeed()
        {
            return (uint)DateTime.Now.Ticks;
        }

        internal static bool IsNextThreeZero(uint[] seeds, int curIndex)
        {
#if NOLINQ
            int count = 0;
            foreach (uint curItem in ArrayOffset(seeds, curIndex))
            {
                if (curItem != 0) return false;

                if (++count >= 3) break;
            }
            return true;
#else
            using (IEnumerator<uint> enumerator = ArrayOffset(seeds, curIndex).Take(3).Where(i => i != 0).GetEnumerator())
                return !enumerator.MoveNext();
#endif
        }

#if NOLINQ
        internal static T[] ToArray<T>(IEnumerable<T> collection)
        {
            if (collection is ICollection<T>)
            {
                T[] array = new T[((ICollection<T>)collection).Count];
                ((ICollection<T>)collection).CopyTo(array, 0);
                return array;
            }
            if (collection is System.Collections.ICollection)
            {
                T[] array = new T[((System.Collections.ICollection)collection).Count];
                ((System.Collections.ICollection)collection).CopyTo(array, 0);
                return array;
            }

            T[] buffer = new T[16];
            int curCount = 0;
            foreach (T item in collection)
            {
                if (curCount >= buffer.Length)
                    Array.Resize<T>(ref buffer, checked(buffer.Length * 2));

                buffer[curCount++] = item;
            }

            if (buffer.Length != curCount)
                Array.Resize(ref buffer, curCount);

            return buffer;
        }
#endif
        internal static IEnumerable<T> GetArraySegment<T>(T[] array, int offset, int count, string paramName)
        {
            if (array == null) throw new ArgumentNullException(paramName);

            var segment = new ArraySegment<T>(array, offset, count);
#if ARRAYSEG
            return segment;
#else
            int end = count + offset;
            for (int i = offset; i < end; i++)
                yield return array[i];
#endif
        }

        internal static IEnumerable<uint> ToUIntIterator(IEnumerable<byte> seeds)
        {
            if (seeds == null) throw new ArgumentNullException("seeds");

            using (IEnumerator<byte> enumerator = seeds.GetEnumerator())
            {
                uint value = 0;
                while (enumerator.MoveNext())
                {
                    value |= enumerator.Current;
                    if (!enumerator.MoveNext())
                    {
                        yield return value;
                        break;
                    }
                    value |= (uint)enumerator.Current << 8;
                    if (!enumerator.MoveNext())
                    {
                        yield return value;
                        break;
                    }
                    value |= (uint)enumerator.Current << 16;
                    if (!enumerator.MoveNext())
                    {
                        yield return value;
                        break;
                    }
                    value |= (uint)enumerator.Current << 24;
                    yield return value;
                    value = 0;
                }
            }
        }

        internal static IEnumerable<uint> ToUIntIterator(IEnumerable<int> seeds)
        {
            if (seeds == null) throw new ArgumentNullException("seeds");
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<IEnumerable<uint>>() != null);
            Contract.EndContractBlock();
#endif
            foreach (int value in seeds)
                yield return unchecked((uint)value);
        }

        internal static IEnumerable<T> ArrayOffset<T>(T[] array, int offset)
        {
            for (int i = offset; i < array.Length; i++)
                yield return array[i];
            for (int i = 0; i < offset; i++)
                yield return array[i];
        }

        /// <summary>
        /// When overridden in a derived class, this method is used by other methods to generate random 32-bit numbers.
        /// </summary>
        /// <returns>A 32-bit unsigned integer which is greater than or equal to 0 and less than or equal to <see cref="UInt32.MaxValue"/>.</returns>
        protected abstract uint SampleUInt32();

        /// <summary>
        /// This method is used by other methods to generate random 64-bit numbers.
        /// </summary>
        /// <returns>A 64-bit unsigned integer which is greater than or equal to 0 and less than or equal to <see cref="UInt64.MaxValue"/>.</returns>
        protected virtual ulong SampleUInt64()
        {
            return ((ulong)SampleUInt32() << 32) | SampleUInt32();
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>A random number which is greater than or equal to 0.0 and which is less than 1.0.</returns>
        protected override double Sample()
        {
            const double max = max32;

            return SampleUInt32() / max;
        }

        private static long _sample32(RandomGen generator, long length)
        {
            BigValue sample = generator.SampleUInt32();
#if NOBIGINT
            return (long)(length * (sample / max32));
#else
            return (long)((length * sample) / max32);
#endif
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
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<int>() >= minValue);
            Contract.Ensures(maxValue == minValue || Contract.Result<int>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (int)(_sample32(this, (long)maxValue - minValue) + minValue);
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
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<int>() >= 0);
            Contract.Ensures(maxValue == 0 || Contract.Result<int>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (int)_sample32(this, maxValue);
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
        public virtual uint NextUInt32(uint minValue, uint maxValue)
        {
            if (minValue > maxValue)
                base.Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<uint>() >= minValue);
            Contract.Ensures(maxValue == minValue || Contract.Result<uint>() < maxValue);
            Contract.EndContractBlock();
#endif
            long length = maxValue - (long)minValue;

            return (uint)(_sample32(this, length) + minValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 32-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public virtual uint NextUInt32(uint maxValue)
        {
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<uint>() >= 0);
            Contract.Ensures(maxValue == 0 || Contract.Result<uint>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (uint)_sample32(this, maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>An unsigned 32-bit integer which is greater than or equal to 0 and less than <see cref="Int32.MaxValue"/>.</returns>
        public virtual uint NextUInt32()
        {
            return NextUInt32(uint.MaxValue);
        }

        internal const int max16 = 1 << 16;
        internal const long max32 = 1L << 32;
        internal const long max48 = 1L << 48;
        internal
#if NOBIGINT
            const
#else
            static readonly
#endif
                BigValue max64 = ulong.MaxValue + BigValue.One;

        private static uint _nextUInt32(Random generator, long length)
        {
            if (generator is RandomGen)
                return (uint)_sample32((RandomGen)generator, length);

            if (length <= int.MaxValue)
                return (uint)generator.Next((int)length);

            BigValue result = ((uint)generator.Next(max16) << 16) | (uint)generator.Next(max16);

#if NOBIGINT
            return (uint)(length * (result / max32));
#else
            return (uint)((length * result) / max32);
#endif
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
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<uint>() >= minValue);
            Contract.Ensures(maxValue == minValue || Contract.Result<uint>() < maxValue);
            Contract.EndContractBlock();
#endif
            return _nextUInt32(generator, (long)maxValue - minValue) + minValue;
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
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<uint>() >= 0);
            Contract.Ensures(maxValue == 0 || Contract.Result<uint>() < maxValue);
            Contract.EndContractBlock();
#endif
            return _nextUInt32(generator, maxValue);
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

        private static BigValue _sample64(RandomGen generator, BigValue length)
        {
            if (length == max32)
                return generator.SampleUInt32();
            if (length <= max32)
#if NOBIGINT
                return (length / max32) * generator.SampleUInt32();

            return (length / max64) * generator.SampleUInt64();
#else
                return (length * generator.SampleUInt32()) / max32;

            return (length * generator.SampleUInt64()) / max64;
#endif
        }

        private static BigValue _next64(Random generator, BigValue length)
        {
            if (generator is RandomGen)
                return _sample64((RandomGen)generator, length);

            if (length <= int.MaxValue)
                return generator.Next((int)length);

            ulong result = (uint)generator.Next(max16) | ((uint)generator.Next(max16) << 16);

            if (length < max32)
                return (length * result) / max32;

            result |= (ulong)generator.Next(max16) << 32;

            if (length < max48)
            {
#if NOBIGINT
                return (length / max48) * result;
#else
                return (length * result) / max48;
#endif
            }
            result |= (ulong)generator.Next(max16) << 48;

#if NOBIGINT
            return ((length / max64) * result);
#else
            return ((length * result) / max64);
#endif
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
        public virtual long Next64(long minValue, long maxValue)
        {
            if (minValue > maxValue)
                base.Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<long>() >= minValue);
            Contract.Ensures(maxValue == minValue || Contract.Result<long>() < maxValue);
            Contract.EndContractBlock();
#endif

            return (long)(_sample64(this, (BigValue)maxValue - minValue) + minValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>A signed 64-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public virtual long Next64(long maxValue)
        {
            if (maxValue < 0)
                base.Next(-1); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<long>() >= 0);
            Contract.Ensures(maxValue == 0 || Contract.Result<long>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (long)_sample64(this, maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>A signed 64-bit integer which is greater than or equal to 0 and less than <see cref="Int32.MaxValue"/>.</returns>
        public virtual long Next64()
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
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<long>() >= minValue);
            Contract.Ensures(maxValue == minValue || Contract.Result<long>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (long)(_next64(generator, (BigValue)maxValue - minValue) + minValue);
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
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<long>() >= 0);
            Contract.Ensures(maxValue == 0 || Contract.Result<long>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (long)_next64(generator, maxValue);
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
        public virtual ulong NextUInt64(ulong minValue, ulong maxValue)
        {
            if (minValue > maxValue)
                base.Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<ulong>() >= minValue);
            Contract.Ensures(maxValue == minValue || Contract.Result<ulong>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (ulong)(_sample64(this, (BigValue)maxValue - minValue) + minValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 64-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public virtual ulong NextUInt64(ulong maxValue)
        {
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<ulong>() >= 0);
            Contract.Ensures(maxValue == 0 || Contract.Result<ulong>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (ulong)_sample64(this, maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>An unsigned 64-bit integer which is greater than or equal to 0 and less than <see cref="Int64.MaxValue"/>.</returns>
        public virtual ulong NextUInt64()
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
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<ulong>() >= minValue);
            Contract.Ensures(maxValue == minValue || Contract.Result<ulong>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (ulong)(_next64(generator, (BigValue)maxValue - minValue) + minValue);
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
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<ulong>() >= 0);
            Contract.Ensures(maxValue == 0 || Contract.Result<ulong>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (ulong)_next64(generator, maxValue);
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
#if !NOCONTRACT
            Contract.EndContractBlock();
#endif
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
        public virtual void NextNonZeroBytes(byte[] buffer)
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
            if (generator == null) throw new ArgumentNullException("generator");
            if (buffer == null) throw new ArgumentNullException("buffer");
#if !NOCONTRACT
            Contract.Ensures(Array.IndexOf<byte>(Contract.ValueAtReturn(out buffer), 0) < 0);
            Contract.EndContractBlock();
#endif
            const int maxBytes = byte.MaxValue + 1;
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)generator.Next(1, maxBytes);
        }

        private static bool TryGetCount<T>(IEnumerable<T> collection, out int count)
        {
            if (collection is ICollection<T>)
            {
                count = ((ICollection<T>)collection).Count;
                return true;
            }
            else if (collection is System.Collections.ICollection)
            {
                count = ((System.Collections.ICollection)collection).Count;
                return true;
            }
            else if (typeof(T).Equals(typeof(char)) && collection is string)
            {
                count = collection.ToString().Length;
                return true;
            }
#if IREADONLY
            else if (collection is IReadOnlyCollection<T>)
            {
                count = ((IReadOnlyCollection<T>)collection).Count;
                return true;
            }
#endif
            count = 0;
            return false;
        }

#if NOFUNC
        internal delegate TOut Func<T1, TOut>(T1 arg1);
        internal delegate TOut Func<TOut>();
#endif
        internal static void _shuffleArray<T>(Func<int, int> nextInt32, T[] array, int startIndex, int length)
        {
            int next;

            for (int i = 0, iPos = startIndex; i < length; i = next, iPos++)
            {
                next = i + 1;
                int j = i == 0 ? 0 : nextInt32(next);

                if (i == j)
                    continue;

                j += startIndex;

                T item = array[iPos];
                array[iPos] = array[j];
                array[j] = item;
            }
        }

        internal static T[] _shuffle<T>(Func<int, int> nextInt32, IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<T[]>() != null);
            Contract.EndContractBlock();
#endif
            int count;
            if (TryGetCount<T>(collection, out count))
            {
                T[] list = new T[count];
                if (count == 0) return list;
                count = 0;

                foreach (T item in collection)
                {
                    int next = count + 1;
                    int j = count == 0 ? 0 : nextInt32(next);

                    if (j == count)
                    {
                        list[count] = item;
                        count = next;
                        continue;
                    }
                    list[count] = list[j];
                    list[j] = item;
                    count = next;
                }
                return list;
            }
            T[] array =
#if NOLINQ
                ToArray<T>(collection);
#else
                collection.ToArray();
#endif
            _shuffleArray(nextInt32, array, 0, array.Length);
            return array;
        }

        /// <summary>
        /// Returns all elements in the specified collection in random order.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="collection">The collection whose elements will be shuffled.</param>
        /// <returns>A list containing the shuffled elements in <paramref name="collection"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> or <paramref name="collection"/> is <c>null</c>.
        /// </exception>
        public static T[] Shuffle<T>(Random generator, IEnumerable<T> collection)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            return _shuffle(generator.Next, collection);
        }

        /// <summary>
        /// Returns all elements in the specified collection in random order.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="collection">The collection whose elements will be shuffled.</param>
        /// <returns>A list containing the shuffled elements in <paramref name="collection"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <c>null</c>.
        /// </exception>
        public T[] Shuffle<T>(IEnumerable<T> collection)
        {
            return Shuffle(this, collection);
        }

        /// <summary>
        /// Shuffles all elements in the specified array.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="array">The array whose elements will be shuffled.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> or <paramref name="array"/> is <c>null</c>.
        /// </exception>
        public static void ShuffleArray<T>(Random generator, T[] array)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            if (array == null) throw new ArgumentNullException("array");
#if !NOCONTRACT
            Contract.EndContractBlock();
#endif
            _shuffleArray(generator.Next, array, 0, array.Length);
        }

        /// <summary>
        /// Shuffles all elements in the specified array.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="array">The array whose elements will be shuffled.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is <c>null</c>.
        /// </exception>
        public void ShuffleArray<T>(T[] array)
        {
            ShuffleArray(this, array);
        }

        internal static T _randomElement<T>(Func<int, int> nextInt32, IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            using (var enumer = collection.GetEnumerator())
                if (!enumer.MoveNext()) throw new ArgumentException("The collection is empty.", "collection");
#if !NOCONTRACT
            Contract.EndContractBlock();
#endif
            int elementCount;
            if (!TryGetCount(collection, out elementCount))
            {
                collection =
#if NOLINQ
                    ToArray(collection);
#else
                    collection.ToArray();
#endif
                elementCount = ((T[])collection).Length;
            }

            int index = nextInt32(elementCount);

            if (collection is IList<T>)
                return ((IList<T>)collection)[index];

            if (collection is System.Collections.IList)
                return (T)((System.Collections.IList)collection)[index];

            if (typeof(T).Equals(typeof(char)) && collection is string)
                return (T)(object)collection.ToString()[index];
#if IREADONLY
            if (collection is IReadOnlyList<T>)
                return ((IReadOnlyList<T>)collection)[index];
#endif

#if NOLINQ
            using (IEnumerator<T> enumer = collection.GetEnumerator())
            {
                while (enumer.MoveNext())
                    if (--index == 0) return enumer.Current;

                throw new InvalidOperationException("This cannot be!");
            }
#else
            return collection.ElementAt(index);
#endif
        }

        /// <summary>
        /// Returns a random element from the specified collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="collection">The collection from which to get a random element.</param>
        /// <returns>A random element from <paramref name="collection"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> or <paramref name="collection"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="collection"/> is empty.
        /// </exception>
        public static T RandomElement<T>(Random generator, IEnumerable<T> collection)
        {
            if (generator == null) throw new ArgumentNullException("generator");

            return _randomElement(generator.Next, collection);
        }

        /// <summary>
        /// Returns a random element from the specified collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="collection">The collection from which to get a random element.</param>
        /// <returns>A random element from <paramref name="collection"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="collection"/> is empty.
        /// </exception>
        public T RandomElement<T>(IEnumerable<T> collection)
        {
            return RandomElement(this, collection);
        }

        internal static T _randomElement<T>(Func<double> nextDouble, IEnumerable<PriorityNode<T>> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            PriorityNode<T>[] array =
#if NOLINQ
                ToArray(collection);
#else
                collection.ToArray();
#endif
            int result = _randomIndex(nextDouble, array, "collection");
#if !NOCONTRACT
            Contract.EndContractBlock();
#endif
            return array[result].Value;
        }

        internal static int _randomIndex<T>(Func<double> nextDouble, IList<PriorityNode<T>> list, string paramName)
        {
            if (list == null) throw new ArgumentNullException("list");

            double total = 0;
            List<PriorityNode<int>> dexList = new List<PriorityNode<int>>();

            for (int i = 0; i < list.Count; i++)
            {
                var curNode = list[i];

                if (curNode.Priority <= 0) continue;
                total += curNode.Priority;
                dexList.Add(new PriorityNode<int>(i, total));
            }
            if (dexList.Count == 0)
                throw new ArgumentException("The collection has no nodes with priorities greater than zero.", paramName);
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<int>() >= 0 && Contract.Result<int>() < list.Count);
            Contract.EndContractBlock();
#endif

            double result = nextDouble() * total;

            int dex = 0;

            while (dex < dexList.Count && dexList[dex].Priority < result)
                dex++;
            return dexList[dex].Value;
        }

        /// <summary>
        /// Returns a random value from the specified collection of <see cref="PriorityNode{T}"/> values.
        /// </summary>
        /// <typeparam name="T">The type of the value to return.</typeparam>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="collection">A collection containing <see cref="PriorityNode{T}"/> objects. Elements with a higher
        /// <see cref="PriorityNode{T}.Priority"/> value will have a higher probability of occurring. Elements with a
        /// <see cref="PriorityNode{T}.Priority"/> value less than or equal to 0 will be ignored.</param>
        /// <returns>The value of a random element in <paramref name="collection"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> or <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="collection"/> does not contain any elements with priorities greater than 0.
        /// </exception>
        public static T RandomElement<T>(Random generator, IEnumerable<PriorityNode<T>> collection)
        {
            if (generator == null) throw new ArgumentNullException("generator");

            return _randomElement(generator.NextDouble, collection);
        }

        /// <summary>
        /// Returns a random value from the specified collection of <see cref="PriorityNode{T}"/> values.
        /// </summary>
        /// <typeparam name="T">The type of the value to return.</typeparam>
        /// <param name="collection">A collection containing <see cref="PriorityNode{T}"/> objects. Elements with a higher
        /// <see cref="PriorityNode{T}.Priority"/> value will have a higher probability of occurring. Elements with a
        /// <see cref="PriorityNode{T}.Priority"/> value less than or equal to 0 will be ignored.</param>
        /// <returns>The value of a random element in <paramref name="collection"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="collection"/> does not contain any elements with priorities greater than 0.
        /// </exception>
        public T RandomElement<T>(IEnumerable<PriorityNode<T>> collection)
        {
            return RandomElement(this, collection);
        }

        /// <summary>
        /// Returns a random index from the specified collection of <see cref="PriorityNode{T}"/> values.
        /// </summary>
        /// <typeparam name="T">The type of the value to return.</typeparam>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="list">A list containing <see cref="PriorityNode{T}"/> objects. Elements with a higher
        /// <see cref="PriorityNode{T}.Priority"/> value will have a higher probability of occurring. Elements with a
        /// <see cref="PriorityNode{T}.Priority"/> value less than or equal to 0 will be ignored.</param>
        /// <returns>The index of a random element in <paramref name="list"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> or <paramref name="list"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="list"/> does not contain any elements with priorities greater than 0.
        /// </exception>
        public static int RandomIndex<T>(Random generator, IList<PriorityNode<T>> list)
        {
            if (generator == null) throw new ArgumentNullException("generator");

            return _randomIndex(generator.NextDouble, list, "list");
        }

        /// <summary>
        /// Returns a random index from the specified collection of <see cref="PriorityNode{T}"/> values.
        /// </summary>
        /// <typeparam name="T">The type of the value to return.</typeparam>
        /// <param name="list">A list containing <see cref="PriorityNode{T}"/> objects. Elements with a higher
        /// <see cref="PriorityNode{T}.Priority"/> value will have a higher probability of occurring. Elements with a
        /// <see cref="PriorityNode{T}.Priority"/> value less than or equal to 0 will be ignored.</param>
        /// <returns>The index of a random element in <paramref name="list"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="list"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="list"/> does not contain any elements with priorities greater than 0.
        /// </exception>
        public int RandomIndex<T>(IList<PriorityNode<T>> list)
        {
            return RandomIndex(this, list);
        }

        internal static T[] _randomElements<T>(Func<int, int> nextInt32, IEnumerable<T> collection, int length)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            using (IEnumerator<T> enumer = collection.GetEnumerator())
                if (!enumer.MoveNext()) throw new ArgumentException("The collection is empty.", "collection");
            if (length < 0)
                throw new ArgumentOutOfRangeException("length",
#if !NOARGRANGE3
                    length,
#endif
                    "The specified value is less than 0.");

            T[] result = new T[length]; //May throw OutOfMemoryException
#if !NOCONTRACT
            Contract.EndContractBlock();
#endif
            if (length == 0) return result;

            if (collection is IList<T>)
            {
                IList<T> list = (IList<T>)collection;

                for (int i = 0; i < length; i++)
                    result[i] = list[nextInt32(list.Count)];
            }
            else if (collection is System.Collections.IList)
            {
                System.Collections.IList list = (System.Collections.IList)collection;

                for (int i = 0; i < length; i++)
                    result[i] = (T)list[nextInt32(list.Count)];
            }
            else if (typeof(T).Equals(typeof(char)) && collection is string)
            {
                string str = collection as string;

                for (int i = 0; i < length; i++)
                    result[i] = (T)((object)str[nextInt32(str.Length)]);
            }
#if IREADONLY
            else if (collection is IReadOnlyList<T>)
            {
                IReadOnlyList<T> list = (IReadOnlyList<T>)collection;

                for (int i = 0; i < length; i++)
                    result[i] = list[nextInt32(list.Count)];
            }
#endif
            else
            {
                T[] array =
#if NOLINQ
                    ToArray(collection);
#else
                    collection.ToArray();
#endif

                for (int i = 0; i < length; i++)
                    result[i] = array[nextInt32(array.Length)];
            }
            return result;
        }

        /// <summary>
        /// Returns an array containing elements randomly copied from the specified collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="collection">A collection whose elements will be copied to the new array.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <returns>An array containing elements copied from <paramref name="collection"/>. Multiple instances of the same element may be copied.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> or <paramref name="collection"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="collection"/> is empty.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Could not allocate the return array.
        /// </exception>
        public static T[] RandomElements<T>(Random generator, IEnumerable<T> collection, int length)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            return _randomElements(generator.Next, collection, length);
        }

        /// <summary>
        /// Returns an array containing elements randomly copied from the specified collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="collection">A collection whose elements will be copied to the new array.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <returns>An array containing elements copied from <paramref name="collection"/>. Multiple instances of the same element may be copied.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="collection"/> is empty.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Could not allocate the return array.
        /// </exception>
        public T[] RandomElements<T>(IEnumerable<T> collection, int length)
        {
            return RandomElements(this, collection, length);
        }

        /// <summary>
        /// Returns a new string containing characters randomly copied from the specified collection.  
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="collection">A collection whose elements will be copied to the new array.</param>
        /// <param name="length">The number of characters to copy.</param>
        /// <returns>A string containing elements copied from <paramref name="collection"/>. Multiple instances of the same element may be copied.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> or <paramref name="collection"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="collection"/> is empty.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Could not allocate the return array.
        /// </exception>
        public static string RandomString(Random generator, IEnumerable<char> collection, int length)
        {
            return new string(RandomElements(generator, collection, length));
        }

        /// <summary>
        /// Returns a new string containing characters randomly copied from the specified collection.  
        /// </summary>
        /// <param name="collection">A collection whose elements will be copied to the new array.</param>
        /// <param name="length">The number of characters to copy.</param>
        /// <returns>A string containing elements copied from <paramref name="collection"/>. Multiple instances of the same element may be copied.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="collection"/> is empty.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Could not allocate the return array.
        /// </exception>
        public string RandomString(IEnumerable<char> collection, int length)
        {
            return new string(RandomElements(collection, length));
        }
    }
}
