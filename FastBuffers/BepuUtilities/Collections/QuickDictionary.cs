﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using BepuUtilities.Memory;
using System.Runtime.CompilerServices;

namespace BepuUtilities.Collections
{
    /// <summary>
    /// Contains basic helpers for hashing.
    /// </summary>
    public static class HashHelper
    {
        /// <summary>
        /// Redistributes a hash. Useful for converting unique but contiguous hashes into a semirandom distribution.
        /// </summary>
        /// <param name="hash">Hash to redistribute.</param>
        /// <returns>Hashed hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Rehash(int hash)
        {
            //This rehash aims to address two problems:
            //1) Many common keys, such as ints and longs, will result in contiguous hash codes. 
            //Contiguous hash codes result in contiguous table entries, which destroy this implementation's linear probing performance.
            //2) Many common hashes have significantly patterned input, such as having all 0's in the lower bits. Since this implementation uses pow2-sized tables, 
            //patterns which could align with the pow2 table size can cause massive numbers of collisions.
            //So, we apply an additional scrambling pass on the hash to get rid of most such patterns. This won't stop a malicious attacker, but most coincidences should be avoided.
            //Keep in mind that this implementation is performance critical- there's no time for a bunch of rounds.
            //The initial multiplication serves to avoid some contiguity-induced patterning, the 
            //following xor'd rotations take care of most bit distribution patterning, and together it's super cheap. 
            //(You may be familiar with these rotation constants from SHA2 rounds.)

            const int a = 6;
            const int b = 13;
            const int c = 25;
            uint uhash = (uint)hash * 982451653u;
            var redongled =
                ((uhash << a) | (uhash >> (32 - a))) ^
                ((uhash << b) | (uhash >> (32 - b))) ^
                ((uhash << c) | (uhash >> (32 - c)));
            return (int)redongled;
        }
    }
    /// <summary>
    /// Container supporting constant time adds and removes of key-value pairs while preserving fast iteration times.
    /// Offers very direct access to information at the cost of safety.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Be very careful when using this type. It has sacrificed a lot upon the altar of performance; a few notable issues include:
    /// it is a value type and copying it around will break things without extreme care,
    /// it cannot be validly default-constructed,
    /// it exposes internal structures to user modification, 
    /// it rarely checks input for errors,
    /// the enumerator doesn't check for mid-enumeration modification,
    /// it allows unsafe addition that can break if the user doesn't manage the capacity,
    /// it works on top of an abstracted memory blob which might internally be a pointer that could be rugpulled, 
    /// it does not (and is incapable of) checking that provided memory gets returned to the same pool that it came from.
    /// </para>
    /// <para>Note that the implementation is extremely simple. It uses single-step linear probing under the assumption of very low collision rates.
    /// A generous table capacity is recommended; this trades some memory for simplicity and runtime performance.</para></remarks>
    /// <typeparam name="TKey">Type of key held by the container.</typeparam>
    /// <typeparam name="TValue">Type of value held by the container.</typeparam>
    /// <typeparam name="TKeySpan">Type of the key holding span.</typeparam>
    /// <typeparam name="TValueSpan">Type of the value holding span.</typeparam>
    /// <typeparam name="TTableSpan">Type of the index table span.</typeparam>
    /// <typeparam name="TEqualityComparer">Type of the equality tester and hash calculator used.</typeparam>
    public struct QuickDictionary<TKey, TValue, TKeySpan, TValueSpan, TTableSpan, TEqualityComparer> //i apologize
        where TKeySpan : ISpan<TKey>
        where TValueSpan : ISpan<TValue>
        where TTableSpan : ISpan<int>
        where TEqualityComparer : IEqualityComparerRef<TKey>
    {
        /// <summary>
        /// Gets the number of elements in the dictionary.
        /// </summary>
        public int Count;

        /// <summary>
        /// Mask for use in performing fast modulo operations for hashes. Requires that the table span is a power of 2.
        /// </summary>
        public int TableMask;

        /// <summary>
        /// Backing memory of the dictionary's table. Values are distributed according to the EqualityComparer's hash function.
        /// Slots containing 0 are unused and point to nothing. Slots containing higher values are equal to one plus the index of an element in the Span.
        /// </summary>
        public TTableSpan Table;

        /// <summary>
        /// Backing memory containing the keys of the dictionary.
        /// Indices from 0 to Count-1 hold actual data. All other data is undefined.
        /// </summary>
        public TKeySpan Keys;

        /// <summary>
        /// Backing memory containing the values of the dictionary.
        /// Indices from 0 to Count-1 hold actual data. All other data is undefined.
        /// </summary>
        public TValueSpan Values;

        /// <summary>
        /// Equality comparer used 
        /// </summary>
        public TEqualityComparer EqualityComparer;

        /// <summary>
        /// Gets or sets a key-value pair at the given index in the list representation.
        /// </summary>
        /// <param name="index">Index to grab a pair from.</param>
        /// <returns>Pair at the given index in the dictionary.</returns>
        public KeyValuePair<TKey, TValue> this[int index]
        {
            //You would think that such a trivial accessor would inline without any external suggestion.
            //Sometimes, yes. Sometimes, no. :(
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Debug.Assert(index >= 0 && index < Count, "Index should be within the dictionary's size.");
                return new KeyValuePair<TKey, TValue>(Keys[index], Values[index]);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Debug.Assert(index >= 0 && index < Count, "Index should be within the dictionary's size.");
                Keys[index] = value.Key;
                Values[index] = value.Value;
            }
        }


        /// <summary>
        /// Creates a new dictionary.
        /// </summary>
        /// <param name="initialKeySpan">Span to use as backing memory of the dictionary keys.</param>
        /// <param name="initialValueSpan">Span to use as backing memory of the dictionary values.</param>
        /// <param name="initialTableSpan">Span to use as backing memory of the table. Must be zeroed.</param>
        /// <param name="comparer">Comparer to use for the dictionary.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QuickDictionary(ref TKeySpan initialKeySpan, ref TValueSpan initialValueSpan, ref TTableSpan initialTableSpan, TEqualityComparer comparer)
        {
            ValidateSpanCapacity(ref initialKeySpan, ref initialValueSpan, ref initialTableSpan);
            Keys = initialKeySpan;
            Values = initialValueSpan;
            Table = initialTableSpan;
            TableMask = Table.Length - 1;
            Count = 0;
            EqualityComparer = comparer;
            Debug.Assert(EqualityComparer != null);
            ValidateTableIsCleared(ref initialTableSpan);
        }

        /// <summary>
        /// Creates a new dictionary with a default constructed comparer.
        /// </summary>
        /// <param name="initialKeySpan">Span to use as backing memory of the dictionary keys.</param>
        /// <param name="initialValueSpan">Span to use as backing memory of the dictionary values.</param>
        /// <param name="initialTableSpan">Span to use as backing memory of the table. Must be zeroed.</param>
        /// <param name="comparer">Comparer to use for the dictionary.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QuickDictionary(ref TKeySpan initialKeySpan, ref TValueSpan initialValueSpan, ref TTableSpan initialTableSpan)
            : this(ref initialKeySpan, ref initialValueSpan, ref initialTableSpan, default(TEqualityComparer))
        {
        }

        /// <summary>
        /// Creates a new dictionary.
        /// </summary>
        /// <param name="initialElementPoolIndex">Initial pool index to pull the object buffer from. The size of the initial buffer will be 2^initialElementPoolIndex.</param>
        /// <param name="tableSizePower">Initial pool index to pull the object buffer from. The size of the initial table buffer will be 2^(initialElementPoolIndex + tableSizePower).</param>
        /// <param name="comparer">Comparer to use in the dictionary.</param>
        /// <param name="dictionary">Created dictionary.</param>
        /// <param name="keyPool">Pool used for key spans.</param>   
        /// <param name="valuePool">Pool used for value spans.</param>   
        /// <param name="tablePool">Pool used for table spans.</param>
        /// <typeparam name="TKeyPool">Type of the pool used for key spans.</typeparam>
        /// <typeparam name="TValuePool">Type of the pool used for value spans.</typeparam>
        /// <typeparam name="TTablePool">Type of the pool used for table spans.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Create<TKeyPool, TValuePool, TTablePool>(TKeyPool keyPool, TValuePool valuePool, TTablePool tablePool, int initialElementPoolIndex, int tableSizePower, TEqualityComparer comparer,
            out QuickDictionary<TKey, TValue, TKeySpan, TValueSpan, TTableSpan, TEqualityComparer> dictionary)
            where TKeyPool : IMemoryPool<TKey, TKeySpan>
            where TValuePool : IMemoryPool<TValue, TValueSpan>
            where TTablePool : IMemoryPool<int, TTableSpan>
        {
            keyPool.TakeForPower(initialElementPoolIndex, out var keySpan);
            valuePool.TakeForPower(initialElementPoolIndex, out var valueSpan);
            tablePool.TakeForPower(initialElementPoolIndex + tableSizePower, out var tableSpan);
            //No guarantee that the table is clean; clear it.
            tableSpan.Clear(0, tableSpan.Length);
            dictionary = new QuickDictionary<TKey, TValue, TKeySpan, TValueSpan, TTableSpan, TEqualityComparer>(ref keySpan, ref valueSpan, ref tableSpan, comparer);
        }
        /// <summary>
        /// Creates a new dictionary with a default constructed comparer.
        /// </summary>
        /// <param name="initialElementPoolIndex">Initial pool index to pull the object buffer from. The size of the initial buffer will be 2^initialElementPoolIndex.</param>
        /// <param name="tableSizePower">Initial pool index to pull the object buffer from. The size of the initial table buffer will be 2^(initialElementPoolIndex + tableSizePower).</param>
        /// <param name="comparer">Comparer to use in the dictionary.</param>
        /// <param name="dictionary">Created dictionary.</param>
        /// <param name="keyPool">Pool used for key spans.</param>   
        /// <param name="valuePool">Pool used for value spans.</param>   
        /// <param name="tablePool">Pool used for table spans.</param>
        /// <typeparam name="TKeyPool">Type of the pool used for key spans.</typeparam>
        /// <typeparam name="TValuePool">Type of the pool used for value spans.</typeparam>
        /// <typeparam name="TTablePool">Type of the pool used for table spans.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Create<TKeyPool, TValuePool, TTablePool>(TKeyPool keyPool, TValuePool valuePool, TTablePool tablePool, int initialElementPoolIndex, int tableSizePower,
            out QuickDictionary<TKey, TValue, TKeySpan, TValueSpan, TTableSpan, TEqualityComparer> dictionary)
            where TKeyPool : IMemoryPool<TKey, TKeySpan>
            where TValuePool : IMemoryPool<TValue, TValueSpan>
            where TTablePool : IMemoryPool<int, TTableSpan>
        {
            Create(keyPool, valuePool, tablePool, initialElementPoolIndex, tableSizePower, default(TEqualityComparer), out dictionary);
        }

        /// <summary>
        /// Swaps out the dictionary's backing memory span for a new span.
        /// If the new span is smaller, the dictionary's count is truncated and the extra elements are dropped. 
        /// The old span is not cleared or returned to any pool; if it needs to be pooled or cleared, the user must handle it.
        /// </summary>
        /// <param name="newKeySpan">New span to use for keys.</param>
        /// <param name="newValueSpan">New span to use for values.</param>
        /// <param name="newTableSpan">New span to use for the table. Must be zeroed.</param>
        /// <param name="oldKeySpan">Previous span used for keys.</param>
        /// <param name="oldValueSpan">Previous span used for values.</param>
        /// <param name="oldTableSpan">Previous span used for the table.</param>
        public void Resize(ref TKeySpan newKeySpan, ref TValueSpan newValueSpan, ref TTableSpan newTableSpan,
            out TKeySpan oldKeySpan, out TValueSpan oldValueSpan, out TTableSpan oldTableSpan)
        {
            ValidateSpanCapacity(ref newKeySpan, ref newValueSpan, ref newTableSpan);
            ValidateTableIsCleared(ref newTableSpan);
            var oldDictionary = this;
            Keys = newKeySpan;
            Values = newValueSpan;
            Table = newTableSpan;
            Count = 0;
            TableMask = newTableSpan.Length - 1;
            var newCount = oldDictionary.Count > newKeySpan.Length ? newKeySpan.Length : oldDictionary.Count;

            //Unfortunately we can't really do a straight copy; the backing table relies on modulo operations.
            //Technically, we could copy the regular dictionary and then rely on a partial add to take care of the rest, but bleh!
            //Should really attempt to avoid resizes on sets and dictionaries whenever possible anyway. It ain't fast.
            for (int i = 0; i < newCount; ++i)
            {
                //We assume that ref adds will get inlined reasonably here. That's not actually guaranteed, but we'll bite the bullet.
                //(You could technically branch on the Unsafe.SizeOf<T>, which should result in a compile time specialized zero overhead implementation... but meh!)
                AddUnsafely(ref oldDictionary.Keys[i], ref oldDictionary.Values[i]);
            }
            oldKeySpan = oldDictionary.Keys;
            oldValueSpan = oldDictionary.Values;
            oldTableSpan = oldDictionary.Table;

        }

        /// <summary>
        /// Resizes the dictionary's backing array for the given size as a power of two.
        /// If the new span is smaller, the dictionary's count is truncated and the extra elements are dropped. 
        /// </summary>
        /// <param name="newSizePower">Exponent of the size of the new memory block. New size will be 2^newSizePower.</param>
        /// <param name="tablePoolOffset">Offset to apply to the object size power to get the table power. New table size will be 2^(newSizePower + tablePoolOffset).</param>
        /// <param name="keyPool">Pool used for key spans.</param>   
        /// <param name="valuePool">Pool used for value spans.</param>   
        /// <param name="tablePool">Pool used for table spans.</param>
        /// <typeparam name="TKeyPool">Type of the pool used for key spans.</typeparam>
        /// <typeparam name="TValuePool">Type of the pool used for value spans.</typeparam>
        /// <typeparam name="TTablePool">Type of the pool used for table spans.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResizeForPower<TKeyPool, TValuePool, TTablePool>(int newSizePower, int tablePoolOffset, TKeyPool keyPool, TValuePool valuePool, TTablePool tablePool)
            where TKeyPool : IMemoryPool<TKey, TKeySpan>
            where TValuePool : IMemoryPool<TValue, TValueSpan>
            where TTablePool : IMemoryPool<int, TTableSpan>
        {
            keyPool.TakeForPower(newSizePower, out var newKeySpan);
            valuePool.TakeForPower(newSizePower, out var newValueSpan);
            tablePool.TakeForPower(newSizePower + tablePoolOffset, out var newTableSpan);
            //There is no guarantee that the table retrieved from the pool is clean. Clear it!
            newTableSpan.Clear(0, newTableSpan.Length);
            var oldDictionary = this;
            Resize(ref newKeySpan, ref newValueSpan, ref newTableSpan, out var oldKeySpan, out var oldValueSpan, out var oldTableSpan);
            oldDictionary.Dispose(keyPool, valuePool, tablePool);
        }

        /// <summary>
        /// Resizes the dictionary's backing array for the given size.
        /// If the new span is smaller, the dictionary's count is truncated and the extra elements are dropped. 
        /// </summary>
        /// <param name="newSize">Minimum size of the new object memory block. Actual size may be larger.</param>
        /// <param name="keyPool">Pool used for key spans.</param>   
        /// <param name="valuePool">Pool used for value spans.</param>   
        /// <param name="tablePool">Pool used for table spans.</param>
        /// <typeparam name="TKeyPool">Type of the pool used for key spans.</typeparam>
        /// <typeparam name="TValuePool">Type of the pool used for value spans.</typeparam>
        /// <typeparam name="TTablePool">Type of the pool used for table spans.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize<TKeyPool, TValuePool, TTablePool>(int newSize, TKeyPool keyPool, TValuePool valuePool, TTablePool tablePool)
            where TKeyPool : IMemoryPool<TKey, TKeySpan>
            where TValuePool : IMemoryPool<TValue, TValueSpan>
            where TTablePool : IMemoryPool<int, TTableSpan>
        {
            var oldSpanPower = SpanHelper.GetContainingPowerOf2(Keys.Length);
            var oldTableSpanPower = SpanHelper.GetContainingPowerOf2(Table.Length);
            var tablePoolOffset = oldTableSpanPower - oldSpanPower;
            ResizeForPower(SpanHelper.GetContainingPowerOf2(newSize), tablePoolOffset, keyPool, valuePool, tablePool);
        }

        /// <summary>
        /// Returns the resources associated with the dictionary to pools. Any managed references still contained within the dictionary are cleared (and some unmanaged resources may also be cleared).
        /// </summary>
        /// <param name="keyPool">Pool used for key spans.</param>   
        /// <param name="valuePool">Pool used for value spans.</param>   
        /// <param name="tablePool">Pool used for table spans.</param>
        /// <typeparam name="TKeyPool">Type of the pool used for key spans.</typeparam>
        /// <typeparam name="TValuePool">Type of the pool used for value spans.</typeparam>
        /// <typeparam name="TTablePool">Type of the pool used for table spans.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose<TKeyPool, TValuePool, TTablePool>(TKeyPool keyPool, TValuePool valuePool, TTablePool tablePool)
             where TKeyPool : IMemoryPool<TKey, TKeySpan>
             where TValuePool : IMemoryPool<TValue, TValueSpan>
             where TTablePool : IMemoryPool<int, TTableSpan>
        {
            Keys.ClearManagedReferences(0, Count);
            Values.ClearManagedReferences(0, Count);
            keyPool.Return(ref Keys);
            valuePool.Return(ref Values);
            tablePool.Return(ref Table);
        }

        /// <summary>
        /// Ensures that the dictionary has enough room to hold the specified number of elements.
        /// </summary>     
        /// <param name="keyPool">Pool used for key spans.</param>   
        /// <param name="valuePool">Pool used for value spans.</param>   
        /// <param name="tablePool">Pool used for table spans.</param>
        /// <typeparam name="TKeyPool">Type of the pool used for key spans.</typeparam>
        /// <typeparam name="TValuePool">Type of the pool used for value spans.</typeparam>
        /// <typeparam name="TTablePool">Type of the pool used for table spans.</typeparam>
        /// <param name="count">Number of elements to hold.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity<TKeyPool, TValuePool, TTablePool>(int count, TKeyPool keyPool, TValuePool valuePool, TTablePool tablePool)
            where TKeyPool : IMemoryPool<TKey, TKeySpan>
            where TValuePool : IMemoryPool<TValue, TValueSpan>
            where TTablePool : IMemoryPool<int, TTableSpan>
        {
            if (count > Keys.Length)
            {
                Resize(count, keyPool, valuePool, tablePool);
            }
        }

        /// <summary>
        /// Shrinks the internal buffers to the smallest acceptable size and releases the old buffers to the pools.
        /// </summary>
        /// <param name="keyPool">Pool used for key spans.</param>   
        /// <param name="valuePool">Pool used for value spans.</param>   
        /// <param name="tablePool">Pool used for table spans.</param>
        /// <typeparam name="TKeyPool">Type of the pool used for key spans.</typeparam>
        /// <typeparam name="TValuePool">Type of the pool used for value spans.</typeparam>
        /// <typeparam name="TTablePool">Type of the pool used for table spans.</typeparam>
        /// <param name="element">Element to add.</param>
        public void Compact<TKeyPool, TValuePool, TTablePool>(TKeyPool keyPool, TValuePool valuePool, TTablePool tablePool)
            where TKeyPool : IMemoryPool<TKey, TKeySpan>
            where TValuePool : IMemoryPool<TValue, TValueSpan>
            where TTablePool : IMemoryPool<int, TTableSpan>
        {
            Validate();
            var minimumRequiredPoolIndex = SpanHelper.GetContainingPowerOf2(Count);
            if ((1 << minimumRequiredPoolIndex) != Keys.Length)
                Resize(Count, keyPool, valuePool, tablePool);
        }



        /// <summary>
        /// Gets the index of the element in the table.
        /// </summary>
        /// <param name="element">Element to look up.</param>
        /// <param name="tableIndex">Index of the element in the redirect table, or if it is not present, the index of where it would be added.</param>
        /// <param name="elementIndex">The index of the element in the element arrays, if it exists; -1 otherwise.</param>
        /// <returns>True if the element is present in the dictionary, false if it is not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetTableIndices(ref TKey element, out int tableIndex, out int elementIndex)
        {
            Validate();

            //The table lengths are guaranteed to be a power of 2, so the modulo is a simple binary operation.
            tableIndex = HashHelper.Rehash(EqualityComparer.Hash(ref element)) & TableMask;
            //0 in the table means 'not taken'; all other values are offset by 1 upward. That is, 1 is actually index 0, 2 is actually index 1, and so on.
            //This is preferred over using a negative number for flagging since clean buffers will contain all 0's.
            while ((elementIndex = Table[tableIndex]) > 0)
            {
                //This table index is taken. Is this the specified element?
                //Remember to decode the object index.
                if (EqualityComparer.Equals(ref Keys[--elementIndex], ref element))
                {
                    return true;
                }
                tableIndex = (tableIndex + 1) & TableMask;
            }
            elementIndex = -1;
            return false;
        }

        /// <summary>
        /// Gets the index of the key in the dictionary values list if it exists.
        /// </summary>
        /// <param name="key">Key to get the index of.</param>
        /// <returns>The index of the key if the key exists in the dictionary, -1 otherwise.</returns>
        public int IndexOf(TKey key)
        {
            Validate();
            GetTableIndices(ref key, out int tableIndex, out int objectIndex);
            return objectIndex;
        }


        /// <summary>
        /// Gets the index of the key in the dictionary values list if it exists.
        /// </summary>
        /// <param name="key">Key to get the index of.</param>
        /// <returns>The index of the key if the key exists in the dictionary, -1 otherwise.</returns>
        public int IndexOf(ref TKey key)
        {
            Validate();
            GetTableIndices(ref key, out int tableIndex, out int objectIndex);
            return objectIndex;
        }

        /// <summary>
        /// Checks if a given key already belongs to the dictionary.
        /// </summary>
        /// <param name="key">Key to test for.</param>
        /// <returns>True if the key already belongs to the dictionary, false otherwise.</returns>
        public bool ContainsKey(TKey key)
        {
            Validate();
            return GetTableIndices(ref key, out int tableIndex, out int objectIndex);
        }

        /// <summary>
        /// Checks if a given key already belongs to the dictionary.
        /// </summary>
        /// <param name="key">Key to test for.</param>
        /// <returns>True if the key already belongs to the dictionary, false otherwise.</returns>
        public bool ContainsKey(ref TKey key)
        {
            Validate();
            return GetTableIndices(ref key, out int tableIndex, out int objectIndex);
        }

        /// <summary>
        /// Tries to retrieve the value associated with a key if it exists.
        /// </summary>
        /// <param name="key">Key to look up.</param>
        /// <param name="value">Value associated with the specified key.</param>
        /// <returns>True if a value was found, false otherwise.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            Validate();
            if (GetTableIndices(ref key, out int tableIndex, out int elementIndex))
            {
                value = Values[elementIndex];
                return true;
            }
            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Tries to retrieve the value associated with a key if it exists.
        /// </summary>
        /// <param name="key">Key to look up.</param>
        /// <param name="value">Value associated with the specified key.</param>
        /// <returns>True if a value was found, false otherwise.</returns>
        public bool TryGetValue(ref TKey key, out TValue value)
        {
            Validate();
            if (GetTableIndices(ref key, out int tableIndex, out int elementIndex))
            {
                value = Values[elementIndex];
                return true;
            }
            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Adds a pair to the dictionary. If a version of the key (same hash code, 'equal' by comparer) is already present,
        /// the existing pair is replaced by the given version.
        /// </summary>
        /// <param name="key">Key of the pair to add.</param>
        /// <param name="value">Value of the pair to add.</param>
        /// <returns>True if the pair was added to the dictionary, false if the key was already present and its pair was replaced.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)] //TODO: Test performance of full chain inline.
        public bool AddAndReplaceUnsafely(ref TKey key, ref TValue value)
        {
            Validate();
            ValidateUnsafeAdd();

            if (GetTableIndices(ref key, out int tableIndex, out int elementIndex))
            {
                //Already present!
                Keys[elementIndex] = key;
                Values[elementIndex] = value;
                return false;
            }

            //It wasn't in the dictionary. Add it!
            Keys[Count] = key;
            Values[Count] = value;
            //Use the encoding- all indices are offset by 1 since 0 represents 'empty'.
            Table[tableIndex] = ++Count;
            return true;
        }

        /// <summary>
        /// Adds a pair to the dictionary. If a version of the key (same hash code, 'equal' by comparer) is already present,
        /// the existing pair is replaced by the given version.
        /// </summary>
        /// <param name="key">Key of the pair to add.</param>
        /// <param name="value">Value of the pair to add.</param>
        /// <returns>True if the pair was added to the dictionary, false if the key was already present and its pair was replaced.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddAndReplaceUnsafely(TKey key, TValue value)
        {
            return AddAndReplaceUnsafely(ref key, ref value);
        }

        /// <summary>
        /// Adds a pair to the dictionary if it is not already present.
        /// </summary>
        /// <param name="key">Key of the pair to add.</param>
        /// <param name="value">Value of the pair to add.</param>
        /// <returns>True if the pair was added to the dictionary, false if the key was already present.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)] //TODO: Test performance of full chain inline.
        public bool AddUnsafely(ref TKey key, ref TValue value)
        {
            Validate();
            ValidateUnsafeAdd();
            if (GetTableIndices(ref key, out int tableIndex, out int elementIndex))
            {
                //Already present!
                return false;
            }

            //It wasn't in the dictionary. Add it!
            Keys[Count] = key;
            Values[Count] = value;
            //Use the encoding- all indices are offset by 1 since 0 represents 'empty'.
            Table[tableIndex] = ++Count;
            return true;
        }

        /// <summary>
        /// Adds a pair to the dictionary if it is not already present.
        /// </summary>
        /// <param name="key">Key of the pair to add.</param>
        /// <param name="value">Value of the pair to add.</param>
        /// <returns>True if the pair was added to the dictionary, false if the key was already present.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddUnsafely(TKey key, TValue value)
        {
            return AddUnsafely(ref key, ref value);
        }

        /// <summary>
        /// Adds a pair to the dictionary. If a version of the key (same hash code, 'equal' by comparer) is already present,
        /// the existing pair is replaced by the given version.
        /// </summary>
        /// <param name="key">Key of the pair to add.</param>
        /// <param name="value">Value of the pair to add.</param>
        /// <param name="keyPool">Pool used for key spans.</param>   
        /// <param name="valuePool">Pool used for value spans.</param>   
        /// <param name="tablePool">Pool used for table spans.</param>
        /// <typeparam name="TKeyPool">Type of the pool used for key spans.</typeparam>
        /// <typeparam name="TValuePool">Type of the pool used for value spans.</typeparam>
        /// <typeparam name="TTablePool">Type of the pool used for table spans.</typeparam>
        /// <returns>True if the pair was added to the dictionary, false if the key was already present and its pair was replaced.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddAndReplace<TKeyPool, TValuePool, TTablePool>(ref TKey key, ref TValue value,
            TKeyPool keyPool, TValuePool valuePool, TTablePool tablePool)
            where TKeyPool : IMemoryPool<TKey, TKeySpan>
            where TValuePool : IMemoryPool<TValue, TValueSpan>
            where TTablePool : IMemoryPool<int, TTableSpan>
        {
            if (Count == Keys.Length)
            {
                //There's no room left; resize.
                Resize(Count * 2, keyPool, valuePool, tablePool);

                //Note that this is tested before any indices are found.
                //If we resized only after determining that it was going to be added,
                //the potential resize would invalidate the computed indices.
            }
            return AddAndReplaceUnsafely(ref key, ref value);
        }

        /// <summary>
        /// Adds a pair to the dictionary. If a version of the key (same hash code, 'equal' by comparer) is already present,
        /// the existing pair is replaced by the given version.
        /// </summary>
        /// <param name="key">Key of the pair to add.</param>
        /// <param name="value">Value of the pair to add.</param>
        /// <param name="keyPool">Pool used for key spans.</param>   
        /// <param name="valuePool">Pool used for value spans.</param>   
        /// <param name="tablePool">Pool used for table spans.</param>
        /// <typeparam name="TKeyPool">Type of the pool used for key spans.</typeparam>
        /// <typeparam name="TValuePool">Type of the pool used for value spans.</typeparam>
        /// <typeparam name="TTablePool">Type of the pool used for table spans.</typeparam>
        /// <returns>True if the pair was added to the dictionary, false if the key was already present and its pair was replaced.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddAndReplace<TKeyPool, TValuePool, TTablePool>(TKey key, TValue value,
            TKeyPool keyPool, TValuePool valuePool, TTablePool tablePool)
            where TKeyPool : IMemoryPool<TKey, TKeySpan>
            where TValuePool : IMemoryPool<TValue, TValueSpan>
            where TTablePool : IMemoryPool<int, TTableSpan>
        {
            return AddAndReplace(ref key, ref value, keyPool, valuePool, tablePool);
        }

        /// <summary>
        /// Adds a pair to the dictionary if it is not already present.
        /// </summary>
        /// <param name="key">Key of the pair to add.</param>
        /// <param name="value">Value of the pair to add.</param>
        /// <param name="keyPool">Pool used for key spans.</param>   
        /// <param name="valuePool">Pool used for value spans.</param>   
        /// <param name="tablePool">Pool used for table spans.</param>
        /// <typeparam name="TKeyPool">Type of the pool used for key spans.</typeparam>
        /// <typeparam name="TValuePool">Type of the pool used for value spans.</typeparam>
        /// <typeparam name="TTablePool">Type of the pool used for table spans.</typeparam>
        /// <returns>True if the pair was added to the dictionary, false if the key was already present.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add<TKeyPool, TValuePool, TTablePool>(ref TKey key, ref TValue value,
            TKeyPool keyPool, TValuePool valuePool, TTablePool tablePool)
            where TKeyPool : IMemoryPool<TKey, TKeySpan>
            where TValuePool : IMemoryPool<TValue, TValueSpan>
            where TTablePool : IMemoryPool<int, TTableSpan>
        {
            Validate();

            if (Count == Keys.Length)
            {
                //There's no room left; resize.
                Resize(Count * 2, keyPool, valuePool, tablePool);

                //Note that this is tested before any indices are found.
                //If we resized only after determining that it was going to be added,
                //the potential resize would invalidate the computed indices.
            }
            return AddUnsafely(ref key, ref value);
        }

        /// <summary>
        /// Adds a pair to the dictionary if it is not already present.
        /// </summary>
        /// <param name="key">Key of the pair to add.</param>
        /// <param name="value">Value of the pair to add.</param>
        /// <param name="keyPool">Pool used for key spans.</param>   
        /// <param name="valuePool">Pool used for value spans.</param>   
        /// <param name="tablePool">Pool used for table spans.</param>
        /// <typeparam name="TKeyPool">Type of the pool used for key spans.</typeparam>
        /// <typeparam name="TValuePool">Type of the pool used for value spans.</typeparam>
        /// <typeparam name="TTablePool">Type of the pool used for table spans.</typeparam>
        /// <returns>True if the pair was added to the dictionary, false if the key was already present.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add<TKeyPool, TValuePool, TTablePool>(TKey key, TValue value,
            TKeyPool keyPool, TValuePool valuePool, TTablePool tablePool)
            where TKeyPool : IMemoryPool<TKey, TKeySpan>
            where TValuePool : IMemoryPool<TValue, TValueSpan>
            where TTablePool : IMemoryPool<int, TTableSpan>
        {
            return Add(ref key, ref value, keyPool, valuePool, tablePool);
        }

        //Note: the reason this is named "FastRemove" instead of just "Remove" despite it being the only remove present is that
        //there may later exist an order preserving "Remove". That would be a very sneaky breaking change.

        /// <summary>
        /// Removes an element from the dictionary according to its table and element index. Can only be used if the table and element index are valid.
        /// </summary>
        /// <param name="tableIndex">Index of the table entry associated with the existing element to remove.</param>
        /// <param name="elementIndex">Index of the existing element to remove in the contiguous key/value arrays.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastRemove(int tableIndex, int elementIndex)
        {
            Debug.Assert(GetTableIndices(ref Keys[elementIndex], out var debugTableIndex, out var debugElementIndex) && debugTableIndex == tableIndex && debugElementIndex == elementIndex,
                "The table index and element index used to directly remove must match an actual key.");
            //Add and remove must both maintain a property:
            //All items are either at their desired index (as defined by the hash), or they are contained in a contiguous block clockwise from the desired index.
            //Removals seek to fill the gap they create by searching clockwise to find items which can be moved backward.
            //Search clockwise for an item to fill this slot. The search must continue until a gap is found.
            int moveCandidateIndex;
            int gapIndex = tableIndex;
            //Search clockwise.
            while ((moveCandidateIndex = Table[tableIndex = (tableIndex + 1) & TableMask]) > 0)
            {
                //This slot contains something. What is its actual index?
                --moveCandidateIndex;
                int desiredIndex = HashHelper.Rehash(EqualityComparer.Hash(ref Keys[moveCandidateIndex])) & TableMask;

                //Would this element be closer to its actual index if it was moved to the gap?
                //To find out, compute the clockwise distance from the gap and the clockwise distance from the ideal location.

                var distanceFromGap = (tableIndex - gapIndex) & TableMask;
                var distanceFromIdeal = (tableIndex - desiredIndex) & TableMask;
                if (distanceFromGap <= distanceFromIdeal)
                {
                    //The distance to the gap is less than or equal the distance to the ideal location, so just move to the gap.
                    Table[gapIndex] = Table[tableIndex];
                    gapIndex = tableIndex;
                }

            }
            //Clear the table gap left by the removal.
            Table[gapIndex] = 0;
            //Swap the final element into the removed object's element array index, if the removed object wasn't the last object.
            --Count;
            if (elementIndex < Count)
            {
                Keys[elementIndex] = Keys[Count];
                Values[elementIndex] = Values[Count];
                //Locate the swapped object in the table and update its index.
                GetTableIndices(ref Keys[elementIndex], out tableIndex, out int oldObjectIndex);
                Table[tableIndex] = elementIndex + 1; //Remember the encoding! all indices offset by 1.
            }
            //Clear the final slot in the elements set.
            Keys[Count] = default;
            Values[Count] = default;
        }

        /// <summary>
        /// Removes a pair associated with a key from the dictionary if belongs to the dictionary.
        /// Does not preserve the order of elements in the dictionary.
        /// </summary>
        /// <param name="key">Key of the pair to remove.</param>
        /// <returns>True if the key was found and removed, false otherwise.</returns>
        public bool FastRemove(ref TKey key)
        {
            Validate();
            //Find it.
            if (GetTableIndices(ref key, out int tableIndex, out int elementIndex))
            {
                //We found the object!
                FastRemove(tableIndex, elementIndex);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a pair associated with a key from the dictionary if belongs to the dictionary.
        /// Does not preserve the order of elements in the dictionary.
        /// </summary>
        /// <param name="key">Key of the pair to remove.</param>
        /// <returns>True if the key was found and removed, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FastRemove(TKey key)
        {
            return FastRemove(ref key);
        }

        /// <summary>
        /// Removes all elements from the dictionary.
        /// </summary>
        public void Clear()
        {
            //While it may be appealing to remove individual elements from the dictionary when sparse,
            //using a brute force clear over the entire table is almost always faster. And it's a lot simpler!
            Table.Clear(0, Table.Length);
            Keys.Clear(0, Count);
            Values.Clear(0, Count);
            Count = 0;
        }

        /// <summary>
        /// Removes all elements from the dictionary without modifying the contents of the keys or values arrays. Be careful about using this with reference types.
        /// </summary>
        public void FastClear()
        {
            Table.Clear(0, Table.Length);
            Count = 0;
        }

        public Enumerator GetEnumerator()
        {
            Validate();
            return new Enumerator(ref Keys, ref Values, Count);
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly TKeySpan keys;
            private readonly TValueSpan values;
            private readonly int count;
            private int index;

            public Enumerator(ref TKeySpan keys, ref TValueSpan values, int count)
            {
                this.keys = keys;
                this.values = values;
                this.count = count;

                index = -1;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get { return new KeyValuePair<TKey, TValue>(keys[index], values[index]); }
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                return ++index < count;
            }

            public void Reset()
            {
                index = -1;
            }
        }
        [Conditional("DEBUG")]
        static void ValidateSpanCapacity(ref TKeySpan keySpan, ref TValueSpan valueSpan, ref TTableSpan tableSpan)
        {
            Debug.Assert(tableSpan.Length >= keySpan.Length, "The table span must be at least as large as the key span.");
            Debug.Assert(valueSpan.Length >= keySpan.Length, "The value span must be at least as large as the key span.");
            Debug.Assert((tableSpan.Length & (tableSpan.Length - 1)) == 0, "Dictionaries depend upon power of 2 backing table span sizes for efficient modulo operations.");
        }

        [Conditional("DEBUG")]
        private void Validate()
        {
            Debug.Assert(Keys.Length != 0 && Values.Length != 0 && Table.Length != 0, "The QuickDictionary must have its internal buffers and pools available; default-constructed or disposed QuickDictionary should not be used.");
            ValidateSpanCapacity(ref Keys, ref Values, ref Table);
        }


        [Conditional("DEBUG")]
        void ValidateUnsafeAdd()
        {
            Debug.Assert(Count < Keys.Length, "Unsafe adders can only be used if the capacity is guaranteed to hold the new size.");
        }



        [Conditional("DEBUG")]
        void ValidateTableIsCleared(ref TTableSpan span)
        {
            for (int i = 0; i < span.Length; ++i)
            {
                Debug.Assert(span[i] == 0, "The table provided to the set must be cleared.");
            }
        }




    }
}
