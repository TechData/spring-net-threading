#region License

/*
 * Copyright � 2002-2006 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Collections;

namespace Spring.Collections
{
    /// <summary> 
    /// A <see cref="Spring.Collections.ISortedDictionary"/> extended with navigation methods returning the
    /// closest matches for given search targets. 
    /// </summary>
    /// <remarks>
    /// Methods <see cref="Spring.Collections.INavigableDictionary.LowerEntry(object)"/>, 
    /// <see cref="Spring.Collections.INavigableDictionary.FloorEntry(object)"/>, 
    /// <see cref="Spring.Collections.INavigableDictionary.CeilingEntry(object)"/>,
    /// and <see cref="Spring.Collections.INavigableDictionary.HigherEntry(object)"/> 
    /// return objects associated with keys respectively less than, less than or equal,
    /// greater than or equal, and greater than a given key, returning
    /// <see lang="null"/> if there is no such key.  Similarly, methods
    /// <see cref="Spring.Collections.INavigableDictionary.LowerKey(object)"/>, 
    /// <see cref="Spring.Collections.INavigableDictionary.FloorKey(object)"/>, 
    /// <see cref="Spring.Collections.INavigableDictionary.CeilingKey(object)"/>, and
    /// <see cref="Spring.Collections.INavigableDictionary.HigherKey(object)"/> 
    /// return only the associated keys. All of these
    /// methods are designed for locating, not traversing entries.
    /// 
    /// <p/>
    /// A <see cref="Spring.Collections.INavigableDictionary"/> may be viewed and traversed in either
    /// ascending or descending key order.  The <see cref="System.Collections.IDictionary"/> properties
    /// <see cref="System.Collections.IDictionary.Keys"/> and <see cref="System.Collections.IDictionary.Values"/> return ascending views, and
    /// the additional methods <see cref="Spring.Collections.INavigableDictionary.DescendingKeys"/> and
    /// <see cref="Spring.Collections.INavigableDictionary.DescendingEntries"/> return descending views. The
    /// performance of ascending traversals is likely to be faster than
    /// descending traversals.  Notice that it is possible to perform
    /// subrange traversals in either direction using <see cref="Spring.Collections.INavigableDictionary.NavigableSubDictionary"/>.
    /// Methods <see cref="Spring.Collections.INavigableDictionary.NavigableSubDictionary"/>, <see cref="Spring.Collections.INavigableDictionary.NavigableHeadDictionary"/>, and
    /// <see cref="Spring.Collections.INavigableDictionary.NavigableTailDictionary"/> differ from the similarly named
    /// <see cref="Spring.Collections.ISortedDictionary"/> methods only in that the returned maps
    /// are guaranteed to obey the <see cref="Spring.Collections.INavigableDictionary"/> interface.
    /// <p/>
    /// This interface additionally defines methods <see cref="Spring.Collections.INavigableDictionary.FirstEntry"/>,
    /// <see cref="Spring.Collections.INavigableDictionary.PollFirstEntry()"/>, 
    /// <see cref="Spring.Collections.INavigableDictionary.LastEntry"/>, 
    /// and <see cref="Spring.Collections.INavigableDictionary.PollLastEntry()"/> that return and/or remove the least and
    /// greatest mappings, if any exist, else returning <see lang="null"/>.
    /// </remarks>
    /// 
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio (.NET) </author>
    public interface INavigableDictionary : ISortedDictionary
    {
        /// <summary> 
        /// Returns a <see cref="System.Collections.DictionaryEntry"/>  associated with the greatest key
        /// strictly less than the given key, or <see lang="null"/> if there is
        /// no such key.
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns> 
        /// an entry with the greatest key less than <paramref name="key"/>,
        /// or <see lang="null"/> if there is no such key
        /// </returns>
        DictionaryEntry LowerEntry(object key);

        /// <summary> 
        /// Returns the greatest key strictly less than the given key, or
        /// <see lang="null"/> if there is no such key.
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns> the greatest key less than <paramref name="key"/>,
        /// or <see lang="null"/> if there is no such key
        /// </returns>
        object LowerKey(object key);

        /// <summary> 
        /// Returns a <see cref="System.Collections.DictionaryEntry"/> associated with the greatest key
        /// less than or equal to the given key, or <see lang="null"/> if there
        /// is no such key.
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns> an entry with the greatest key less than or equal to
        /// <paramref name="key"/>, or <see lang="null"/> if there is no such key
        /// </returns>
        DictionaryEntry FloorEntry(object key);

        /// <summary> 
        /// Returns the greatest key less than or equal to the given key,
        /// or <see lang="null"/> if there is no such key.
        /// </summary>
        /// <param name="key">the key
        /// </param>
        /// <returns> the greatest key less than or equal to <paramref name="key"/>,
        /// or <see lang="null"/> if there is no such key
        /// </returns>
        object FloorKey(Object key);

        /// <summary> 
        /// Returns a <see cref="System.Collections.DictionaryEntry"/> associated with the least key
        /// greater than or equal to the given key, or <see lang="null"/> if
        /// there is no such key.
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns> an entry with the least key greater than or equal to
        /// <paramref name="key"/>, or <see lang="null"/> if there is no such key
        /// </returns>
        DictionaryEntry CeilingEntry(object key);

        /// <summary> 
        /// Returns the least key greater than or equal to the given key,
        /// or <see lang="null"/> if there is no such key.
        /// </summary>
        /// <param name="key">the key
        /// </param>
        /// <returns> the least key greater than or equal to <paramref name="key"/>,
        /// or <see lang="null"/> if there is no such key
        /// </returns>
        object CeilingKey(Object key);

        /// <summary> 
        /// Returns a <see cref="System.Collections.DictionaryEntry"/> associated with the least key
        /// strictly greater than the given key, or <see lang="null"/> if there
        /// is no such key.
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns> an entry with the least key greater than <paramref name="key"/>,
        /// or <see lang="null"/> if there is no such key
        /// </returns>
        DictionaryEntry HigherEntry(object key);

        /// <summary> 
        /// Returns the least key strictly greater than the given key, or
        /// <see lang="null"/> if there is no such key.
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns> the least key greater than <paramref name="key"/>,
        /// or <see lang="null"/> if there is no such key
        /// </returns>
        object HigherKey(object key);

        /// <summary> 
        /// Returns a <see cref="System.Collections.DictionaryEntry"/> associated with the least
        /// key in this dictionary, or <see lang="null"/> if the dictionary is empty.
        /// </summary>
        /// <returns> an entry with the least key,
        /// or <see lang="null"/> if this dictionary is empty
        /// </returns>
        DictionaryEntry FirstEntry { get; }

        /// <summary> 
        /// Returns a <see cref="System.Collections.DictionaryEntry"/> associated with the greatest
        /// key in this dictionary, or <see lang="null"/> if the dictionary is empty.
        /// </summary>
        /// <returns> an entry with the greatest key,
        /// or <see lang="null"/> if this dictionary is empty
        /// </returns>
        DictionaryEntry LastEntry { get; }

        /// <summary> 
        /// Removes and returns a <see cref="System.Collections.DictionaryEntry"/> associated with
        /// the least key in this dictionary, or <see lang="null"/> if the dictionary is empty.
        /// </summary>
        /// <returns> the removed first entry of this dictionary,
        /// or <see lang="null"/> if this dictionary is empty
        /// </returns>
        DictionaryEntry PollFirstEntry();

        /// <summary> 
        /// Removes and returns a <see cref="System.Collections.DictionaryEntry"/> associated with
        /// the greatest key in this dictionary, or <see lang="null"/> if the dictionary is empty.
        /// </summary>
        /// <returns> the removed last entry of this dictionary,
        /// or <see lang="null"/> if this dictionary is empty
        /// </returns>
        DictionaryEntry PollLastEntry();

        /// <summary> 
        /// Returns a <see cref="Spring.Collections.ISet"/> view of the keys contained in this dictionary.
        /// </summary>
        /// <remarks>
        /// The set's iterator returns the keys in descending order.
        /// The set is backed by the dictionary, so changes to the dictionary are
        /// reflected in the set, and vice-versa.  If the dictionary is modified
        /// while an iteration over the set is in progress (except through
        /// the iterator's own remove operation), the results of
        /// the iteration are undefined.  The set supports element removal,
        /// which removes the corresponding mapping from the dictionary, via the
        /// <see cref="Spring.Collections.ISet.Remove(object)"/>,
        /// <see cref="Spring.Collections.ISet.RemoveAll(ICollection)"/>, <see cref="Spring.Collections.ISet.RetainAll(ICollection)"/>, 
        /// and <see cref="Spring.Collections.ISet.Clear()"/> operations.  
        /// It does not support the <see cref="Spring.Collections.ISet.Add(object)"/> or <see cref="Spring.Collections.ISet.AddAll(ICollection)"/>
        /// operations.
        /// </remarks>
        /// <returns> 
        /// a set view of the keys contained in this dictionary, sorted indescending order
        /// </returns>
        ISet DescendingKeys { get; }

        /// <summary> 
        /// Returns a <see cref="Spring.Collections.ISet"/> view of the key / value pairs contained in this dictionary.
        /// </summary>
        /// <remarks>
        /// The set's iterator returns the entries in descending order.
        /// The set is backed by the dictionary, so changes to the dictionary are
        /// reflected in the set, and vice-versa.  If the dictionary is modified
        /// while an iteration over the set is in progress (except through
        /// the iterator's own remove operation), the results of
        /// the iteration are undefined.  The set supports element removal,
        /// which removes the corresponding mapping from the dictionary, via the
        /// <see cref="Spring.Collections.ISet.Remove(object)"/>,
        /// <see cref="Spring.Collections.ISet.RemoveAll(ICollection)"/>, <see cref="Spring.Collections.ISet.RetainAll(ICollection)"/>, 
        /// and <see cref="Spring.Collections.ISet.Clear()"/> operations.  
        /// It does not support the <see cref="Spring.Collections.ISet.Add(object)"/> or <see cref="Spring.Collections.ISet.AddAll(ICollection)"/>
        /// operations.
        /// </remarks>
        /// <returns> 
        /// a set view of the mappings contained in this dictionary,
        /// sorted in descending key order
        /// </returns>
        ISet DescendingEntries { get; }

        /// <summary> 
        /// Returns a view of the portion of this dictionary whose keys range from
        /// <paramref name="fromKey"/>, inclusive, to <parmref name="toKey"/> , exclusive.  
        /// </summary>
        /// <remarks>
        /// If <paramref name="fromKey"/> and <parmref name="toKey"/> are equal, the returned dictionary
        /// is empty.  The returned dictionary is backed by this dictionary, so changes
        /// in the returned dictionary are reflected in this dictionary, and vice-versa.
        /// The returned dictionary supports all optional dictionary operations that this
        /// dictionary supports.
        /// 
        /// <p/>
        /// The returned dictionary will throw an <see cref="System.ArgumentException"/> 
        /// on an attempt to insert a key outside its range.
        /// </remarks>
        /// <param name="fromKey">
        /// low endpoint (inclusive) of the keys in the returned dictionary
        /// </param>
        /// <param name="toKey">
        /// high endpoint (exclusive) of the keys in the returned dictionary
        /// </param>
        /// <returns> a view of the portion of this dictionary whose keys range from
        /// <paramref name="fromKey"/>, inclusive, to <parmref name="toKey"/>, exclusive
        /// </returns>
        INavigableDictionary NavigableSubDictionary(object fromKey, object toKey);

        /// <summary> 
        /// Returns a view of the portion of this dictionary whose keys are
        /// strictly less than <paramref name="toKey"/>.  
        /// </summary>
        /// <remarks> 
        /// The returned dictionary is backed
        /// by this dictionary, so changes in the returned dictionary are reflected in
        /// this dictionary, and vice-versa.  The returned dictionary supports all
        /// optional dictionary operations that this dictionary supports.
        /// 
        /// <p/>
        /// The returned dictionary will throw an <see cref="System.ArgumentException"/>
        /// on an attempt to insert a key outside its range.
        /// </remarks>
        /// <param name="toKey">
        /// high endpoint (exclusive) of the keys in the returned dictionary
        /// </param>
        /// <returns> 
        /// a view of the portion of this dictionary whose keys are strictly
        /// less than <paramref name="toKey"/>
        /// </returns>
        INavigableDictionary NavigableHeadDictionary(object toKey);

        /// <summary> 
        /// Returns a view of the portion of this dictionary whose keys are
        /// greater than or equal to <paramref name="fromKey"/>.  
        /// </summary>
        /// <remarks> 
        /// The returned dictionary is backed by this dictionary, so changes in the returned dictionary are
        /// reflected in this dictionary, and vice-versa.  The returned dictionary
        /// supports all optional dictionary operations that this dictionary supports.
        /// 
        /// <p/>
        /// The returned dictionary will throw an <see cref="System.ArgumentException"/>
        /// on an attempt to insert a key outside its range.
        /// </remarks>
        /// <param name="fromKey">low endpoint (inclusive) of the keys in the returned dictionary
        /// </param>
        /// <returns> a view of the portion of this dictionary whose keys are greater
        /// than or equal to <paramref name="fromKey"/>
        /// </returns>
        INavigableDictionary NavigableTailDictionary(object fromKey);
    }
}