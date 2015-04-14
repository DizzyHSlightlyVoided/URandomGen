using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace URandomGen
{
    /// <summary>
    /// A random number generator based on the xorshift* algorithm.
    /// </summary>
    public class RandomXorShift : RandomGen
    {
        private const int _seedCount = 32;
        private const int _seedMask = 31;

        private uint[] SeedArray;
        private int _curIndex;

        /// <summary>
        /// Creates a new instance using the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <c>null</c>.
        /// </exception>
        public RandomXorShift(IEnumerable<uint> seeds)
        {
            SeedArray = new uint[_seedCount];
            uint curCount = CopyToArray(seeds, SeedArray);

            for (uint i = curCount; i < _seedCount; i++)
            {
                switch (i)
                {
                    case 0:
                        SeedArray[0] = _seedCount;
                        break;
                    case 1:
                        SeedArray[1] = SeedArray[0] + _seedMask;
                        break;
                    default:
                        SeedArray[i] = curCount + (i * _generate(SeedArray[i - 2], SeedArray[i - 1]));
                        break;
                }
            }
        }

        /// <summary>
        /// Creates a new instance using the specified collection of seeds.
        /// </summary>
        /// <param name="seeds">A collection of seeds used to initialize the random number generator.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seeds"/> is <c>null</c>.
        /// </exception>
        public RandomXorShift(params uint[] seeds)
            : this((IEnumerable<uint>)seeds)
        {
        }

        /// <summary>
        /// Creates a new instance using the specified seed.
        /// </summary>
        /// <param name="seed">A 32-bit seed used to initialize the random number generator.</param>
        public RandomXorShift(uint seed)
            : this((IEnumerable<uint>)new uint[] { seed })
        {
        }

        /// <summary>
        /// Creates a new instance using <see cref="RandomGen.DefaultSeeds()"/>.
        /// </summary>
        public RandomXorShift()
            : this((IEnumerable<uint>)DefaultSeeds())
        {
        }

        private static uint _generate(uint seedX, uint seedW)
        {
            uint seedT = seedX ^ (seedX << 11);

            return seedW ^ (seedW >> 19) ^ seedT ^ (seedT >> 8);
        }

        /// <summary>
        /// This method is used by other methods to generate random numbers.
        /// </summary>
        /// <returns>A 32-bit unsigned integer between 0 and <see cref="UInt32.MaxValue"/>.</returns>
        protected override uint SampleUInt32()
        {
            uint seedX = SeedArray[_curIndex];
            uint seedW = SeedArray[_curIndex = (_curIndex + 1) & _seedMask];

            uint result = _generate(seedX, seedW) * 69069;

            //Really unlikely edge-case, but why risk it?
            SeedArray[_curIndex] = result == 0 ? 1 : result;

            return result;
        }
    }
}
