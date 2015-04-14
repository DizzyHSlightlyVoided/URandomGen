URandomGen
==========

A .Net library containing alternate implementations of the `System.Random` type.

Each random generator is derived from the [`System.Random`](http://msdn.microsoft.com/en-us/library/system.random.aspx) class of the .Net standard library, and may be used in the exact same way. While they are implemented in a number of different ways, each one allows the programmer to use a collection of unsigned seeds, rather than a single 32-bit seed, and (hopefully) fulfills the following principles:

1. An arbitrary number of seeds may be used (subject to the limitations of the implementation).
2. All seeds are equal; there are no cases where some seeds are "better" or "get better random results" than others.
3. As an extension of point 2, multiple successive 0-values in the seed collection should not necessarily result in the random number generator outputting multiple successive 0-values.

The Generators
--------------
The RandomCMWC class is based on the [complementary multiply-with-carry](http://en.wikipedia.org/wiki/Multiply-with-carry) random number generator, also invented by George Marsaglia, described [here](http://digitalcommons.wayne.edu/cgi/viewcontent.cgi?article=1725&context=jmasm); the implementation is derived from [a post](https://groups.google.com/d/msg/comp.lang.C/qZFQgKRCQGg/rmPkaRHqxOMJ) which Marsaglia made to the comp.lang.c newsgroup in 2003.

The RandomXorshift class is based on the [xorshift](http://en.wikipedia.org/wiki/Xorshift) random number generator developed by George Marsaglia, described [here](http://www.jstatsoft.org/v08/i14/paper). Specifically, it uses the [xorshift*](http://en.wikipedia.org/wiki/Xorshift#Xorshift.2A) variation, which takes a typical xorshift operation and multiplies it by a constant value.

Licenses
--------

The URandomGen library is released under the [FreeBSD license](LICENSE.md). All of the classes herein are derived from existing principles; see [LICENSE-ThirdParty.md](LICENSE-ThirdParty.md) for more information.
