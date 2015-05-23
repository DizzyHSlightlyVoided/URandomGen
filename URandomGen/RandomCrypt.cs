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
using System.Security.Cryptography;
#if !NOCONTRACT
using System.Diagnostics.Contracts;
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
    }
}
