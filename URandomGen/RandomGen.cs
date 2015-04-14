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
        /// property of <see cref="DateTime.Now"/>, the lower 32 bits thereof, and the sum of all previous values, converted to <see cref="UInt32"/>
        /// values.</returns>
        public static uint[] DefaultSeeds()
        {
            uint tickCount = (uint)Environment.TickCount;
            ulong nowTicks = (ulong)DateTime.Now.Ticks;

            uint[] seeds = new uint[]
            {
                (uint)Environment.TickCount,
                (uint)(nowTicks >> 32),
                (uint)nowTicks,
                0
            };

            seeds[3] = seeds[0] + seeds[1] + seeds[2];

            return seeds;
        }

        internal uint CopyToArray(IEnumerable<uint> seeds, uint[] destination)
        {
            if (seeds == null) throw new ArgumentNullException("seeds");

            uint count = 0;
            bool allZero = true;
            foreach (uint curVal in seeds)
            {
                destination[count++] = curVal;
                if (curVal != 0) allZero = false;
                if (count >= destination.Length) break;
            }

            if (allZero) destination[0] = count;

            return count;
        }

        /// <summary>
        /// When overridden in a derived class, this method is used by other methods to generate random numbers.
        /// </summary>
        /// <returns>A 32-bit unsigned integer between 0 and <see cref="UInt32.MaxValue"/>.</returns>
        protected abstract uint SampleUInt32();

        /// <summary>
        /// Return a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>A random number which is greater than or equal to 0.0 and which is less than 1.0.</returns>
        protected sealed override double Sample()
        {
            const double max = uint.MaxValue + 1.0;

            return SampleUInt32() / max;
        }

        private long _sampleValue(long length)
        {
            return ((length * SampleUInt32()) / uint.MaxValue);
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
            Contract.EndContractBlock();

            long length = maxValue - (long)minValue;

            return (int)(minValue + _sampleValue(length));
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
            Contract.EndContractBlock();

            return (int)_sampleValue(maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>A signed 32-bit integer which is greater than or equal to 0 and less than <see cref="Int32.MaxValue"/>.</returns>
        public override int Next()
        {
            return (int)(SampleUInt32() >> 1);
        }

        /// <summary>
        /// Fills the elements of the specified array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">A byte array to fill with random numbers.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is <c>null</c>.
        /// </exception>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");

            Contract.EndContractBlock();

            const int maxBytes = Byte.MaxValue + 1;

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)(SampleUInt32() % maxBytes);
        }
    }
}
