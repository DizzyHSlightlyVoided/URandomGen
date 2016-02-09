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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
#if !NOCONTRACT
using System.Diagnostics.Contracts;
#endif
#if NOFIND
using System.Linq;
#endif

namespace URandomGen
{
    /// <summary>
    /// A list of weighted values.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(WeightedList<>.DebugView))]
    public class WeightedList<T> : IList<PriorityNode<T>>, IList
#if IREADONLY
        , IReadOnlyList<PriorityNode<T>>
#endif
    {
        private List<PriorityNode<T>> _list;

        /// <summary>
        /// Creates a new empty list with the default initial capacity.
        /// </summary>
        public WeightedList()
        {
            _list = new List<PriorityNode<T>>();
        }

        /// <summary>
        /// Creates a new empty list with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The initial number of elements the list can contain.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="capacity"/> is less than 0.
        /// </exception>
        public WeightedList(int capacity)
        {
            _list = new List<PriorityNode<T>>(capacity);
        }

        /// <summary>
        /// Creates a new list containing elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">A collection containing elements to copy to the new list.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        public WeightedList(IEnumerable<PriorityNode<T>> collection)
        {
            _list = new List<PriorityNode<T>>(collection);
        }


        /// <summary>
        /// Gets and sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or is greater than or equal to <see cref="Count"/>.
        /// </exception>
        public PriorityNode<T> this[int index]
        {
            get { return _list[index]; }
            set { _list[index] = value; }
        }

        object IList.this[int index]
        {
            get { return _list[index]; }
            set { ((IList)_list)[index] = value; }
        }

        /// <summary>
        /// Gets and sets the total number of elements the collection can contain without resizing.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// In a set operation, the specified value is less than <see cref="Count"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// In a set operation, there is not enough memory to allocate the specified size.
        /// </exception>
        public int Capacity
        {
            get { return _list.Capacity; }
            set { _list.Capacity = value; }
        }

        /// <summary>
        /// Gets the number of elements contained in the list.
        /// </summary>
        public int Count { get { return _list.Count; } }

        /// <summary>
        /// Sets <see cref="Capacity"/> equal to <see cref="Count"/>, if <see cref="Count"/> is less than 90% of <see cref="Capacity"/>.
        /// </summary>
        public void TrimExcess()
        {
            _list.TrimExcess();
        }


        /// <summary>
        /// Adds the specified node to the list.
        /// </summary>
        /// <param name="node">The node to add to the list.</param>
        public void Add(PriorityNode<T> node)
        {
            _list.Add(node);
        }

        int IList.Add(object o)
        {
            return ((IList)_list).Add(o);
        }

        /// <summary>
        /// Adds the specified value to the list with the specified priority.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <param name="priority">Indicates the priority of the value. Values less than or equal to 0 are ignored.</param>
        public void Add(T value, double priority)
        {
            _list.Add(new PriorityNode<T>(value, priority));
        }

        /// <summary>
        /// Inserts the specified node into the list at the specified index.
        /// </summary>
        /// <param name="index">The index at which the value will be inserted.</param>
        /// <param name="node">The node to add to the list.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or is greater than <see cref="Count"/>.
        /// </exception>
        public void Insert(int index, PriorityNode<T> node)
        {
            _list.Insert(index, node);
        }

        /// <summary>
        /// Inserts the specified node into the list with the specified value at the specified index.
        /// </summary>
        /// <param name="index">The index at which the value will be inserted.</param>
        /// <param name="value">The value to add.</param>
        /// <param name="priority">Indicates the priority of the value. Values less than or equal to 0 are ignored.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or is greater than <see cref="Count"/>.
        /// </exception>
        public void Insert(int index, T value, double priority)
        {
            _list.Insert(index, new PriorityNode<T>(value, priority));
        }

        void IList.Insert(int index, object value)
        {
            ((IList)_list).Insert(index, value);
        }

        /// <summary>
        /// Removes the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or is greater than or equal to <see cref="Count"/>.
        /// </exception>
        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        /// <summary>
        /// Removes the first instance of the specified node in the list.
        /// </summary>
        /// <param name="node">The node to search for in the list.</param>
        /// <returns><see langword="true"/> if a node with the same value and priority as <paramref name="node"/>
        /// was found and successfully removed; <see langword="false"/> otherwise.</returns>
        public bool Remove(PriorityNode<T> node)
        {
            return _list.Remove(node);
        }

        /// <summary>
        /// Removes the first instance of the specified node in the list.
        /// </summary>
        /// <param name="value">The node to search for in the list.</param>
        /// <param name="priority">The priority of the node to search for in the list.</param>
        /// <returns><see langword="true"/> if a node with the same value and priority as <paramref name="value"/> and <paramref name="priority"/>
        /// was found and successfully removed; <see langword="false"/> otherwise.</returns>
        public bool Remove(T value, double priority)
        {
            return _list.Remove(new PriorityNode<T>(value, priority));
        }

        /// <summary>
        /// Removes the first instance of the specified node in the list.
        /// </summary>
        /// <param name="value">The node to search for in the list.</param>
        /// <returns><see langword="true"/> if a node with the same value as <paramref name="value"/>
        /// was found and successfully removed; <see langword="false"/> otherwise.</returns>
        public bool Remove(T value)
        {
            int dex = IndexOf(value);
            if (dex < 0) return false;
            RemoveAt(dex);
            return true;
        }

        void IList.Remove(object value)
        {
            ((IList)_list).Remove(value);
        }

        /// <summary>
        /// Removes all elements from the list.
        /// </summary>
        public void Clear()
        {
            _list.Clear();
        }

        /// <summary>
        /// Sets the priority of the node at the specified index.
        /// </summary>
        /// <param name="index">The index of the priority to set.</param>
        /// <param name="priority">The priority of the node to set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than 0 or is greater than or equal to <see cref="Count"/>.</para>
        /// <para>-OR-</para>
        /// <para><paramref name="priority"/> is less than 0.</para>
        /// </exception>
        public void SetPriorityAt(int index, double priority)
        {
            var node = _list[index];
            PriorityNode<T>.PriorityCheck(priority, "priority");
            node.Priority = priority;
            _list[index] = node;
        }

        /// <summary>
        /// Sets the value of the node at the specified index.
        /// </summary>
        /// <param name="index">The index of the value to set.</param>
        /// <param name="value">The value of the node to set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or is greater than or equal to <see cref="Count"/>.
        /// </exception>
        public void SetValueAt(int index, T value)
        {
            var node = _list[index];
            node.Value = value;
            _list[index] = node;
        }

        /// <summary>
        /// Determines if the specified node exists in the list.
        /// </summary>
        /// <param name="node">The node to search for in the list.</param>
        /// <returns><see langword="true"/> if a node with the same value and priority as <paramref name="node"/>
        /// was found; <see langword="false"/> otherwise.</returns>
        public bool Contains(PriorityNode<T> node)
        {
            return _list.Contains(node);
        }

        /// <summary>
        /// Determines if the specified node exists in the list.
        /// </summary>
        /// <param name="value">The value to search for in the list.</param>
        /// <param name="priority">The priority of the node to search for in the list.</param>
        /// <returns><see langword="true"/> if a node with the same value and priority as <paramref name="value"/> and <paramref name="priority"/>
        /// was found; <see langword="false"/> otherwise.</returns>
        public bool Contains(T value, double priority)
        {
            return _list.Contains(new PriorityNode<T>(value, priority));
        }

        /// <summary>
        /// Determines if the specified value exists in the list.
        /// </summary>
        /// <param name="value">The value to search for in the list.</param>
        /// <returns><see langword="true"/> if a node with the same value as <paramref name="value"/>
        /// was found; <see langword="false"/> otherwise.</returns>
        public bool Contains(T value)
        {
            return IndexOf(value) >= 0;
        }

        bool IList.Contains(object value)
        {
            return ((IList)_list).Contains(value);
        }

        /// <summary>
        /// Returns the index of the specified node.
        /// </summary>
        /// <param name="node">The node to search for in the list.</param>
        /// <returns>The index of the first occurrence of a node with the same value and weight as <paramref name="node"/>
        /// within the list, if found; otherwise, -1.</returns>
        public int IndexOf(PriorityNode<T> node)
        {
            return _list.IndexOf(node);
        }

        int IList.IndexOf(object value)
        {
            return ((IList)_list).IndexOf(value);
        }

        /// <summary>
        /// Returns the index of the specified node.
        /// </summary>
        /// <param name="value">The value to search for in the list.</param>
        /// <param name="priority">The priority of the value to search for in the list.</param>
        /// <returns>The index of the first occurrence of a node with the same value and weight as <paramref name="value"/> and <paramref name="priority"/>
        /// within the list, if found; otherwise, -1.</returns>
        public int IndexOf(T value, double priority)
        {
            return _list.IndexOf(new PriorityNode<T>(value, priority));
        }

        /// <summary>
        /// Returns the index of the specified value.
        /// </summary>
        /// <param name="value">The value to search for in the list.</param>
        /// <returns>The index of the first occurrence of a node with the same value as <paramref name="value"/>
        /// within the list, if found; otherwise, -1.</returns>
        public int IndexOf(T value)
        {
#if NOFIND
            for (int i = 0; i < _list.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(_list[i].Value, value))
                    return i;
            }
            return -1;
#else
            return _list.FindIndex(i => EqualityComparer<T>.Default.Equals(value, i.Value));
#endif
        }

        /// <summary>
        /// Copies all elements in the list to the specified array.
        /// </summary>
        /// <param name="array">The array to which the current list will be copied.</param>
        /// <exception cref="ArgumentException">
        /// The number of elements in <paramref name="array"/> is less than <see cref="Count"/>.
        /// </exception>
        public void CopyTo(PriorityNode<T>[] array)
        {
            _list.CopyTo(array);
        }

        /// <summary>
        /// Copies all elements in the list to the specified array.
        /// </summary>
        /// <param name="array">The array to which the current list will be copied.</param>
        /// <param name="arrayIndex">The index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="arrayIndex"/> plus <see cref="Count"/> is greater than the length of <paramref name="array"/>.
        /// </exception>
        public void CopyTo(PriorityNode<T>[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Copies a range of elements in the list to the specified array.
        /// </summary>
        /// <param name="index">The index in the current list at which copying begins.</param>
        /// <param name="array">The array to which the current list will be copied.</param>
        /// <param name="arrayIndex">The index in <paramref name="array"/> at which copying begins.</param>
        /// <param name="count">The number of elements to copy.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/>, <paramref name="arrayIndex"/>, or <paramref name="count"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="arrayIndex"/> plus <paramref name="count"/> is greater than the length of <paramref name="array"/>.</para>
        /// <para>-OR-</para>
        /// <para><paramref name="arrayIndex"/> plus <paramref name="count"/> is greater than <see cref="Count"/>.</para>
        /// </exception>
        public void CopyTo(int index, PriorityNode<T>[] array, int arrayIndex, int count)
        {
            _list.CopyTo(index, array, arrayIndex, count);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((IList)_list).CopyTo(array, index);
        }

        /// <summary>
        /// Returns an array containing elements copied from the current list.
        /// </summary>
        /// <returns>An array containing elements copied from the current list.</returns>
        public PriorityNode<T>[] ToArray()
        {
            return _list.ToArray();
        }

        /// <summary>
        /// Returns a read-only wrapper for the current list.
        /// </summary>
        /// <returns>A read-only wrapper for the current list.</returns>
        public ReadOnly AsReadOnly()
        {
            return new ReadOnly(this);
        }

        /// <summary>
        /// Returns a random index from the current instance. Elements with a higher <see cref="PriorityNode{T}.Priority"/> value
        /// will have a higher probability; elements with a <see cref="PriorityNode{T}.Priority"/> less than or equal to 0 will be ignored.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <returns>The index of a random element in the current list.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The current list does not contain any elements with a <see cref="PriorityNode{T}.Priority"/> value greater than 0.
        /// </exception>
        public int GetRandomIndex(Random generator)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            try
            {
                return RandomGen._randomIndex(generator.NextDouble, this, null);
            }
            catch (ArgumentException x)
            {
                throw new InvalidOperationException(x.Message);
            }
        }

        /// <summary>
        /// Returns a random value from the current instance. Elements with a higher <see cref="PriorityNode{T}.Priority"/> value
        /// will have a higher probability; elements with a <see cref="PriorityNode{T}.Priority"/> less than or equal to 0 will be ignored.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <returns>A random value in the current list.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The current list does not contain any elements with a <see cref="PriorityNode{T}.Priority"/> value greater than 0.
        /// </exception>
        public T GetRandomValue(Random generator)
        {
            return _list[GetRandomIndex(generator)].Value;
        }

        #region Find ...
        private static Predicate<PriorityNode<T>> _predicator(Predicate<T> match)
        {
            if (match == null) return null;

            return i => match(i.Value);
        }

        /// <summary>
        /// Searches for a node matching the conditions defined by the specified predicate,
        /// and returns the first occurrence of the node in the entire list.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the element to search for.</param>
        /// <returns>The first occurrence in the list of a node matching <paramref name="match"/>, if found; otherwise, the default value
        /// for type <see cref="PriorityNode{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        public PriorityNode<T> Find(Predicate<PriorityNode<T>> match)
        {
#if NOFIND
            int dex = FindIndex(match);
            if (dex < 0)
                return default(PriorityNode<T>);
            return _list[dex];
#else
            return _list.Find(match);
#endif
        }

        /// <summary>
        /// Searches for a value matching the conditions defined by the specified predicate,
        /// and returns the first node with a matching value in the entire list.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <typeparamref name="T"/> defining the element to search for.</param>
        /// <returns>The first occurrence in the list of a node matching <paramref name="match"/>, if found; otherwise, the default value
        /// for type <see cref="PriorityNode{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        public PriorityNode<T> Find(Predicate<T> match)
        {
            return Find(_predicator(match));
        }

        /// <summary>
        /// Searches for a node matching the conditions defined by the specified predicate,
        /// and returns the last occurrence of the node in the entire list.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the element to search for.</param>
        /// <returns>The last occurrence in the list of a node matching <paramref name="match"/>, if found; otherwise, the default value
        /// for type <see cref="PriorityNode{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        public PriorityNode<T> FindLast(Predicate<PriorityNode<T>> match)
        {
#if NOFIND
            int dex = FindLastIndex(match);
            if (dex < 0)
                return default(PriorityNode<T>);
            return _list[dex];
#else
            return _list.FindLast(match);
#endif
        }

        /// <summary>
        /// Searches for a value matching the conditions defined by the specified predicate,
        /// and returns the last node with a matching value in the entire list.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <typeparamref name="T"/> defining the element to search for.</param>
        /// <returns>The last occurrence in the list of a node matching <paramref name="match"/>, if found; otherwise, the default value
        /// for type <see cref="PriorityNode{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        public PriorityNode<T> FindLast(Predicate<T> match)
        {
            return FindLast(_predicator(match));
        }

        /// <summary>
        /// Searches for a node matching the conditions defined by the specified predicate,
        /// and returns the zero-based index of the first occurrence of the node in the entire list.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence in the list of a node matching <paramref name="match"/>, if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        public int FindIndex(Predicate<PriorityNode<T>> match)
        {
#if NOFIND
            if (match == null)
                throw new ArgumentNullException("match");
            Contract.EndContractBlock();

            for (int i = 0; i < _list.Count; i++)
            {
                if (match(_list[i]))
                    return i;
            }
            return -1;
#else
            return _list.FindIndex(match);
#endif
        }

        /// <summary>
        /// Searches for a value matching the conditions defined by the specified predicate,
        /// and returns the zero-based index of the first occurrence of the node in the entire list.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence in the list of a node matching <paramref name="match"/>, if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        public int FindIndex(Predicate<T> match)
        {
            return FindIndex(_predicator(match));
        }

#if NOFIND
        private int FindIndex(Predicate<PriorityNode<T>> match, int startIndex, int end)
        {
            if (match == null)
                throw new ArgumentNullException("match");
            Contract.EndContractBlock();

            for (int i = startIndex; i < end; i++)
            {
                if (match(_list[i]))
                    return i;
            }
            return -1;
        }
#endif

        /// <summary>
        /// Searches for a node matching the conditions defined by the specified predicate,
        /// and returns the zero-based index of the first occurrence of the node in the list, starting with the specified index.
        /// </summary>
        /// <param name="startIndex">The zero-based index of the first element to search.</param>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence in the list of a node matching <paramref name="match"/>, if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is less than 0 or is greater than or equal to <see cref="Count"/>.
        /// </exception>
        public int FindIndex(int startIndex, Predicate<PriorityNode<T>> match)
        {
#if NOFIND
            if (startIndex < 0 || startIndex >= _list.Count)
                throw new ArgumentOutOfRangeException("startIndex");

            return FindIndex(match, startIndex, _list.Count);
#else
            return _list.FindIndex(startIndex, match);
#endif
        }

        /// <summary>
        /// Searches for a value matching the conditions defined by the specified predicate,
        /// and returns the zero-based index of the first occurrence of the node in the list, starting with the specified index.
        /// </summary>
        /// <param name="startIndex">The zero-based index of the first element to search.</param>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence in the list of a node matching <paramref name="match"/>, if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is less than 0 or is greater than or equal to <see cref="Count"/>.
        /// </exception>
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return FindIndex(startIndex, _predicator(match));
        }

        /// <summary>
        /// Searches for a node matching the conditions defined by the specified predicate,
        /// and returns the zero-based index of the first occurrence of the node in the specified range of elements in the list.
        /// </summary>
        /// <param name="startIndex">The zero-based index of the first element to search.</param>
        /// <param name="count">The number of elements to search.</param>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence in the list of a node matching <paramref name="match"/>, if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> and <paramref name="count"/> do not specify a valid range of elements in the current list.
        /// </exception>
        public int FindIndex(int startIndex, int count, Predicate<PriorityNode<T>> match)
        {
#if NOFIND
            if (startIndex < 0 || startIndex >= _list.Count)
                throw new ArgumentOutOfRangeException("startIndex");
            int end = startIndex + count;
            if (end > _list.Count)
                throw new ArgumentOutOfRangeException(null, "The specified values do not specify a valid range of elements for the search.");

            return FindIndex(match, startIndex, end);
#else
            return _list.FindIndex(startIndex, count, match);
#endif
        }

        /// <summary>
        /// Searches for a value matching the conditions defined by the specified predicate,
        /// and returns the zero-based index of the first occurrence of the node in the list, starting with the specified index.
        /// </summary>
        /// <param name="startIndex">The zero-based index of the first element to search.</param>
        /// <param name="count">The number of elements to search.</param>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence in the list of a node matching <paramref name="match"/>, if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is less than 0 or is greater than or equal to <see cref="Count"/>.
        /// </exception>
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return FindIndex(startIndex, count, _predicator(match));
        }

        /// <summary>
        /// Searches for a node matching the conditions defined by the specified predicate,
        /// and returns the zero-based index of the last occurrence of the node in the entire list.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the element to search for.</param>
        /// <returns>The zero-based index of the last occurrence in the list of a node matching <paramref name="match"/>, if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        public int FindLastIndex(Predicate<PriorityNode<T>> match)
        {
#if NOFIND
            if (match == null)
                throw new ArgumentNullException("match");

            for (int i = _list.Count - 1; i >= 0; i--)
            {
                if (match(_list[i]))
                    return i;
            }
            return -1;
#else
            return _list.FindLastIndex(match);
#endif
        }

        /// <summary>
        /// Searches for a value matching the conditions defined by the specified predicate,
        /// and returns the zero-based index of the last occurrence of the node in the entire list.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the element to search for.</param>
        /// <returns>The zero-based index of the last occurrence in the list of a node matching <paramref name="match"/>, if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        public int FindLastIndex(Predicate<T> match)
        {
            return FindLastIndex(_predicator(match));
        }

#if NOFIND
        private int FindLastIndex(Predicate<PriorityNode<T>> match, int startIndex, int end)
        {
            if (match == null)
                throw new ArgumentNullException("match");
            Contract.EndContractBlock();

            for (int i = startIndex; i >= end; i--)
            {
                if (match(_list[i]))
                    return i;
            }
            return -1;
        }
#endif

        /// <summary>
        /// Searches for a node matching the conditions defined by the specified predicate,
        /// and returns the zero-based index of the last occurrence of the node in the list, starting backward from with the specified index.
        /// </summary>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the element to search for.</param>
        /// <returns>The zero-based index of the last occurrence in the list of a node matching <paramref name="match"/>, if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is less than 0 or is greater than or equal to <see cref="Count"/>.
        /// </exception>
        public int FindLastIndex(int startIndex, Predicate<PriorityNode<T>> match)
        {
#if NOFIND
            if (startIndex < 0 || startIndex >= _list.Count)
                throw new ArgumentOutOfRangeException("startIndex");

            return FindLastIndex(match, startIndex, 0);
#else
            return _list.FindLastIndex(startIndex, match);
#endif
        }

        /// <summary>
        /// Searches for a value matching the conditions defined by the specified predicate,
        /// and returns the zero-based index of the last occurrence of the node in the list, starting backward from with the specified index.
        /// </summary>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the element to search for.</param>
        /// <returns>The zero-based index of the last occurrence in the list of a node matching <paramref name="match"/>, if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is less than 0 or is greater than or equal to <see cref="Count"/>.
        /// </exception>
        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return FindLastIndex(startIndex, _predicator(match));
        }

        /// <summary>
        /// Searches for a node matching the conditions defined by the specified predicate,
        /// and returns the zero-based index of the last occurrence of the node in the specified range of elements in the list.
        /// </summary>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements to search.</param>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the element to search for.</param>
        /// <returns>The zero-based index of the last occurrence in the list of a node matching <paramref name="match"/>, if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> and <paramref name="count"/> do not specify a valid range of elements in the current list.
        /// </exception>
        public int FindLastIndex(int startIndex, int count, Predicate<PriorityNode<T>> match)
        {
#if NOFIND
            if (startIndex < 0 || startIndex >= _list.Count)
                throw new ArgumentOutOfRangeException("startIndex");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            int end = startIndex - count;
            if (end < 0)
                throw new ArgumentOutOfRangeException("count", "The specified values do not specify a valid range of elements for the backward search.");

            return FindLastIndex(match, startIndex, end);
#else
            return _list.FindLastIndex(startIndex, count, match);
#endif
        }

        /// <summary>
        /// Searches for a value matching the conditions defined by the specified predicate,
        /// and returns the zero-based index of the last occurrence of the node in the list, starting backward from with the specified index.
        /// </summary>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements to search.</param>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the element to search for.</param>
        /// <returns>The zero-based index of the last occurrence in the list of a node matching <paramref name="match"/>, if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is less than 0 or is greater than or equal to <see cref="Count"/>.
        /// </exception>
        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            return FindLastIndex(startIndex, count, _predicator(match));
        }

        /// <summary>
        /// Searches for all nodes matching the specified predicate,
        /// and returns a list containing all matching nodes.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the elements to search for.</param>
        /// <returns>A list containing any and all nodes matched by <paramref name="match"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        public WeightedList<T> FindAll(Predicate<PriorityNode<T>> match)
        {
#if NOFIND
            if (match == null)
                throw new ArgumentNullException("match");
            Contract.EndContractBlock();

            var list = new WeightedList<T>(_list.Where(i => match(i)));
            list.TrimExcess();
            return list;
#else
            return new WeightedList<T>() { _list = _list.FindAll(match) };
#endif
        }

        /// <summary>
        /// Searches for all nodes matching the specified predicate,
        /// and returns a list containing all matching nodes.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <typeparamref name="T"/> defining the elements to search for.</param>
        /// <returns>A list containing any and all nodes matched by <paramref name="match"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        public WeightedList<T> FindAll(Predicate<T> match)
        {
            return FindAll(_predicator(match));
        }

        /// <summary>
        /// Determines if the list contains any nodes matching the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the elements to search for.</param>
        /// <returns><see langword="true"/> if at least one node matches <paramref name="match"/>; <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        public bool Exists(Predicate<PriorityNode<T>> match)
        {
#if NOFIND
            if (match == null)
                throw new ArgumentNullException("match");
            Contract.EndContractBlock();

            return _list.Any(i => match(i));
#else
            return _list.Exists(match);
#endif
        }

        /// <summary>
        /// Determines if the list contains any values matching the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <typeparamref name="T"/> defining the elements to search for.</param>
        /// <returns><see langword="true"/> if at least one value matches <paramref name="match"/>; <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        public bool Exists(Predicate<T> match)
        {
            return Exists(_predicator(match));
        }

        /// <summary>
        /// Determines if every node in the list matches the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <see cref="PriorityNode{T}"/> defining the elements to search for.</param>
        /// <returns><see langword="true"/> if every node in the list matches <paramref name="match"/>; <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        public bool TrueForAll(Predicate<PriorityNode<T>> match)
        {
#if NOFIND
            if (match == null)
                throw new ArgumentNullException("match");
            Contract.EndContractBlock();

            return !_list.Any(i => !match(i));
#else
            return _list.TrueForAll(match);
#endif
        }

        /// <summary>
        /// Determines if every value in the list matches the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> of type <typeparamref name="T"/> defining the elements to search for.</param>
        /// <returns><see langword="true"/> if every value in the list matches <paramref name="match"/>; <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        public bool TrueForAll(Predicate<T> match)
        {
            return TrueForAll(_predicator(match));
        }
        #endregion

        /// <summary>
        /// Returns an enumerator which iterates through the list.
        /// </summary>
        /// <returns>An enumerator which iterates through the list.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<PriorityNode<T>> IEnumerable<PriorityNode<T>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        bool ICollection<PriorityNode<T>>.IsReadOnly { get { return false; } }
        bool IList.IsFixedSize { get { return false; } }
        bool IList.IsReadOnly { get { return true; } }
        bool ICollection.IsSynchronized { get { return false; } }
        object ICollection.SyncRoot { get { return ((IList)_list).SyncRoot; } }

        /// <summary>
        /// An enumerator which iterates through the list.
        /// </summary>
        public struct Enumerator : IEnumerator<PriorityNode<T>>
        {
            private IEnumerator<PriorityNode<T>> _enum;
            private PriorityNode<T> _current;

            internal Enumerator(WeightedList<T> list)
            {
                _enum = list._list.GetEnumerator();
                _current = default(PriorityNode<T>);
            }

            /// <summary>
            /// Gets the element at the current position in the enumerator.
            /// </summary>
            public PriorityNode<T> Current
            {
                get { return _current; }
            }

            object IEnumerator.Current
            {
                get { return _current; }
            }

            /// <summary>
            /// Disposes of the current instance.
            /// </summary>
            public void Dispose()
            {
                if (_enum == null) return;
                _enum.Dispose();
                this = default(Enumerator);
            }

            /// <summary>
            /// Advances the enumerator to the next position in the list.
            /// </summary>
            /// <returns><see langword="true"/> if the enumerator was successfully advanced; <see langword="false"/> if the enumerator
            /// has passed the end of the list.</returns>
            public bool MoveNext()
            {
                if (_enum == null) return false;
                if (_enum.MoveNext())
                {
                    _current = _enum.Current;
                    return true;
                }
                Dispose();
                return false;
            }

            void IEnumerator.Reset()
            {
                throw new InvalidOperationException();
            }
        }

        private class DebugView
        {
            private WeightedList<T> _list;

            public DebugView(WeightedList<T> list)
            {
                _list = list;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public PriorityNode<T>[] Items { get { return _list.ToArray(); } }
        }

        /// <summary>
        /// A read-only wrapper for a <see cref="WeightedList{T}"/>.
        /// </summary>
        public class ReadOnly : ReadOnlyCollection<PriorityNode<T>>
        {
            private readonly WeightedList<T> _list;
            /// <summary>
            /// Creates a new instance using the specified list.
            /// </summary>
            /// <param name="list">The list to wrap.</param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="list"/> is <see langword="null"/>.
            /// </exception>
            public ReadOnly(WeightedList<T> list)
                : base(list)
            {
                _list = list;
            }

            /// <summary>
            /// Determines if the specified value exists in the list with the specified priority.
            /// </summary>
            /// <param name="value">The value to search for in the list.</param>
            /// <param name="priority">The priority of the node to search for in the list.</param>
            /// <returns><see langword="true"/> if a node with the same value and priority as <paramref name="value"/> and <paramref name="priority"/>
            /// was found; <see langword="false"/> otherwise.</returns>
            public bool Contains(T value, double priority)
            {
                return _list.Contains(value, priority);
            }

            /// <summary>
            /// Determines if the specified value exists in the list.
            /// </summary>
            /// <param name="value">The value to search for in the list.</param>
            /// <returns><see langword="true"/> if a node with the same value as <paramref name="value"/>
            /// was found; <see langword="false"/> otherwise.</returns>
            public bool Contains(T value)
            {
                return _list.Contains(value);
            }

            /// <summary>
            /// Returns the index of the specified node.
            /// </summary>
            /// <param name="value">The value to search for in the list.</param>
            /// <param name="priority">The priority of the value to search for in the list.</param>
            /// <returns>The index of the first occurrence of a node with the same value and weight as <paramref name="value"/> and <paramref name="priority"/>
            /// within the list, if found; otherwise, -1.</returns>
            public int IndexOf(T value, double priority)
            {
                return _list.IndexOf(value, priority);
            }

            /// <summary>
            /// Returns the index of the specified value.
            /// </summary>
            /// <param name="value">The value to search for in the list.</param>
            /// <returns>The index of the first occurrence of a node with the same value as <paramref name="value"/>
            /// within the list, if found; otherwise, -1.</returns>
            public int IndexOf(T value)
            {
                return _list.IndexOf(value);
            }

            /// <summary>
            /// Returns a random index from the current instance. Elements with a higher <see cref="PriorityNode{T}.Priority"/> value
            /// will have a higher probability; elements with a <see cref="PriorityNode{T}.Priority"/> less than or equal to 0 will be ignored.
            /// </summary>
            /// <param name="generator">The random number generator to use.</param>
            /// <returns>The index of a random element in the current list.</returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="generator"/> is <see langword="null"/>.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// The current list does not contain any elements with a <see cref="PriorityNode{T}.Priority"/> value greater than 0.
            /// </exception>
            public int GetRandomIndex(Random generator)
            {
                return _list.GetRandomIndex(generator);
            }

            /// <summary>
            /// Returns a random value from the current instance. Elements with a higher <see cref="PriorityNode{T}.Priority"/> value
            /// will have a higher probability; elements with a <see cref="PriorityNode{T}.Priority"/> less than or equal to 0 will be ignored.
            /// </summary>
            /// <param name="generator">The random number generator to use.</param>
            /// <returns>A random value in the current list.</returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="generator"/> is <see langword="null"/>.
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// The current list does not contain any elements with a <see cref="PriorityNode{T}.Priority"/> value greater than 0.
            /// </exception>
            public T GetRandomValue(Random generator)
            {
                return _list.GetRandomValue(generator);
            }
        }
    }

    /// <summary>
    /// Represents a priority/value pair.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public struct PriorityNode<T> : IEquatable<PriorityNode<T>>, IComparable<PriorityNode<T>>
    {
        /// <summary>
        /// Creates a new instance with the specified values.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="priority">Indicates the priority of the value. A priority of 0.0 will mean the current instance is ignored.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="priority"/> is less than 0.
        /// </exception>
        public PriorityNode(T value, double priority)
        {
            PriorityCheck(priority, "priority");
#if !NOCONTRACT
            Contract.EndContractBlock();
#endif
            _value = value;
            _priority = priority;
        }

        private T _value;
        /// <summary>
        /// The value of the current instance.
        /// </summary>
        public T Value
        {
            get { return _value; }
            set { _value = value; }
        }

        internal static void PriorityCheck(double priority, string paramName)
        {
            if (priority < 0)
                throw new ArgumentOutOfRangeException(paramName,
#if !NOARGRANGE3
                    priority,
#endif
                        "The specified value is less than 0.");

        }

        private double _priority;
        /// <summary>
        /// Gets and sets the priority of the current instance. A value of 0.0 will mean the current instance is ignored.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// In a set operation, the specified value is less than 0.
        /// </exception>
        public double Priority
        {
            get { return _priority; }
            set
            {
                PriorityCheck(value, "value");
#if !NOCONTRACT
                Contract.EndContractBlock();
#endif
                _priority = value;
            }
        }

        /// <summary>
        /// Compares the current value to the specified other <see cref="PriorityNode{T}"/> value.
        /// If <see cref="Value"/> implements <see cref="IComparable{T}"/> or <see cref="IComparable"/> for type
        /// <typeparamref name="T"/>, both <see cref="Priority"/> and <see cref="Value"/> will also be compared; otherwise,
        /// only <see cref="Priority"/> will be compared.
        /// </summary>
        /// <param name="other">The other <see cref="PriorityNode{T}"/> to compare.</param>
        /// <returns>A value less than 1 if the current instance is less than <paramref name="other"/>; a value greater than 1 if
        /// the current instance is greater than <paramref name="other"/>; or 0 if the current instance is equal to
        /// <paramref name="other"/>.</returns>
        public int CompareTo(PriorityNode<T> other)
        {
            int comp = Priority.CompareTo(other.Priority);
            if (comp != 0) return comp;

            if (Value is IComparable<T>)
                return ((IComparable<T>)Value).CompareTo(other.Value);

            if (Value is IComparable)
                return ((IComparable)Value).CompareTo(other.Value);

            if (other.Value is IComparable<T>)
                return -((IComparable<T>)other.Value).CompareTo(Value);

            if (other.Value is IComparable)
                return -((IComparable)other.Value).CompareTo(Value);

            return 0;
        }

        /// <summary>
        /// Returns a string representation of the current value.
        /// </summary>
        /// <returns>A string representation of the current value.</returns>
        public override string ToString()
        {
            return string.Concat(Priority, ", {", Value, "}");
        }

        /// <summary>
        /// Determines if the current value is equal to the specified other <see cref="PriorityNode{T}"/> value.
        /// </summary>
        /// <param name="other">The other <see cref="PriorityNode{T}"/> to compare.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>;
        /// <see langword="false"/> otherwise.</returns>
        public bool Equals(PriorityNode<T> other)
        {
            return Priority == other.Priority && EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        /// <summary>
        /// Determines if the current value is equal to the specified other object.
        /// </summary>
        /// <param name="obj">The other object to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="PriorityNode{T}"/> equal to the current value;
        /// <see langword="false"/> otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is PriorityNode<T> && Equals((PriorityNode<T>)obj);
        }

        /// <summary>
        /// Returns the hash code for the current value.
        /// </summary>
        /// <returns>The hash code for the current value.</returns>
        public override int GetHashCode()
        {
            int curHash = Priority.GetHashCode();

            if (Value == null) return curHash;
            return curHash + Value.GetHashCode();
        }

        /// <summary>
        /// Determines equality of two <see cref="PriorityNode{T}"/> objects.
        /// </summary>
        /// <param name="t1">A <see cref="PriorityNode{T}"/> to compare.</param>
        /// <param name="t2">A <see cref="PriorityNode{T}"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="t1"/> is equal to <paramref name="t2"/>;
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator ==(PriorityNode<T> t1, PriorityNode<T> t2)
        {
            return t1.Equals(t2);
        }

        /// <summary>
        /// Determines inequality of two <see cref="PriorityNode{T}"/> objects.
        /// </summary>
        /// <param name="t1">A <see cref="PriorityNode{T}"/> to compare.</param>
        /// <param name="t2">A <see cref="PriorityNode{T}"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="t1"/> is not equal to <paramref name="t2"/>;
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator !=(PriorityNode<T> t1, PriorityNode<T> t2)
        {
            return !t1.Equals(t2);
        }

        #region Get Comparer
        /// <summary>
        /// Returns a <see cref="Comparer{T}"/> implementation which compares only the <see cref="Value"/> of priority nodes.
        /// </summary>
        /// <param name="comparer">The existing comparer to use, or <see langword="null"/> to use the default comparer.</param>
        /// <returns>A <see cref="PriorityNode{T}"/> comparer which compares only the values.</returns>
        public Comparer<PriorityNode<T>> GetComparer(IComparer<T> comparer)
        {
            return new ValueComparer(comparer);
        }

        /// <summary>
        /// Returns a <see cref="Comparer{T}"/> implementation which compares only the <see cref="Value"/> of priority nodes,
        /// using the specified <see cref="Comparison{T}"/>.
        /// </summary>
        /// <param name="comparison">The <see cref="Comparison{T}"/> to use.</param>
        /// <returns>A <see cref="PriorityNode{T}"/> comparer which compares only the values.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="comparison"/> is <see langword="null"/>.
        /// </exception>
        public Comparer<PriorityNode<T>> GetComparer(Comparison<T> comparison)
        {
#if NOCOMPARECREATE
            if (comparison == null)
                throw new ArgumentNullException("comparison");
#if !NOCONTRACT
            Contract.EndContractBlock();
#endif
            return new ValueComparer(comparison);
#else
            return new ValueComparer(Comparer<T>.Create(comparison));
#endif
        }

        private class ValueComparer : Comparer<PriorityNode<T>>, IEquatable<ValueComparer>
        {
            public ValueComparer(IComparer<T> comparer)
            {
                if (comparer == null)
                    _comparer = Comparer<T>.Default;
                else
                    _comparer = comparer;
            }

#if NOCOMPARECREATE
            public ValueComparer(Comparison<T> comparison)
            {
                _comparison = comparison;
            }

            private Comparison<T> _comparison;
#endif
            private IComparer<T> _comparer;

            public override int Compare(PriorityNode<T> x, PriorityNode<T> y)
            {
#if NOCOMPARECREATE
                if (_comparison != null)
                    return _comparison(x.Value, y.Value);
#endif
                return _comparer.Compare(x.Value, y.Value);
            }

            public bool Equals(ValueComparer other)
            {
#if NOCOMPARECREATE
                if (_comparer != null)
                    return _comparer == other._comparer;
#endif
                return _comparer.Equals(other._comparer);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as ValueComparer);
            }

            public override int GetHashCode()
            {
                int nameHash = StringComparer.Ordinal.GetHashCode("ValueComparer");
#if NOCOMPARECREATE
                if (_comparison != null)
                    return _comparison.GetHashCode() + nameHash;
#endif
                return nameHash + _comparer.GetHashCode();
            }
        }

        /// <summary>
        /// Returns an <see cref="EqualityComparer{T}"/> which compares only the <see cref="Value"/> of priority nodes.
        /// </summary>
        /// <param name="comparer">The existing equality comparer to use, or <see langword="null"/> to use the default equality comparer.</param>
        /// <returns>A <see cref="PriorityNode{T}"/> equality comparer which compares only the values.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="comparer"/> is <see langword="null"/>.
        /// </exception>
        public EqualityComparer<PriorityNode<T>> GetEqualityComparer(IEqualityComparer<T> comparer)
        {
            return new ValueEqualityComparer(comparer);
        }

        private class ValueEqualityComparer : EqualityComparer<PriorityNode<T>>, IEquatable<ValueEqualityComparer>
        {
            public ValueEqualityComparer(IEqualityComparer<T> comparer)
            {
                if (comparer == null)
                    _comparer = EqualityComparer<T>.Default;
                else
                    _comparer = comparer;
            }

            private IEqualityComparer<T> _comparer;

            public override bool Equals(PriorityNode<T> x, PriorityNode<T> y)
            {
                return _comparer.Equals(x.Value, y.Value);
            }

            public override int GetHashCode(PriorityNode<T> obj)
            {
                try
                {
                    return _comparer.GetHashCode(obj.Value);
                }
                catch (ArgumentNullException)
                {
                    return 0;
                }
            }

            public bool Equals(ValueEqualityComparer other)
            {
                return _comparer.Equals(other._comparer);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as ValueEqualityComparer);
            }

            public override int GetHashCode()
            {
                int nameHash = StringComparer.Ordinal.GetHashCode("ValueEqualityComparer");
                return nameHash + _comparer.GetHashCode();
            }
        }
        #endregion
    }
}
