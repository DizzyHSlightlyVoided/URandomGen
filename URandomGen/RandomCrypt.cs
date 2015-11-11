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

#if !NOCRYPT
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
#if !NOCONTRACT
using System.Diagnostics.Contracts;
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
    /// A random number generator which uses an underlying <see cref="RandomNumberGenerator"/> to perform its random operations.
    /// NOTE: This class does not call <see cref="IDisposable.Dispose()"/> on the random number generator.
    /// Always dispose of <see cref="IDisposable"/> objects when you're done using them.
    /// </summary>
    public class RandomCrypt : RandomGen
    {
        /// <summary>
        /// Initializes a new instance using the specified random number generator.
        /// </summary>
        /// <param name="generator">The <see cref="RandomNumberGenerator"/> to use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        public RandomCrypt(RandomNumberGenerator generator)
        {
            if (generator == null) throw new ArgumentNullException("generator");
#if !NOCONTRACT
            Contract.EndContractBlock();
#endif
            _gen = generator;
        }

        private RandomNumberGenerator _gen;
        /// <summary>
        /// Gets the <see cref="RandomNumberGenerator"/> used by the current instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// In a set operation, the specified value is <c>null</c>.
        /// </exception>
        public RandomNumberGenerator Generator
        {
            get { return _gen; }
            set
            {
                if (value == null) throw new ArgumentNullException();
#if !NOCONTRACT
                Contract.EndContractBlock();
#endif
                _gen = value;
            }
        }

        internal static uint SampleGen32(RandomNumberGenerator generator)
        {
            byte[] data = new byte[sizeof(uint)];

            generator.GetBytes(data);

            return data[0] | ((uint)data[1] << 8) | ((uint)data[2] << 16) | ((uint)data[3] << 24);
        }

        internal static ulong SampleGen64(RandomNumberGenerator generator)
        {
            byte[] data = new byte[8];
            generator.GetBytes(data);

            return data[0] | ((ulong)data[1] << 8) | ((ulong)data[2] << 16) | ((ulong)data[3] << 24) |
                ((ulong)data[4] << 32) | ((ulong)data[5] << 40) | ((ulong)data[6] << 48) | ((ulong)data[7] << 56);
        }

        private static long _next32(RandomNumberGenerator generator, long length)
        {
            BigValue sample = SampleGen32(generator);

#if NOBIGINT
            return (long)(length * (sample / max32));
#else
            return (long)((length * sample) / max32);
#endif
        }

        /// <summary>
        /// This method is used by other methods to generate random 64-bit numbers.
        /// </summary>
        /// <returns>A 32-bit unsigned integer which is greater than or equal to 0 and less than or equal to <see cref="UInt32.MaxValue"/>.</returns>
        protected override uint SampleUInt32()
        {
            return SampleGen32(_gen);
        }

        /// <summary>
        /// This method is used by other methods to generate random 64-bit numbers.
        /// </summary>
        /// <returns>A 64-bit unsigned integer which is greater than or equal to 0 and less than or equal to <see cref="UInt64.MaxValue"/>.</returns>
        protected override ulong SampleUInt64()
        {
            return SampleGen64(_gen);
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
            _gen.GetBytes(buffer);
        }

        /// <summary>
        /// Fills the elements of the specified array of bytes with random numbers between 1 and 255 inclusive.
        /// </summary>
        /// <param name="buffer">A byte array to fill with random numbers.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is <c>null</c>.
        /// </exception>
        public override void NextNonZeroBytes(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
#if !NOCONTRACT
            Contract.EndContractBlock();
#endif
            _gen.GetNonZeroBytes(buffer);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="minValue">The inclusive lower bound of the random value.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>A signed 32-bit integer which is greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than <paramref name="minValue"/>.
        /// </exception>
        public static int Next(RandomNumberGenerator generator, int minValue, int maxValue)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            if (maxValue < minValue)
                new Random().Next(minValue, maxValue);
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<int>() >= minValue);
            Contract.Ensures(maxValue == minValue || Contract.Result<int>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (int)(_next32(generator, (long)maxValue - minValue) + minValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>A signed 32-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public static int Next(RandomNumberGenerator generator, int maxValue)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            if (maxValue < 0) new Random().Next(maxValue);
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<int>() >= 0);
            Contract.Ensures(maxValue == 0 || Contract.Result<int>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (int)_next32(generator, maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <returns>A signed 32-bit integer which is greater than or equal to 0 and less than <see cref="Int32.MaxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        public static int Next(RandomNumberGenerator generator)
        {
            return Next(generator, int.MaxValue);
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
        public static uint NextUInt32(RandomNumberGenerator generator, uint minValue, uint maxValue)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            if (minValue > maxValue)
                new Random().Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<uint>() >= minValue);
            Contract.Ensures(maxValue == minValue || Contract.Result<uint>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (uint)(_next32(generator, (long)maxValue - minValue) + minValue);
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
        public static uint NextUInt32(RandomNumberGenerator generator, uint maxValue)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            if (maxValue < 0)
                new Random().Next(-1); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<uint>() >= 0);
            Contract.Ensures(maxValue == 0 || Contract.Result<uint>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (uint)_next32(generator, maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <returns>An unsigned 32-bit integer which is greater than or equal to 0 and less than <see cref="UInt32.MaxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        public static uint NextUInt32(RandomNumberGenerator generator)
        {
            return NextUInt32(generator, uint.MaxValue);
        }

        private static BigValue _next64(RandomNumberGenerator generator, BigValue length)
        {
            if (length < max32)
                return _next32(generator, (long)length);

            if (length == max32)
                return SampleGen32(generator);

            if (length < max48)
            {
                byte[] data = new byte[6];
                generator.GetBytes(data);

                ulong result = data[0] | ((ulong)data[1] << 8) | ((ulong)data[2] << 16) | ((ulong)data[3] << 24) | ((ulong)data[4] << 32) | ((ulong)data[5] << 40);

#if NOBIGINT
                return (length / max48) * result;
#else
                return (length * result) / max48;
#endif
            }
            else
            {
                ulong result = SampleGen64(generator);

#if NOBIGINT
                return (length / max64) * result;
#else
                return (length * result) / max64;
#endif
            }
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
        public static long Next64(RandomNumberGenerator generator, long minValue, long maxValue)
        {
            if (minValue > maxValue)
                new Random().Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
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
        public static long Next64(RandomNumberGenerator generator, long maxValue)
        {
            if (maxValue < 0)
                new Random().Next(-1); //Throw ArgumentOutOfRangeException according to default form.
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
        public static long Next64(RandomNumberGenerator generator)
        {
            return Next64(generator, long.MaxValue);
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
        public static ulong NextUInt64(RandomNumberGenerator generator, ulong minValue, ulong maxValue)
        {
            if (minValue > maxValue)
                new Random().Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
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
        public static ulong NextUInt64(RandomNumberGenerator generator, ulong maxValue)
        {
            if (maxValue < 0)
                new Random().Next(-1); //Throw ArgumentOutOfRangeException according to default form.
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
        public static ulong NextUInt64(RandomNumberGenerator generator)
        {
            return NextUInt64(generator, ulong.MaxValue);
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
        public static T[] Shuffle<T>(RandomNumberGenerator generator, IEnumerable<T> collection)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            return _shuffle(i => (int)_next32(generator, i), collection);
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
        public static void ShuffleArray<T>(RandomNumberGenerator generator, T[] array)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            if (array == null) throw new ArgumentNullException("array");
#if !NOCONTRACT
            Contract.EndContractBlock();
#endif
            _shuffleArray(i => (int)_next32(generator, i), array, 0, array.Length);
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
        public static T RandomElement<T>(RandomNumberGenerator generator, IEnumerable<T> collection)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            return _randomElement(i => (int)_next32(generator, i), collection);
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
        public static T[] RandomElements<T>(RandomNumberGenerator generator, IEnumerable<T> collection, int length)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            return _randomElements(i => (int)_next32(generator, i), collection, length);
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
        public static string RandomString(RandomNumberGenerator generator, IEnumerable<char> collection, int length)
        {
            return new string(RandomElements(generator, collection, length));
        }
    }
}
#endif
