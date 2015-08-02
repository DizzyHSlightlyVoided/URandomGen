See also the documentation for [`System.Random`](https://msdn.microsoft.com/en-us/library/system.random.aspx).

Constructors
------------
### All Except `RandomCrypt`
```C#
new RandomCMWC(uint seed);
new RandomMersenne(uint seed);
new RandomWichHill(uint seed);
new RandomXorshift(uint seed);
```
Initializes a random number generator with the specified unsigned 32-bit integer seed.

```C#
new RandomCMWC(IEnumerable<uint> seeds);
new RandomMersenne(IEnumerable<uint> seeds);
new RandomWichHill(IEnumerable<uint> seeds);
new RandomXorshift(IEnumerable<uint> seeds);
```
Initializes a random number generator with the specified collection of unsigned 32-bit integer seeds. Throws [`ArgmunentNullException`](https://msdn.microsoft.com/en-us/library/system.argumentnullexception.aspx) when `seeds` is null.

```C#
new RandomCMWC(params uint[] seeds);
new RandomMersenne(params uint[] seeds);
new RandomWichHill(params uint[] seeds);
new RandomXorshift(params uint[] seeds);
```
Initializes a random number generator with the specified array of unsigned 32-bit integer seeds. Throws [`ArgmunentNullException`](https://msdn.microsoft.com/en-us/library/system.argumentnullexception.aspx) when `seeds` is null.

```C#
new RandomCMWC(int seed);
new RandomMersenne(int seed);
new RandomWichHill(int seed);
new RandomXorshift(int seed);
```
Initializes a random number generator with the specified signed 32-bit integer seed.

```C#
new RandomCMWC(IEnumerable<int> seeds);
new RandomMersenne(IEnumerable<int> seeds);
new RandomWichHill(IEnumerable<int> seeds);
new RandomXorshift(IEnumerable<int> seeds);
```
Initializes a random number generator with the specified collection of signed 32-bit integer seeds. Throws [`ArgmunentNullException`](https://msdn.microsoft.com/en-us/library/system.argumentnullexception.aspx) when `seeds` is null.

```C#
new RandomCMWC(IEnumerable<int> seeds);
new RandomMersenne(IEnumerable<int> seeds);
new RandomWichHill(IEnumerable<int> seeds);
new RandomXorshift(IEnumerable<int> seeds);
```
Initializes a random number generator with the specified array of signed 32-bit integer seeds. Throws [`ArgmunentNullException`](https://msdn.microsoft.com/en-us/library/system.argumentnullexception.aspx) when `seeds` is null.

```C#
new RandomCMWC();
new RandomMersenne();
new RandomWichHill();
new RandomXorshift();
```
Initializes a random number generator using the static method `RandomGen.DefaultSeeds()`.

### `RandomCrypt` Constructor

```C#
new RandomCrypt(RandomNumberGenerator generator);
```
Initializes a `RandomCrypt` generator with the specified [`RandomNumberGenerator`](https://msdn.microsoft.com/en-us/library/system.security.cryptography.randomnumbergenerator.aspx). Throws [`ArgmunentNullException`](https://msdn.microsoft.com/en-us/library/system.argumentnullexception.aspx) when `generator` is null.

Methods
-------
### `RandomGen.DefaultSingleSeed()`
```C#
public static uint DefaultSingleSeed();
```
Returns a single time-based default seed, specifically the lower 32 bits of the [`DateTime.Ticks`](https://msdn.microsoft.com/en-us/library/system.datetime.ticks.aspx) property of [`DateTime.Now`](https://msdn.microsoft.com/en-us/library/system.datetime.now.aspx).

### `RandomGen.DefaultSeeds()`
```C#
public static uint[] DefaultSeeds();
```
Returns an array containing time-based default seed values, all casted to unsigned 32-bit integer values:

* [`Environment.TickCount`](https://msdn.microsoft.com/en-us/library/system.environment.tickcount.aspx),
* The [`DateTime.Ticks`](https://msdn.microsoft.com/en-us/library/system.datetime.ticks.aspx) property of [`DateTime.Now`](https://msdn.microsoft.com/en-us/library/system.datetime.now.aspx) divided into two 32 bit values with the upper 32 bits first, and
* The sum of all three preceding values.

### `RandomCrypt.Next()`
```C#
public static int RandomCrypt.Next64(RandomNumberGenerator generator);
public static int RandomCrypt.Next64(RandomNumberGenerator generator, long maxValue);
public static int RandomCrypt.Next64(RandomNumberGenerator generator, long minValue, long maxValue);
```
This method and its overloads may be used to generate random numbers using a [`System.Security.Cryptography.RandomNumberGenerator`](https://msdn.microsoft.com/en-us/library/system.security.cryptography.randomnumbergenerator.aspx).

This and all other static methods throw [`ArgmunentNullException`](https://msdn.microsoft.com/en-us/library/system.argumentnullexception.aspx) when `generator` is null.

**Note:** This and all other methods which use `System.Security.Cryptography.RandomNumberGenerator` are a part of `URandomGen.RandomCrypt`, which is unavailable in the PCL version of this library.

### `RandomGen.Next64()` and `RandomCrypt.Next64()`
```C#
public long RandomGen.Next64();
public long RandomGen.Next64(long maxValue);
public long RandomGen.Next64(long minValue, long maxValue);
```
This implementation behaves exactly the same way as [`Random.Next()`](https://msdn.microsoft.com/en-us/library/system.random.next.aspx), but with 64-bit integers.

```C#
public static long RandomGen.Next64(Random generator);
public static long RandomGen.Next64(Random generator, long maxValue);
public static long RandomGen.Next64(Random generator, long minValue, long maxValue);

public static long RandomCrypt.Next64(RandomNumberGenerator generator);
public static long RandomCrypt.Next64(RandomNumberGenerator generator, long maxValue);
public static long RandomCrypt.Next64(RandomNumberGenerator generator, long minValue, long maxValue);
```
These methods replicate the above functionality for any `System.Random` or `System.Security.Cryptography.RandomNumberGenerator`.

### `RandomGen.NextUInt32()` and `RandomCrypt.Next64()`
```C#
public uint RandomGen.NextUInt32();
public uint RandomGen.NextUInt32(uint maxValue);
public uint RandomGen.NextUInt32(uint minValue, uint maxValue);

public static uint RandomGen.NextUInt32(Random generator);
public static uint RandomGen.NextUInt32(Random generator, uint maxValue);
public static uint RandomGen.NextUInt32(Random generator, uint minValue, uint maxValue);

public static uint RandomCrypt.NextUInt32(RandomNumberGenerator generator);
public static uint RandomCrypt.NextUInt32(RandomNumberGenerator generator, uint maxValue);
public static uint RandomCrypt.NextUInt32(RandomNumberGenerator generator, uint minValue, uint maxValue);
```
As above, but with 32-bit unsigned integers.

### `RandomGen.SampleUInt32();
```C#
protected abstract uint RandomGen.SampleUInt32();
```
This method is used by all other methods for generating random numbers. It returns a value greater than or equal to 0 and less than or equal to [`uint.MaxValue`](https://msdn.microsoft.com/en-us/library/system.uint32.maxvalue.aspx).

### `RandomGen.SampleUInt64();
```C#
protected virtual ulong RandomGen.SampleUInt64();
```
This method is used by all methods for generating 64-bit random numbers. It returns a value greater than or equal to 0 and less than or equal to [`ulong.MaxValue`](https://msdn.microsoft.com/en-us/library/system.uint64.maxvalue.aspx). The default version simply returns `SampleUInt32() | (ulong)SampleUInt32() << 32`.


### `RandomGen.NextUInt64()` and `RandomCrypt.NextUInt64()`
```C#
public uint RandomGen.NextUInt64();
public uint RandomGen.NextUInt64(uint maxValue);
public uint RandomGen.NextUInt64(uint minValue, uint maxValue);

public static uint RandomGen.NextUInt64(Random generator);
public static uint RandomGen.NextUInt64(Random generator, uint maxValue);
public static uint RandomGen.NextUInt64(Random generator, uint minValue, uint maxValue);

public static uint RandomCrypt.NextUInt64(RandomNumberGenerator generator);
public static uint RandomCrypt.NextUInt64(RandomNumberGenerator generator, uint maxValue);
public static uint RandomCrypt.NextUInt64(RandomNumberGenerator generator, uint minValue, uint maxValue);
```
As above, but with 64-bit unsigned integers.

### `RandomGen.NextNonZeroBytes()` and `RandomCrypt.NextNonZeroBytes()`
```C#
public void RandomGen.NextNonZeroBytes(byte[] buffer);
public static void RandomGen.NextNonZeroBytes(Random generator, byte[] buffer);
public static void RandomCrypt.NextNonZeroBytes(RandomNumberGenerator generator, byte[] buffer);
```
Fills `buffer` with randomly generated byte values which are greater than or equal to 1 and less than or equal to 255. These methods throw [`ArgmunentNullException`](https://msdn.microsoft.com/en-us/library/system.argumentnullexception.aspx) when `buffer` is null.

### `RandomGen.Shuffle<T>()` and `RandomCrypt.Shuffle<T>()`
```C#
public T[] RandomGen.Shuffle<T>(IEnumerable<T> collection);
public static T[] RandomGen.Shuffle<T>(Random generator, IEnumerable<T> collection);
public static T[] RandomCrypt.Shuffle<T>(RandomNumberGenerator generator, IEnumerable<T> collection);
```
Returns an array with all elements in `collection` in a randomized order. These methods throw [`ArgmunentNullException`](https://msdn.microsoft.com/en-us/library/system.argumentnullexception.aspx) when `collection` is null.

### `RandomGen.ShuffleArray<T>()` and `RandomCrypt.ShuffleArray<T>()`
```C#
public void RandomGen.ShuffleArray<T>(T[] array);
public static void RandomGen.ShuffleArray<T>(Random generator, T[] array);
public static void RandomCrypt.ShuffleArray<T>(RandomNumberGenerator generator, T[] array);
```
Reorders the elements of the specified array. This method performs the same operations as `Shuffle<T>()`. These methods throw [`ArgmunentNullException`](https://msdn.microsoft.com/en-us/library/system.argumentnullexception.aspx) when `array` is null.

### `RandomGen.RandomElement<T>()` and `RandomCrypt.RandomElement<T>()`
```C#
public T RandomGen.RandomElement<T>(IEnumerable<T> collection);
public static T RandomGen.RandomElement<T>(Random generator, IEnumerable<T> collection);
public static T RandomCrypt.RandomElement<T>(RandomNumberGenerator generator, IEnumerable<T> collection);
```
Returns a single random element from the specified collection. This method throws [`ArgmunentException`](https://msdn.microsoft.com/en-us/library/system.argumentexception.aspx) when `collection` is empty.

### `RandomGen.RandomElements<T>()` and `RandomCrypt.RandomElements<T>()`
```C#
public T[] RandomGen.RandomElements<T>(IEnumerable<T> collection, int length);
public static T[] RandomGen.RandomElements<T>(Random generator, IEnumerable<T> collection, int length);
public static T[] RandomCrypt.RandomElements<T>(RandomNumberGenerator generator, IEnumerable<T> collection, int length);
```
Returns an array containing random elements from the specified collection. This method throws [`ArgmunentException`](https://msdn.microsoft.com/en-us/library/system.argumentexception.aspx) when `collection` is empty, and  This method throws [`ArgmunentOutOfRangeException`](https://msdn.microsoft.com/en-us/library/system.argumentoutofrangeexception.aspx) when `length` is less than 0.

### `RandomGen.RandomString()`
```C#
public string RandomString(IEnumerable<char> collection, int length);
public static string RandomString(Random generator, IEnumerable<char> collection, int length);
public static string RandomCrypt.RandomString(RandomNumberGenerator generator,IEnumerable<char> collection, int length);
```

Same as `RandomElements<T>()`, but takes a `char` collection and returns a string.
