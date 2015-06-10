URandomGen
==========

A .Net library containing alternate implementations of the `System.Random` type.

Each random generator is derived from the [`System.Random`](http://msdn.microsoft.com/en-us/library/system.random.aspx) class of the .Net standard library, and may be used in the exact same way. While they are implemented in a number of different ways, each one allows the programmer to use a collection of unsigned seeds, rather than a single 32-bit seed, and (hopefully) fulfills the following principles:

1. An arbitrary number of seeds may be passed to the constructor.
2. All seeds are equal; there are no cases where some seeds are "better" or "get better random results" than others.
3. As an extension of point 2, multiple successive 0-values in the seed collection should not necessarily result in the random number generator outputting multiple successive 0-values.

The Generators
--------------
The base class is `URandomGen.RandomGen`; all other classes in this library are derived from this class.

`RandomCMWC` is based on the [complementary multiply-with-carry](http://en.wikipedia.org/wiki/Multiply-with-carry) random number generator invented by George Marsaglia, described [here](http://digitalcommons.wayne.edu/cgi/viewcontent.cgi?article=1725&context=jmasm); the implementation is derived from [a post](https://groups.google.com/d/msg/comp.lang.C/qZFQgKRCQGg/rmPkaRHqxOMJ) which Marsaglia made to the comp.lang.c newsgroup in 2003.

`RandomXorshift` is based on the [xorshift](http://en.wikipedia.org/wiki/Xorshift) random number generator, also invented by George Marsaglia, described [here](http://www.jstatsoft.org/v08/i14/paper). The implementation is derived from the aforementioned comp.lang.c post, but with 32 seeds stored instead of 5.

`RandomMersenne` is based on the [Mersenne Twister](http://en.wikipedia.org/wiki/Mersenne_Twister), developed by Makoto Matsumoto and Takuji Nishimura. The name comes from the fact that its period length is chosen to be a Mersenne prime. The class is mostly a port of Matsumodo and Nishimura's [MT19937](http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/MT2002/emt19937ar.html) implementation, which is based on the Mersenne prime 2<sup>19937</sup>-1.

`RandomWichHill` is a variation on the [Wichmann-Hill](http://en.wikipedia.org/wiki/Wichmann-Hill) algorithm, and stores 64 values instead of 3.

`RandomCrypt` simply uses a [`System.Security.Cryptography.RandomNumberGenerator`](https://msdn.microsoft.com/en-us/library/system.security.cryptography.randomnumbergenerator.aspx) to generate its random values.

Licenses
--------

The URandomGen library is released under the [three-clause BSD license](LICENSE.md). All of the classes herein are derived from existing principles; see [LICENSE-ThirdParty.md](LICENSE-ThirdParty.md) for more information.