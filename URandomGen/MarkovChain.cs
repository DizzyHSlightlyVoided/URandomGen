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
    /// Represents a Markov chain generator, which produces a sequence of values in which each value is dependent only on the immediately previous value.
    /// </summary>
    /// <typeparam name="T">The type of the objects in the Markov chain.</typeparam>
    public class MarkovChain<T>
    {
        #region Constructors
        /// <summary>
        /// Creates a new empty Markov chain generator using the specified equality comparer.
        /// </summary>
        /// <param name="comparer">The equality comparer to use, or <see langword="null"/> to use the default comparer for type <typeparamref name="T"/>.</param>
        public MarkovChain(IEqualityComparer<T> comparer)
        {
            _lengthDict = new Dictionary<int, int>();
            _lengths = new WeightedList<int>();
            Lengths = _lengths.AsReadOnly();

            ChainValue.Comparer valComp = new ChainValue.Comparer(comparer);
            _comparer = valComp.EqualityComparer;

            _itemDict = new Dictionary<ChainValue, int>(valComp);
            _items = new WeightedList<MarkovChainNode<T>>();
            ItemList = _items.AsReadOnly();
            _itemDict.Add(default(ChainValue), 0);
            _items.Add(new MarkovChainNode<T>(this), 0);

            _firstDict = new Dictionary<ChainValue, int>(valComp);
            _firsts = new WeightedList<MarkovChainNode<T>>();
            FirstList = _firsts.AsReadOnly();
        }

        /// <summary>
        /// Creates a new empty Markov chain generator using the default equality comparer.
        /// </summary>
        public MarkovChain()
            : this(null)
        {
        }
        #endregion

        private IEqualityComparer<T> _comparer;
        /// <summary>
        /// Gets the equality comparer associated with the current instance.
        /// </summary>
        public IEqualityComparer<T> Comparer { get { return _comparer; } }

        internal IEqualityComparer<ChainValue> ChainNodeComparer { get { return _itemDict.Comparer; } }

        private Dictionary<int, int> _lengthDict;
        private WeightedList<int> _lengths;
        /// <summary>
        /// Gets a read-only list containing all lengths in the dictionary.
        /// </summary>
        public WeightedList<int>.ReadOnly Lengths { get; private set; }

        private Dictionary<ChainValue, int> _itemDict;
        private WeightedList<MarkovChainNode<T>> _items;
        /// <summary>
        /// Gets a read-only list containing all values used by the Markov chain.
        /// </summary>
        public WeightedList<MarkovChainNode<T>>.ReadOnly ItemList { get; private set; }

        private Dictionary<ChainValue, int> _firstDict;
        private WeightedList<MarkovChainNode<T>> _firsts;
        /// <summary>
        /// Gets a read-only list containing 
        /// </summary>
        public WeightedList<MarkovChainNode<T>>.ReadOnly FirstList { get; private set; }

        /// <summary>
        /// Uses the specified collection to compute probabilities of successive values for the Markov chain.
        /// </summary>
        /// <param name="collection">The collection whose elements will be used to compute probabilities for the Markov chain.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        public void ComputePriorities(IEnumerable<T> collection)
        {
            lock (_lock)
            {
                if (collection == null) throw new ArgumentNullException("collection");

                MarkovChainNode<T> prevNode = null;
                int curLen = 0;

                foreach (T curValue in collection)
                {
                    curLen++;
                    ChainValue curChainValue = new ChainValue(curValue);

                    MarkovChainNode<T> curNode = AddOrIncrementPriority(curChainValue, _itemDict, _items);

                    if (prevNode == null)
                    {
                        int firstDex;
                        AddOrIncrementPriority(curChainValue, _firstDict, _firsts, GetNode, out firstDex);
                    }
                    else
                        prevNode.SetNext(curChainValue);

                    var someNode = GetNode(curChainValue);

                    prevNode = curNode;
                }

                AddOrIncrementPriority(curLen, _lengthDict, _lengths);

                if (prevNode == null)
                {
                    AddOrIncrementPriority(default(ChainValue), _firstDict, _firsts);
                    AddOrIncrementPriority(default(ChainValue), _itemDict, _items);
                }
                else prevNode.SetNext(default(ChainValue));
            }
        }

        private object _lock = new object();

        #region Generate Chain
        private void _checkInitialized()
        {
            if (_firsts.Count == 0)
                throw new InvalidOperationException("The current instance has not been initialized.");
        }

        /// <summary>
        /// Returns a randomly-generated Markov chain using the specified random number generator.
        /// This method continues until it reaches a node which has <see cref="MarkovChainNode{T}.IsEnd"/> set to <see langword="true"/>.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="strictStart">If <see langword="true"/>, starts with an element in <see cref="FirstList"/>.
        /// Otherwise, starts with any element in <see cref="ItemList"/>.</param>
        /// <returns>An array of a random length, containing a random sequence in which each element depends solely on the immediately previous element.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The current instance has not been initialized with any data.
        /// </exception>
        public T[] GenerateChainAnyLength(Random generator, bool strictStart)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            lock (_lock)
            {
                _checkInitialized();
#if !NOCONTRACT
                Contract.EndContractBlock();
#endif
                List<T> getList = new List<T>();
                MarkovChainNode<T> curNode = _firsts.GetRandomValue(generator);

                while (!curNode.IsEnd)
                {
                    getList.Add(curNode.Value);

                    curNode = curNode.NextListField.GetRandomValue(generator);
                }

                return getList.ToArray();
            }
        }

        /// <summary>
        /// Returns a randomly-generated markov chain using the specified random number generator.
        /// This method starts with an element in <see cref="FirstList"/> and continues 
        /// until it reaches a node which has <see cref="MarkovChainNode{T}.IsEnd"/> set to <see langword="true"/>.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <returns>An array containing elements of a random length.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The current instance has not been initialized with any data.
        /// </exception>
        public T[] GenerateChainAnyLength(Random generator)
        {
            return GenerateChainAnyLength(generator, true);
        }

        private T[] _generateChain(Random generator, int length, bool strictStart)
        {
            T[] result = new T[length];

            if (length == 0)
                return result;

            WeightedList<MarkovChainNode<T>> firstItems, allItems;
            if (strictStart)
            {
                firstItems = _firsts.FindAll(NotEnd);
                allItems = null;
            }
            else firstItems = allItems = _items.FindAll(NotEnd);

            MarkovChainNode<T> curNode = firstItems.GetRandomValue(generator);

            result[0] = curNode.Value;

            for (int i = 1; i < length; i++)
            {
                if (curNode.NextListNoEnd.Count == 1 && curNode.NextListNoEnd[0].Value.IsEnd)
                {
                    if (allItems == null) allItems = _items.FindAll(NotEnd);
                    curNode = allItems.GetRandomValue(generator);
                }
                else curNode = curNode.NextListNoEnd.GetRandomValue(generator);

                result[i] = curNode.Value;
            }

            return result;
        }

        private static bool NotEnd(MarkovChainNode<T> value)
        {
            return !value.IsEnd;
        }

        /// <summary>
        /// Returns a randomly-generated Markov chain of the specified length using the specified random number generator.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="length">The number of elements in the Markov chain.</param>
        /// <param name="strictStart">If <see langword="true"/>, starts with an element in <see cref="FirstList"/>.
        /// Otherwise, starts with any element in <see cref="ItemList"/>.</param>
        /// <returns>An array with <paramref name="length"/> elements, containing a random sequence in which each element
        /// depends solely on the immediately previous element.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// In a set operation, <paramref name="length"/> is less than 0.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The current instance has not been initialized with any data.
        /// </exception>
        public T[] GenerateChain(Random generator, int length, bool strictStart)
        {
            if (generator == null) throw new ArgumentNullException("generator");

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length",
#if !NOARGRANGE3
                    length,
#endif
                        "The specified length is less than 0.");
            }

            lock (_lock)
            {
                _checkInitialized();
#if !NOCONTRACT
                Contract.EndContractBlock();
#endif
                return _generateChain(generator, length, strictStart);
            }
        }

        /// <summary>
        /// Returns a randomly-generated Markov chain of the specified length using the specified random number generator,
        /// starting with an element in <see cref="FirstList"/>.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="length">The number of elements in the Markov chain.</param>
        /// <returns>An array with <paramref name="length"/> elements, containing a random sequence in which each element
        /// depends solely on the immediately previous element.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// In a set operation, <paramref name="length"/> is less than 0.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The current instance has not been initialized with any data.
        /// </exception>
        public T[] GenerateChain(Random generator, int length)
        {
            return GenerateChain(generator, length, true);
        }

        /// <summary>
        /// Returns a randomly-generated Markov chain using the specified random number generator.
        /// This method selects a length from <see cref="Lengths"/>.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="strictStart">If <see langword="true"/>, starts with an element in <see cref="FirstList"/>.
        /// Otherwise, starts with any element in <see cref="ItemList"/>.</param>
        /// <returns>An array of a random length selected from <see cref="Lengths"/>, containing a random sequence in which each element
        /// depends solely on the immediately previous element.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The current instance has not been initialized with any data.
        /// </exception>
        public T[] GenerateChainExistingLength(Random generator, bool strictStart)
        {
            if (generator == null) throw new ArgumentNullException("generator");

            lock (_lock)
            {
                _checkInitialized();
#if !NOCONTRACT
                Contract.EndContractBlock();
#endif
                return _generateChain(generator, _lengths.GetRandomValue(generator), strictStart);
            }
        }

        /// <summary>
        /// Returns a randomly-generated Markov chain using the specified random number generator.
        /// This method selects a length from <see cref="Lengths"/>, and starts with an item selected from <see cref="FirstList"/>
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <returns>An array of a random length selected from <see cref="Lengths"/>, containing a random sequence in which each element
        /// depends solely on the immediately previous element.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The current instance has not been initialized with any data.
        /// </exception>
        public T[] GenerateChainExistingLength(Random generator)
        {
            return GenerateChainExistingLength(generator, true);
        }
        #endregion

        internal MarkovChainNode<T> GetNode(ChainValue value)
        {
            return _items[_itemDict[value]].Value;
        }

        #region AddOrIncrementPriority(...)
        private MarkovChainNode<T> AddOrIncrementPriority(ChainValue value, Dictionary<ChainValue, int> dictionary, WeightedList<MarkovChainNode<T>> list)
        {
            int curDex;
            return AddOrIncrementPriority(value, dictionary, list, n => new MarkovChainNode<T>(n, this), out curDex);
        }

        internal static TDestination AddOrIncrementPriority<TSource, TDestination>(TSource value, Dictionary<TSource, int> dictionary, WeightedList<TDestination> list,
            Converter<TSource, TDestination> converter, out int curDex)
        {
            PriorityNode<TDestination> curNode;

            if (dictionary.TryGetValue(value, out curDex))
            {
                curNode = list[curDex];
                curNode.Priority++;
                list[curDex] = curNode;
                return curNode.Value;
            }
            curDex = list.Count;
            dictionary.Add(value, curDex);
            TDestination resultVal = converter(value);
            list.Add(resultVal, 1);
            return resultVal;
        }

        internal static void AddOrIncrementPriority<TValue>(TValue value, Dictionary<TValue, int> dictionary, WeightedList<TValue> list)
        {
            int curDex;
            AddOrIncrementPriority(value, dictionary, list, i => i, out curDex);
        }
        #endregion

        internal struct ChainValue : IEquatable<ChainValue>
        {
            internal ChainValue(T value)
            {
                _value = value;
                _notEnd = true;
            }

            private readonly bool _notEnd;
            public bool IsEnd { get { return !_notEnd; } }

            private readonly T _value;
            public T Value
            {
                get { return _value; }
            }

            public override string ToString()
            {
                if (_notEnd)
                    return string.Concat('{', _value, '}');

                return "[end]";
            }

            public bool Equals(ChainValue other)
            {
                return Equals(other, EqualityComparer<T>.Default);
            }

            internal bool Equals(ChainValue other, IEqualityComparer<T> comparer)
            {
                if (!_notEnd)
                    return !other._notEnd;

                return other._notEnd && comparer.Equals(_value, other._value);
            }

            public override bool Equals(object obj)
            {
                return obj is ChainValue && Equals((ChainValue)obj);
            }

            public override int GetHashCode()
            {
                if (!_notEnd)
                    return -1;

                if (_value == null) return 0;
                return _value.GetHashCode();
            }

            internal class Comparer : IEqualityComparer<ChainValue>, IEquatable<Comparer>
            {
                internal Comparer(IEqualityComparer<T> comparer)
                {
                    if (comparer == null)
                        _comparer = EqualityComparer<T>.Default;
                    else
                        _comparer = comparer;
                }

                private IEqualityComparer<T> _comparer;
                public IEqualityComparer<T> EqualityComparer { get { return _comparer; } }

                public bool Equals(ChainValue x, ChainValue y)
                {
                    return x.Equals(y, _comparer);
                }

                public int GetHashCode(ChainValue obj)
                {
                    if (obj._notEnd)
                    {
                        try
                        {
                            return _comparer.GetHashCode(obj._value);
                        }
                        catch (ArgumentNullException)
                        {
                            return 0;
                        }
                    }

                    return -1;
                }

                public bool Equals(Comparer other)
                {
                    return _comparer.Equals(other._comparer);
                }

                public override bool Equals(object obj)
                {
                    return Equals(obj as Comparer);
                }

                public override int GetHashCode()
                {
                    return "CoMp".GetHashCode() ^ _comparer.GetHashCode();
                }
            }
        }
    }

#if PCL
    internal delegate TDestination Converter<TSource, TDestination>(TSource source);
#endif

    /// <summary>
    /// Contains a value in a Markov chain, and all possible subsequent values.
    /// </summary>
    /// <typeparam name="T">The type of the objects in the Markov chain.</typeparam>
    public class MarkovChainNode<T>
    {
        internal MarkovChainNode(MarkovChain<T>.ChainValue value, MarkovChain<T> chainGenerator)
        {
            _value = value;
            _gen = chainGenerator;
            NextListDict = new Dictionary<MarkovChain<T>.ChainValue, int>(chainGenerator.ChainNodeComparer);
            NextListField = new WeightedList<MarkovChainNode<T>>();
            NextListNoEnd = new WeightedList<MarkovChainNode<T>>();
            NextList = NextListField.AsReadOnly();
        }

        internal MarkovChainNode(MarkovChain<T> chainGenerator)
            : this(default(MarkovChain<T>.ChainValue), chainGenerator)
        {
        }

        private readonly MarkovChain<T> _gen;
        /// <summary>
        /// Gets the Markov chain generator associated with the current instance.
        /// </summary>
        public MarkovChain<T> MarkovChainGenerator { get { return _gen; } }

        private readonly MarkovChain<T>.ChainValue _value;
        /// <summary>
        /// Gets the value associated with the current instance, or the default value for type <typeparamref name="T"/>
        /// if <see cref="IsEnd"/> is <see langword="true"/>.
        /// </summary>
        public T Value { get { return _value.Value; } }

        /// <summary>
        /// Gets a value indicating whether the current node indicates the end of a sequence.
        /// </summary>
        public bool IsEnd { get { return _value.IsEnd; } }

        internal readonly Dictionary<MarkovChain<T>.ChainValue, int> NextListDict;
        internal readonly WeightedList<MarkovChainNode<T>> NextListField, NextListNoEnd;
        /// <summary>
        /// Gets a read-only list containing all possible subsequent values.
        /// </summary>
        public WeightedList<MarkovChainNode<T>>.ReadOnly NextList { get; private set; }

        /// <summary>
        /// Returns a string representation of the current instance.
        /// </summary>
        /// <returns>A string representation of the current instance.</returns>
        public override string ToString()
        {
            return _value.ToString();
        }

        internal void SetNext(MarkovChain<T>.ChainValue nextVal)
        {
            int index;
            var node = MarkovChain<T>.AddOrIncrementPriority(nextVal, NextListDict, NextListField, _gen.GetNode, out index);
            if (index >= NextListNoEnd.Count)
                NextListNoEnd.Add(node, node.IsEnd ? 0 : 1);
            else if (!node.IsEnd)
            {
                double priority = NextListField.GetPriorityAt(index);
                NextListNoEnd.SetPriorityAt(index, priority);
            }
        }
    }
}
