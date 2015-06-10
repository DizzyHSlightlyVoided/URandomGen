#region BSD license
/*
Copyright © 2015, KimikoMuffin.
The Mersenne Twister and MT19937 implementations are copyright © 1997 - 2002,
Makoto Matsumoto and Takuji Nishimura.
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

namespace URandomGen
{
    /// <summary>
    /// A random number generator based on the Mersenne twister 19937 algorithm.
    /// </summary>
    public class RandomMersenne : RandomGen
    {
        private const int _seedCount = 624;

        private uint[] _seedArray;
        private uint _curIndex = _seedCount;

        /// <summary>
        /// Creates a new instance using the specified seed.
        /// </summary>
        /// <param name="seed">A 32-bit seed used to initialize the random number generator.</param>
        public RandomMersenne(uint seed)
        {
            _seedArray = new uint[_seedCount];
            uint prevSeed = _seedArray[0] = seed;

            for (uint i = 1; i < _seedCount; i++)
                prevSeed = _seedArray[i] = (1812433253U * _msblsb(prevSeed) + i);
        }

        /// <summary>
        /// Creates a new instance using the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <c>null</c>.
        /// </exception>
        public RandomMersenne(uint[] seeds)
            : this(_arrayLen(seeds), seeds)
        {
        }

        /// <summary>
        /// Creates a new instance using the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <c>null</c>.
        /// </exception>
        public RandomMersenne(IEnumerable<int> seeds)
            : this(ToUIntIterator(seeds))
        {
        }

        /// <summary>
        /// Creates a new instance using the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <c>null</c>.
        /// </exception>
        public RandomMersenne(params int[] seeds)
            : this(ToUIntIterator(seeds))
        {
        }

        /// <summary>
        /// Creates a new instance using the specified seed.
        /// </summary>
        /// <param name="seed">A 32-bit seed used to initialize the random number generator.</param>
        public RandomMersenne(int seed)
            : this(unchecked((uint)seed))
        {
        }

        /// <summary>
        /// Creates a new instance using the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <c>null</c>.
        /// </exception>
        public RandomMersenne(IEnumerable<uint> seeds)
            : this(_toArray(seeds))
        {
        }

        /// <summary>
        /// Creates a new instance using <see cref="RandomGen.DefaultSeeds()"/>.
        /// </summary>
        public RandomMersenne()
            : this(DefaultSeeds())
        {
        }

        private RandomMersenne(int length, uint[] seeds)
            : this(19650218U) //Initializing via a constant, as per MT19937
        {
            if (length == 0) return;

            uint i = 1, j = 0;

            for (uint k = (_seedCount > (uint)length) ? _seedCount : (uint)length; k > 0; k--)
            {
                _seedArray[i] = (_seedArray[i] ^ (_msblsb(_seedArray[i - 1]) * 1664525U)) + seeds[j] + j;

                _iLoop(ref i);
                j++;
                if (j >= length) j = 0;
            }

            for (uint k = _seedCount - 1; k > 0; k--)
            {
                _seedArray[i] = (_seedArray[i] ^ (_msblsb(_seedArray[i - 1]) * 1566083941U)) - i;

                _iLoop(ref i);
            }
            _seedArray[0] = 0x80000000U;
        }

        private void _iLoop(ref uint i)
        {
            i++;
            if (i >= _seedCount)
            {
                _seedArray[0] = _seedArray[_seedCount - 1];
                i = 1;
            }
        }

        private static uint _msblsb(uint prevSeed)
        {
            return prevSeed ^ (prevSeed >> 30);
        }

        private static int _arrayLen(uint[] seeds)
        {
            if (seeds == null) throw new ArgumentNullException("seeds");
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<int>() >= 0);
            Contract.EndContractBlock();
#endif
            return seeds.Length;
        }

        private static uint[] _toArray(IEnumerable<uint> seeds)
        {
            if (seeds == null) throw new ArgumentNullException("seeds");
#if !NOCONTRACT
            Contract.EndContractBlock();
#endif
            if (seeds is uint[]) return (uint[])seeds;
#if NOLINQ
            return ToArray<uint>(seeds);
#else
            return seeds.ToArray();
#endif
        }

        /// <summary>
        /// This method is used by other methods to generate random numbers.
        /// </summary>
        /// <returns>A 32-bit unsigned integer between 0 and <see cref="UInt32.MaxValue"/>.</returns>
        protected override uint SampleUInt32()
        {
            if (_curIndex >= _seedCount)
            {
                const uint upperMask = 0x80000000U;
                const uint lowerMask = 0x7fffffffU;

                for (uint i = 0; i < _seedCount; i++)
                {
                    uint curVal = (_seedArray[i] & upperMask) + (_seedArray[(i + 1) % _seedCount] & lowerMask);

                    _seedArray[i] = _seedArray[(i + 397) % _seedCount] ^ (curVal >> 1);

                    if ((curVal & 1) == 1) //If y is odd ...
                        _seedArray[i] ^= 0x9908b0df;
                }
                _curIndex = 0;
            }

            uint result = _seedArray[_curIndex++];
            result ^= result >> 11;
            result ^= (result << 7) & 0x9d2c5680U;
            result ^= (result << 15) & 0xefc60000U;
            result ^= result >> 18;

            return result;
        }
    }
}
