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
http://en.wikipedia.org/wiki/Multiply-with-carry
This article is released under a Creative Commons Attribution Share-Alike license.
See http://creativecommons.org/ and LICENSE-ThirdParty.md for more information.
*/
#endregion

using System;
using System.Collections.Generic;

namespace URandomGen
{
    /// <summary>
    /// A random number generator based on the complementary multiply-with-carry algorithm.
    /// </summary>
    public class RandomCMWC : RandomGen
    {

        private const int _seedCount = 4096;
        private const int _seedMask = 4095;
        private uint[] _seedArray;
        private int _curIndex;
        private uint _carry;

        /// <summary>
        /// Creates a new instance using the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <c>null</c>.
        /// </exception>
        public RandomCMWC(IEnumerable<uint> seeds)
        {
            if (seeds == null) throw new ArgumentNullException("seeds");
            _seedArray = new uint[_seedCount];
            _curIndex = _seedMask;

            const uint phi = 0x9E3779B9;

            uint curCount = 0, prev1 = 0, prev2 = 0, prev3 = 0;
            foreach (uint c in seeds)
            {
                uint curVal = _seedArray[_curIndex = (_curIndex + 1) & _seedMask] += (prev2 ^ prev3 ^ phi ^ c) * (curCount + c);
                curCount++;
                prev3 = prev2;
                prev2 = prev1;
                prev1 = curVal;
                _carry += curVal;
            }

            // According to George Marsaglia's 2003 post, the default "carry" should be a random number less than this value.
            _carry %= 809430660u;

            for (int i = 0; i < _seedCount; i++)
            {
                SampleUInt32();
                _seedArray[_curIndex] += ~phi;
            }
        }

        /// <summary>
        /// Creates a new instance using the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <c>null</c>.
        /// </exception>
        public RandomCMWC(params uint[] seeds)
            : this((IEnumerable<uint>)seeds)
        {
        }

        /// <summary>
        /// Creates a new instance using the specified seed.
        /// </summary>
        /// <param name="seed">A 32-bit seed used to initialize the random number generator.</param>
        public RandomCMWC(uint seed)
            : this((IEnumerable<uint>)new uint[] { seed })
        {
        }

        /// <summary>
        /// Creates a new instance using <see cref="RandomGen.DefaultSeeds()"/>.
        /// </summary>
        public RandomCMWC()
            : this((IEnumerable<uint>)DefaultSeeds())
        {
        }


        /// <summary>
        /// This method is used by other methods to generate random numbers.
        /// </summary>
        /// <returns>A 32-bit unsigned integer between 0 and <see cref="UInt32.MaxValue"/>.</returns>
        protected override uint SampleUInt32()
        {
            uint prevSeed = _seedArray[_curIndex];

            //Use multiple seeds, in order to mitigate the effects of successive zero-values.
            uint seed = _seedArray[_curIndex = (_curIndex + 1) & _seedMask];
            seed ^= prevSeed;

            const ulong multiplier = 18782UL;
            ulong t = (multiplier * seed) + _carry;
            _carry = (uint)(t >> 32);

            uint x = (uint)(t + _carry);

            if (x < _carry)
            {
                x++;
                _carry++;
            }

            const uint mask = 0xfffffffe;
            uint result = mask - x;

            //Really unlikely edge-case, but why risk it?
            _seedArray[_curIndex] = (result == 0 && prevSeed == 0) ? 1 : result;

            return result;
        }
    }
}
