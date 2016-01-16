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

namespace URandomGen
{
    /// <summary>
    /// A random number generator based on the xorshift algorithm.
    /// </summary>
    public class RandomXorshift : RandomGen
    {
        private const int _seedCount = 32;
        private const int _seedMask = 31;

        private uint[] _seedArray;
        private int _curIndex;

        /// <summary>
        /// Creates a new instance using the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <see langword="null"/>.
        /// </exception>
        public RandomXorshift(IEnumerable<uint> seeds)
        {
            if (seeds == null) throw new ArgumentNullException("seeds");
#if !NOCONTRACT
            Contract.EndContractBlock();
#endif
            _seedArray = new uint[_seedCount];
            _curIndex = _seedMask;

            uint curCount = 0, prev = 0;

            foreach (uint c in seeds)
            {
                prev = _seedArray[_curIndex = (_curIndex + 1) & _seedMask] += c + (curCount ^ c ^ prev);

                curCount++;
            }

            if (IsNextThreeZero(_seedArray, _curIndex))
                prev = _seedArray[_curIndex] = uint.MaxValue / 3;

            for (int i = 0; i < (_seedCount * 2); i++)
            {
                uint result = SampleUInt32();
                _seedArray[_curIndex] = result;
            }
        }

        /// <summary>
        /// Creates a new instance using the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <see langword="null"/>.
        /// </exception>
        public RandomXorshift(params uint[] seeds)
            : this((IEnumerable<uint>)seeds)
        {
        }

        /// <summary>
        /// Creates a new instance using a range of elements within the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <param name="offset">The index in <paramref name="seeds"/> of the range of elements to use.</param>
        /// <param name="count">The number of elements to use in <paramref name="seeds"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> or <paramref name="count"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="count"/> plus <paramref name="offset"/> is greater than the number of elements in <paramref name="seeds"/>.
        /// </exception>
        public RandomXorshift(uint[] seeds, int offset, int count)
            : this(GetArraySegment(seeds, offset, count, "seeds"))
        {
        }

        /// <summary>
        /// Creates a new instance using the specified seed.
        /// </summary>
        /// <param name="seed">A 32-bit seed used to initialize the random number generator.</param>
        public RandomXorshift(uint seed)
            : this((IEnumerable<uint>)new uint[] { seed })
        {
        }

        /// <summary>
        /// Creates a new instance using the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <see langword="null"/>.
        /// </exception>
        public RandomXorshift(IEnumerable<int> seeds)
            : this(ToUIntIterator(seeds))
        {
        }

        /// <summary>
        /// Creates a new instance using the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <see langword="null"/>.
        /// </exception>
        public RandomXorshift(params int[] seeds)
            : this(ToUIntIterator(seeds))
        {
        }

        /// <summary>
        /// Creates a new instance using a range of elements within the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <param name="offset">The index in <paramref name="seeds"/> of the range of elements to use.</param>
        /// <param name="count">The number of elements to use in <paramref name="seeds"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> or <paramref name="count"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="count"/> plus <paramref name="offset"/> is greater than the number of elements in <paramref name="seeds"/>.
        /// </exception>
        public RandomXorshift(int[] seeds, int offset, int count)
            : this(GetArraySegment(seeds, offset, count, "seeds"))
        {
        }

        /// <summary>
        /// Creates a new instance using the specified seed.
        /// </summary>
        /// <param name="seed">A 32-bit seed used to initialize the random number generator.</param>
        public RandomXorshift(int seed)
            : this(unchecked((uint)seed))
        {
        }

        /// <summary>
        /// Creates a new instance using the specified collection of byte values.
        /// </summary>
        /// <param name="seeds">A collection containing bytes to add.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// Each segment of 4 bytes is converted to an unsigned 32-bit integer in little endian order; padding is added if necessary.
        /// </remarks>
        public RandomXorshift(IEnumerable<byte> seeds)
            : this(ToUIntIterator(seeds))
        {
        }

        /// <summary>
        /// Creates a new instance using the specified collection of byte values.
        /// </summary>
        /// <param name="seeds">A collection containing bytes to add.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// Each segment of 4 bytes is converted to an unsigned 32-bit integer in little endian order; padding is added if necessary.
        /// </remarks>
        public RandomXorshift(params byte[] seeds)
            : this(ToUIntIterator(seeds))
        {
        }

        /// <summary>
        /// Creates a new instance using a range of elements within the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <param name="offset">The index in <paramref name="seeds"/> of the range of elements to use.</param>
        /// <param name="count">The number of elements to use in <paramref name="seeds"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> or <paramref name="count"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="count"/> plus <paramref name="offset"/> is greater than the number of elements in <paramref name="seeds"/>.
        /// </exception>
        /// <remarks>
        /// Each segment of 4 bytes is converted to an unsigned 32-bit integer in little endian order; padding is added if necessary.
        /// </remarks>
        public RandomXorshift(byte[] seeds, int offset, int count)
            : this(GetArraySegment(seeds, offset, count, "seeds"))
        {
        }

        /// <summary>
        /// Creates a new instance using <see cref="RandomGen.DefaultSeeds()"/>.
        /// </summary>
        public RandomXorshift()
            : this((IEnumerable<uint>)DefaultSeeds())
        {
        }


        /// <summary>
        /// This method is used by other methods to generate random numbers.
        /// </summary>
        /// <returns>A 32-bit unsigned integer between 0 and <see cref="UInt32.MaxValue"/>.</returns>
        protected override uint SampleUInt32()
        {
            uint x = _seedArray[_curIndex];
            uint t = x ^ (x >> 7);
            uint y = _seedArray[(_curIndex + 2) & _seedMask];
            uint v = _seedArray[(_curIndex - 1) & _seedMask];
            _seedArray[_curIndex] = v = (v ^ (v << 6)) ^ (t ^ (t << 13));

            //Really unlikely edge-case, and the circumstances where it would even be an issue are rare as all get out, but why risk it?
            if (IsNextThreeZero(_seedArray, _curIndex))
                _seedArray[_curIndex] = uint.MaxValue / 7;

            _curIndex = (_curIndex + 1) & _seedMask;
            return (y + y + 1) * v;
        }
    }
}
