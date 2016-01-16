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

This file uses code derived from the content of the Wikipedia article
http://en.wikipedia.org/wiki/Wichmann-Hill
This article is released under a Creative Commons Attribution Share-Alike license.
See http://creativecommons.org/ and LICENSE-ThirdParty.md for more information.
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
    /// A random number generator using a variation on the Wichmann-Hill algorithm.
    /// </summary>
    public class RandomWichHill : RandomGen
    {
        private const int _seedCount = 64, _seedMask = 63;
        private ushort[] _seedArray;
        private int _curIndex;

        /// <summary>
        /// Creates a new instance using the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <see langword="null"/>.
        /// </exception>
        public RandomWichHill(IEnumerable<uint> seeds)
        {
            if (seeds == null) throw new ArgumentNullException("seeds");
#if !NOCONTRACT
            Contract.EndContractBlock();
#endif
            _seedArray = new ushort[_seedCount];

            _seedArray[0] = 177;
            int nextIndex = 2;
            _curIndex = 1;

            foreach (uint curSeed in seeds)
            {
                _seedArray[_curIndex] = (ushort)(((_seedArray[_curIndex] + (curSeed & 0xFFFF)) % 30000) + 1);
                _seedArray[nextIndex] = (ushort)(((curSeed >> 16) % 30000) + 1);
                Sample();
                nextIndex = (nextIndex + 1) & _seedMask;
            }
            _curIndex = (_curIndex - 1) & _seedMask;

            for (int i = 0; i < _seedCount * 2; i++)
                Sample();
        }

        /// <summary>
        /// Creates a new instance using the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <see langword="null"/>.
        /// </exception>
        public RandomWichHill(params uint[] seeds)
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
        /// <remarks>
        /// Each segment of 4 bytes is converted to an unsigned 32-bit integer in little endian order; padding is added if necessary.
        /// </remarks>
        public RandomWichHill(uint[] seeds, int offset, int count)
            : this(GetArraySegment(seeds, offset, count, "seeds"))
        {
        }

        /// <summary>
        /// Creates a new instance using the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <see langword="null"/>.
        /// </exception>
        public RandomWichHill(IEnumerable<int> seeds)
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
        public RandomWichHill(params int[] seeds)
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
        public RandomWichHill(int[] seeds, int offset, int count)
            : this(GetArraySegment(seeds, offset, count, "seeds"))
        {
        }

        /// <summary>
        /// Creates a new instance using the specified seed.
        /// </summary>
        /// <param name="seed">A 32-bit seed used to initialize the random number generator.</param>
        public RandomWichHill(int seed)
            : this(unchecked((uint)seed))
        {
        }

        /// <summary>
        /// Creates a new instance using the specified seed.
        /// </summary>
        /// <param name="seed">A 32-bit seed used to initialize the random number generator.</param>
        public RandomWichHill(uint seed)
            : this((IEnumerable<uint>)new uint[] { seed })
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
        public RandomWichHill(IEnumerable<byte> seeds)
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
        public RandomWichHill(params byte[] seeds)
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
        public RandomWichHill(byte[] seeds, int offset, int count)
            : this(GetArraySegment(seeds, offset, count, "seeds"))
        {
        }

        /// <summary>
        /// Creates a new instance using <see cref="RandomGen.DefaultSeeds()"/>.
        /// </summary>
        public RandomWichHill()
            : this((IEnumerable<uint>)DefaultSeeds())
        {
        }


        /// <summary>
        /// This method is used by other methods to generate random 32-bit numbers.
        /// </summary>
        /// <returns>A 32-bit unsigned integer which is greater than or equal to 0 and less than or equal to <see cref="UInt32.MaxValue"/>.</returns>
        protected override uint SampleUInt32()
        {
            return (uint)Math.Round(Sample() * uint.MaxValue + 1.0);
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>A random number which is greater than or equal to 0.0 and which is less than 1.0.</returns>
        protected override double Sample()
        {
            int prevIndex = (_curIndex - 1) & _seedMask;
            int nextIndex = (_curIndex + 1) & _seedMask;
            ushort s1 = _seedArray[prevIndex];
            ushort s2 = _seedArray[_curIndex];
            ushort s3 = _seedArray[nextIndex];

            const ushort div1 = 30269, div2 = 30307, div3 = 30323;
            double d1 = s1 = (ushort)((171 * s1) % div1);
            double d2 = s2 = (ushort)((172 * s2) % div2);
            double d3 = s3 = (ushort)((170 * s3) % div3);

            double result = ((d1 / div1) + (d2 / div2) + (d3 / div3)) % 1.0;

            if (s1 == 0) s1 = (ushort)((174 * result) % div1);
            if (s2 == 0) s2 = (ushort)((175 * result) % div2);
            if (s3 == 0) s3 = (ushort)((173 * result) % div3);
            _seedArray[prevIndex] = s1;
            _seedArray[_curIndex] = s2;
            _seedArray[nextIndex] = s3;
            _curIndex = nextIndex;
            return result;
        }
    }
}
