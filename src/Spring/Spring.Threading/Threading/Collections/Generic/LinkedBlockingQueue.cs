#region License

/*
 * Copyright (C) 2002-2008 the original author or authors.
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
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using Spring.Collections.Generic;
using Spring.Utility;

namespace Spring.Threading.Collections.Generic {
    /// <summary> 
    /// An optionally-bounded <see cref="IBlockingQueue{T}"/> based on
    /// linked nodes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This queue orders elements FIFO (first-in-first-out).
    /// The <b>head</b> of the queue is that element that has been on the
    /// queue the longest time.
    /// The <b>tail</b> of the queue is that element that has been on the
    /// queue the shortest time. New elements
    /// are inserted at the tail of the queue, and the queue retrieval
    /// operations obtain elements at the head of the queue.
    /// Linked queues typically have higher throughput than array-based queues but
    /// less predictable performance in most concurrent applications.
    /// </para>
    /// <para>
    /// The optional capacity bound constructor argument serves as a
    /// way to prevent excessive queue expansion. The capacity, if unspecified,
    /// is equal to <see cref="System.Int32.MaxValue"/>.  Linked nodes are
    /// dynamically created upon each insertion unless this would bring the
    /// queue above capacity.
    /// </para>
    /// </remarks>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio (.NET)</author>
    [Serializable]
    public class LinkedBlockingQueue<T> : AbstractBlockingQueue<T>, ISerializable //BACKPORT_3_1
    {

        #region inner classes

        internal class Node {
            internal T item;

            internal Node next;

            internal Node(T x) {
                item = x;
            }
        }

        #endregion

        #region private fields

        /// <summary>
        /// Change version to support enumerator fast fail when queue changes.
        /// </summary>
        [NonSerialized]
        private volatile int _version;

        /// <summary>The capacity bound, or <see cref="int.MaxValue"/> if none </summary>
        private readonly int _capacity;

        /// <summary>Current number of elements </summary>
        [NonSerialized]
        private volatile int _activeCount;

        /// <summary>Head of linked list </summary>
        [NonSerialized]
        private Node _head;

        /// <summary>Tail of linked list </summary>
        [NonSerialized]
        private Node _last;

        /// <summary>Lock held by take, poll, etc </summary>
        [NonSerialized]
        private readonly object _takeLock = new object();

        /// <summary>Lock held by put, offer, etc </summary>
        [NonSerialized]
        private readonly object _putLock = new object();

        /// <summary> 
        /// Signals a waiting take. Called only from put/offer (which do not
        /// otherwise ordinarily lock _takeLock.)
        /// </summary>
        private void SignalNotEmpty() {
            lock(_takeLock) {
                Monitor.Pulse(_takeLock);
            }
        }

        /// <summary> Signals a waiting put. Called only from take/poll.</summary>
        private void SignalNotFull() {
            lock(_putLock) {
                Monitor.Pulse(_putLock);
            }
        }

        /// <summary> 
        /// Creates a node and links it at end of queue.</summary>
        /// <param name="x">the item to insert</param>
        private void Insert(T x) {
            _last = _last.next = new Node(x);
        }

        /// <summary>Removes a node from head of queue,</summary>
        /// <returns>the node</returns>
        private T Extract() {
            Node first = _head.next;
            _head = first;
            T x = first.item;
            first.item = default(T);
            return x;
        }


        #endregion

        #region ctors

        /// <summary> Creates a <see cref="LinkedBlockingQueue{T}"/> with a capacity of
        /// <see cref="System.Int32.MaxValue"/>.
        /// </summary>
        public LinkedBlockingQueue()
            : this(Int32.MaxValue) {
        }

        /// <summary> Creates a <see cref="LinkedBlockingQueue{T}"/> with the given (fixed) capacity.</summary>
        /// <param name="capacity">the capacity of this queue</param>
        /// <exception cref="System.ArgumentException">if the <paramref name="capacity"/> is not greater than zero.</exception>
        public LinkedBlockingQueue(int capacity) {
            if(capacity <= 0) throw new ArgumentOutOfRangeException(
                    "capacity", capacity, "Capacity must be positive integer.");
            _capacity = capacity;
            _last = _head = new Node(default(T));
        }

        /// <summary> Creates a <see cref="LinkedBlockingQueue{T}"/> with a capacity of
        /// <see cref="System.Int32.MaxValue"/>, initially containing the elements o)f the
        /// given collection, added in traversal order of the collection's iterator.
        /// </summary>
        /// <param name="collection">the collection of elements to initially contain</param>
        /// <exception cref="System.ArgumentNullException">if the collection or any of its elements are null.</exception>
        /// <exception cref="System.ArgumentException">if the collection size exceeds the capacity of this queue.</exception>
        public LinkedBlockingQueue(ICollection<T> collection)
            : this(Int32.MaxValue) {
            if(collection == null) {
                throw new ArgumentNullException("collection", "must not be null.");
            }
            int count = 0;
            foreach (var item in collection)
            {
                Insert(item);
                count++; // we must count ourselves, as collection can change.
            }
            _activeCount = count;
        }

        /// <summary> Reconstitute this queue instance from a stream (that is,
        /// deserialize it).
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> to populate with data. </param>
        /// <param name="context">The destination (see <see cref="System.Runtime.Serialization.StreamingContext"/>) for this serialization. </param>
        protected LinkedBlockingQueue(SerializationInfo info, StreamingContext context)
        {
            MemberInfo[] mi = FormatterServices.GetSerializableMembers(GetType(), context);
            for (int i = 0; i < mi.Length; i++)
            {
                FieldInfo fi = (FieldInfo) mi[i];
                fi.SetValue(this, info.GetValue(fi.Name, fi.FieldType));
            }
            _last = _head = new Node(default(T));

            T[] items = (T[]) info.GetValue("Data", typeof (T[]));
            foreach (var item in items) Insert(item);
            _activeCount = items.Length;
        }

        #endregion

        #region ISerializable Members

        /// <summary>
        ///Populates a <see cref="System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> to populate with data. </param>
        /// <param name="context">The destination (see <see cref="System.Runtime.Serialization.StreamingContext"/>) for this serialization. </param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            lock(_putLock) {
                lock(_takeLock) {
                    SerializationUtilities.DefaultWriteObject(info, context, this);
                    info.AddValue("Data", ToArray());
                }
            }
        }


        #endregion

        #region base class overrides

        /// <summary> 
        /// Inserts the specified element into this queue, waiting if necessary
        /// for space to become available.
        /// </summary>
        /// <param name="element">the element to add</param>
        /// <exception cref="ThreadInterruptedException">
        /// if interrupted while waiting.
        /// </exception>
        public override void Put(T element) {
            int tempCount;
            lock(_putLock) {
                /*
                 * Note that count is used in wait guard even though it is
                 * not protected by lock. This works because count can
                 * only decrease at this point (all other puts are shut
                 * out by lock), and we (or some other waiting put) are
                 * signaled if it ever changes from capacity. Similarly 
                 * for all other uses of count in other wait guards.
                 */
                try
                {
                    while(_activeCount == _capacity)
                        Monitor.Wait(_putLock);
                }
                catch(ThreadInterruptedException e) {
                    Monitor.Pulse(_putLock);
                    throw SystemExtensions.PreserveStackTrace(e);
                }
                Insert(element);
                lock(this) {
                    tempCount = _activeCount++;
                    _version++;
                }
                if(tempCount + 1 < _capacity)
                    Monitor.Pulse(_putLock);
            }

            if(tempCount == 0)
                SignalNotEmpty();
        }

        /// <summary> 
        /// Inserts the specified element into this queue, waiting up to the
        /// specified wait time if necessary for space to become available.
        /// </summary>
        /// <param name="element">the element to add</param>
        /// <param name="duration">how long to wait before giving up</param>
        /// <returns> <see lang="true"/> if successful, or <see lang="false"/> if
        /// the specified waiting time elapses before space is available
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the element cannot be added at this time due to capacity restrictions.
        /// </exception>
        /// <exception cref="ThreadInterruptedException">
        /// if interrupted while waiting.
        /// </exception>
        public override bool Offer(T element, TimeSpan duration) {
            int tempCount;
            lock(_putLock) {
                DateTime deadline = DateTime.UtcNow.Add(duration);
                for(; ; ) {
                    if(_activeCount < _capacity) {
                        Insert(element);
                        lock(this) {
                            tempCount = _activeCount++;
                            _version++;
                        }
                        if(tempCount + 1 < _capacity)
                            Monitor.Pulse(_putLock);
                        break;
                    }
                    if (duration.Ticks <= 0)
                        return false;
                    try {
                        Monitor.Wait(_putLock, duration);
                        duration = deadline.Subtract(DateTime.UtcNow);
                    }
                    catch(ThreadInterruptedException e) {
                        Monitor.Pulse(_putLock);
                        throw SystemExtensions.PreserveStackTrace(e);
                    }
                }
            }
            if(tempCount == 0)
                SignalNotEmpty();
            return true;
        }

        /// <summary> 
        /// Inserts the specified element into this queue if it is possible to do
        /// so immediately without violating capacity restrictions.
        /// </summary>
        /// <remarks>
        /// When using a capacity-restricted queue, this method is generally
        /// preferable to <see cref="Spring.Collections.IQueue.Add(object)"/>,
        /// which can fail to insert an element only by throwing an exception.
        /// </remarks>
        /// <param name="element">
        /// The element to add.
        /// </param>
        /// <returns>
        /// <see lang="true"/> if the element was added to this queue.
        /// </returns>
        public override bool Offer(T element) {
            if(_activeCount == _capacity)
                return false;
            int tempCount = -1;
            lock(_putLock) {
                if(_activeCount < _capacity) {
                    Insert(element);
                    lock(this) {
                        tempCount = _activeCount++;
                        _version++;
                    }
                    if(tempCount + 1 < _capacity)
                        Monitor.Pulse(_putLock);
                }
            }
            if(tempCount == 0)
                SignalNotEmpty();
            return tempCount >= 0;
        }

        /// <summary> 
        /// Retrieves and removes the head of this queue, waiting if necessary
        /// until an element becomes available.
        /// </summary>
        /// <returns> the head of this queue</returns>
        public override T Take() {
            T x;
            int tempCount;
            lock(_takeLock) {
                try {
                    while(_activeCount == 0)
                        Monitor.Wait(_takeLock);
                }
                catch(ThreadInterruptedException e) {
                    Monitor.Pulse(_takeLock);
                    throw SystemExtensions.PreserveStackTrace(e);
                }

                x = Extract();
                lock(this) {
                    tempCount = _activeCount--;
                    _version++;
                }
                if(tempCount > 1)
                    Monitor.Pulse(_takeLock);
            }
            if(tempCount == _capacity)
                SignalNotFull();

            return x;
        }

        /// <summary> 
        /// Retrieves and removes the head of this queue, waiting up to the
        /// specified wait time if necessary for an element to become available.
        /// </summary>
        /// <param name="element">
        /// Set to the head of this queue. <c>default(T)</c> if queue is empty.
        /// </param>
        /// <param name="duration">How long to wait before giving up.</param>
        /// <returns> 
        /// <c>false</c> if the queue is still empty after waited for the time 
        /// specified by the <paramref name="duration"/>. Otherwise <c>true</c>.
        /// </returns>
        public override bool Poll(TimeSpan duration, out T element) {
            T x;
            int c;
            lock(_takeLock) {
                DateTime deadline = DateTime.UtcNow.Add(duration);
                for(; ; ) {
                    if(_activeCount > 0) {
                        x = Extract();
                        lock(this) {
                            c = _activeCount--;
                            _version++;
                        }
                        if(c > 1)
                            Monitor.Pulse(_takeLock);
                        break;
                    }
                    if (duration.Ticks <= 0)
                    {
                        element = default(T);
                        return false;
                    }
                    try {
                        Monitor.Wait(_takeLock, duration);
                        duration = deadline.Subtract(DateTime.UtcNow);
                    }
                    catch(ThreadInterruptedException e) {
                        Monitor.Pulse(_takeLock);
                        throw SystemExtensions.PreserveStackTrace(e);
                    }
                }
            }
            if(c == _capacity)
                SignalNotFull();
            element = x;
            return true;
        }

        /// <summary>
        /// Retrieves and removes the head of this queue into out parameter
        /// <paramref name="element"/>. 
        /// </summary>
        /// <param name="element">
        /// Set to the head of this queue. <c>default(T)</c> if queue is empty.
        /// </param>
        /// <returns>
        /// <c>false</c> if the queue is empty. Otherwise <c>true</c>.
        /// </returns>
        public override bool Poll(out T element) {
            if(_activeCount == 0) {
                element = default(T);
                return false;
            }

            T x = default(T);
            int c = -1;
            lock(_takeLock) {
                if(_activeCount > 0) {
                    x = Extract();
                    lock(this) {
                        c = _activeCount--;
                        _version++;
                    }
                    if(c > 1)
                        Monitor.Pulse(_takeLock);
                }
            }
            if(c == _capacity) {
                SignalNotFull();
            }
            element = x;
            return true;
        }

        /// <summary>
        /// Retrieves, but does not remove, the head of this queue into out
        /// parameter <paramref name="element"/>.
        /// </summary>
        /// <param name="element">
        /// The head of this queue. <c>default(T)</c> if queue is empty.
        /// </param>
        /// <returns>
        /// <c>false</c> is the queue is empty. Otherwise <c>true</c>.
        /// </returns>
        public override bool Peek(out T element) {
            if(_activeCount == 0) {
                element = default(T);
                return false;
            }
            lock(_takeLock) {
                Node first = _head.next;
                bool exists = first != null;
                element = exists ? first.item : default(T);
                return exists;
            }
        }

        /// <summary> 
        /// Removes a single instance of the specified element from this queue,
        /// if it is present.  
        /// </summary>
        /// <remarks> 
        ///	If this queue contains one or more such elements.
        /// Returns <see lang="true"/> if this queue contained the specified element
        /// (or equivalently, if this queue changed as a result of the call).
        /// </remarks>
        /// <param name="objectToRemove">element to be removed from this queue, if present</param>
        /// <returns><see lang="true"/> if this queue changed as a result of the call</returns>
        public override bool Remove(T objectToRemove) {
            bool removed = false;
            lock(_putLock) {
                lock(_takeLock) {
                    Node trail = _head;
                    Node p = _head.next;
                    while(p != null) {
                        if(Equals(objectToRemove, p.item)) {
                            removed = true;
                            break;
                        }
                        trail = p;
                        p = p.next;
                    }
                    if(removed) {
                        p.item = default(T);
                        trail.next = p.next;
                        if(_last == p)
                            _last = trail;
                        lock(this) {
                            _version++;
                            if(_activeCount-- == _capacity)
                                Monitor.PulseAll(_putLock);
                        }
                    }
                }
            }
            return removed;
        }

        /// <summary> 
        /// Returns the number of additional elements that this queue can ideally
        /// (in the absence of memory or resource constraints) accept without
        /// blocking. This is always equal to the initial capacity of this queue
        /// minus the current <see cref="LinkedBlockingQueue{T}.Count"/> of this queue.
        /// </summary>
        /// <remarks> 
        /// Note that you <b>cannot</b> always tell if an attempt to insert
        /// an element will succeed by inspecting <see cref="LinkedBlockingQueue{T}.RemainingCapacity"/>
        /// because it may be the case that another thread is about to
        /// insert or remove an element.
        /// </remarks>
        public override int RemainingCapacity {
            get {
                return _capacity == int.MaxValue ? int.MaxValue : _capacity - _activeCount;
            }
        }

        /// <summary>
        /// Does the real work for the <see cref="AbstractQueue{T}.Drain(System.Action{T})"/>
        /// and <see cref="AbstractQueue{T}.Drain(System.Action{T},Predicate{T})"/>.
        /// </summary>
        protected internal override int DoDrainTo(Action<T> action, Predicate<T> criteria) {
            if (criteria!=null)
            {
                return DoDrainTo(action, int.MaxValue, criteria);
            }
            Node first;
            lock(_putLock) {
                lock(_takeLock) {
                    first = _head.next;
                    _head.next = null;

                    _last = _head;
                    int cold;
                    lock(this) {
                        cold = _activeCount;
                        _activeCount = 0;
                        _version++;
                    }
                    if(cold == _capacity)
                        Monitor.PulseAll(_putLock);
                }
            }
            // Transfer the elements outside of locks
            int n = 0;
            for(Node p = first; p != null; p = p.next) {
                action(p.item);
                p.item = default(T);
                ++n;
            }
            return n;
        }

        /// <summary> 
        /// Does the real work for all drain methods. Caller must
        /// guarantee the <paramref name="action"/> is not <c>null</c> and
        /// <paramref name="maxElements"/> is greater then zero (0).
        /// </summary>
        /// <seealso cref="IQueue{T}.Drain(System.Action{T})"/>
        /// <seealso cref="IQueue{T}.Drain(System.Action{T}, int)"/>
        /// <seealso cref="IQueue{T}.Drain(System.Action{T}, Predicate{T})"/>
        /// <seealso cref="IQueue{T}.Drain(System.Action{T}, int, Predicate{T})"/>
        internal protected override int DoDrainTo(Action<T> action, int maxElements, Predicate<T> criteria)
        {
            lock(_putLock) {
                lock(_takeLock) {
                    int n = 0;
                    Node p = _head;
                    Node c = p.next;
                    while(c != null && n < maxElements) {
                        if (criteria == null || criteria(c.item))
                        {
                            action(c.item);
                            c.item = default(T);
                            p.next = c.next;
                            ++n;
                        }
                        else
                        {
                            p = c;
                        }
                        c = c.next;
                    }
                    if(n != 0) {
                        if(c == null)
                            _last = p;
                        int cold;
                        lock(this) {
                            cold = _activeCount;
                            _activeCount -= n;
                            _version++;
                        }
                        if(cold == _capacity)
                            Monitor.PulseAll(_putLock);
                    }
                    return n;
                }
            }
        }

        /// <summary>
        /// Gets the capacity of this queue.
        /// </summary>
        public override int Capacity {
            get { return _capacity; }
        }

        /// <summary>
        /// Does the actual work of copying to array. Subclass is recommended to 
        /// override this method instead of <see cref="AbstractCollection{T}.CopyTo(T[], int)"/> method, which 
        /// does all neccessary parameter checking and raises proper exception
        /// before calling this method.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the 
        /// destination of the elements copied from <see cref="ICollection{T}"/>. 
        /// The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        protected override void DoCopyTo(T[] array, int arrayIndex) {
            lock(_putLock) {
                lock(_takeLock) {
                    for(Node p = _head.next; p != null; p = p.next)
                        array[arrayIndex++] = p.item;
                }
            }
        }

        /// <summary>
        /// Gets the count of the queue. 
        /// </summary>
        public override int Count {
            get { return _activeCount; }
        }

        /// <summary>
        /// test whether the queue contains <paramref name="item"/> 
        /// </summary>
        /// <param name="item">the item whose containement should be checked</param>
        /// <returns><c>true</c> if item is in the queue, <c>false</c> otherwise</returns>
        public override bool Contains(T item) {
            lock (_putLock)
            {
                lock (_takeLock)
                {
                    for (Node p = _head.next; p!=null; p = p.next)
                    {
                        if (Equals(item, p.item)) return true;
                    }
                }
            }
            return false;
        }

        #endregion

        /// <summary> 
        /// Returns an array containing all of the elements in this queue, in
        /// proper sequence.
        /// </summary>
        /// <remarks> 
        /// The returned array will be "safe" in that no references to it are
        /// maintained by this queue.  (In other words, this method must allocate
        /// a new array).  The caller is thus free to modify the returned array.
        /// 
        /// <p/>
        /// This method acts as bridge between array-based and collection-based
        /// APIs.
        /// </remarks>
        /// <returns> an array containing all of the elements in this queue</returns>
        public virtual T[] ToArray() {
            lock(_putLock) {
                lock(_takeLock) {
                    int size = _activeCount;
                    T[] a = new T[size];
                    int k = 0;
                    for(Node p = _head.next; p != null; p = p.next)
                        a[k++] = p.item;
                    return a;
                }
            }
        }

        /// <summary>
        /// Returns an array containing all of the elements in this queue, in
        /// proper sequence; the runtime type of the returned array is that of
        /// the specified array.  If the queue fits in the specified array, it
        /// is returned therein.  Otherwise, a new array is allocated with the
        /// runtime type of the specified array and the size of this queue.
        ///	</summary>	 
        /// <remarks>
        /// If this queue fits in the specified array with room to spare
        /// (i.e., the array has more elements than this queue), the element in
        /// the array immediately following the end of the queue is set to
        /// <see lang="null"/>.
        /// <p/>
        /// Like the <see cref="LinkedBlockingQueue{T}.ToArray()"/>  method, this method acts as bridge between
        /// array-based and collection-based APIs.  Further, this method allows
        /// precise control over the runtime type of the output array, and may,
        /// under certain circumstances, be used to save allocation costs.
        /// <p/>
        /// Suppose <i>x</i> is a queue known to contain only strings.
        /// The following code can be used to dump the queue into a newly
        /// allocated array of <see lang="string"/>s:
        /// 
        /// <code>
        ///		string[] y = x.ToArray(new string[0]);
        ///	</code>
        ///	<p/>	
        /// Note that <i>toArray(new object[0])</i> is identical in function to
        /// <see cref="LinkedBlockingQueue{T}.ToArray()"/>.
        /// 
        /// </remarks>
        /// <param name="targetArray">
        /// the array into which the elements of the queue are to
        /// be stored, if it is big enough; otherwise, a new array of the
        /// same runtime type is allocated for this purpose
        /// </param>
        /// <returns> an array containing all of the elements in this queue</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="targetArray"/> is
        /// <see lang="null"/> and this queue does not permit <see lang="null"/>
        /// elements.
        /// </exception>
        public virtual T[] ToArray(T[] targetArray) {
            if (targetArray == null) throw new ArgumentNullException("targetArray");
            lock(_putLock) {
                lock(_takeLock) {
                    int size = _activeCount;
                    if(targetArray.Length < size)
                        targetArray = new T[size];

                    int k = 0;
                    for(Node p = _head.next; p != null; p = p.next)
                        targetArray[k++] = p.item;
                    return targetArray;
                }
            }
        }

        /// <summary>
        /// Returns a string representation of this colleciton.
        /// </summary>
        /// <returns>String representation of the elements of this collection.</returns>
        public override string ToString() {
            lock(_putLock) {
                lock(_takeLock) {
                    return base.ToString();
                }
            }
        }

        /// <summary> 
        /// Removes all of the elements from this queue.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The queue will be empty after this call returns.
        /// </p>
        /// <p>
        /// This implementation repeatedly invokes
        /// <see cref="Spring.Collections.AbstractQueue.Poll()"/> until it
        /// returns <see lang="null"/>.
        /// </p>
        /// </remarks>
        public override void Clear() {
            lock(_putLock) {
                lock(_takeLock) {
                    _head.next = null;

                    _last = _head;
                    int c;
                    lock(this) {
                        c = _activeCount;
                        _activeCount = 0;
                        _version++;
                    }
                    if(c == _capacity)
                        Monitor.PulseAll(_putLock);
                }
            }
        }

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that can iterate through a collection.
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        public override IEnumerator<T> GetEnumerator() {
            return new LinkedBlockingQueueEnumerator(this);
        }


        /// <summary>
        /// Internal enumerator class
        /// </summary>
        private class LinkedBlockingQueueEnumerator : AbstractEnumerator<T> {
            private readonly LinkedBlockingQueue<T> _queue;
            private Node _currentNode;
            private readonly int _startVersion;
            private T _currentElement;

            internal LinkedBlockingQueueEnumerator(LinkedBlockingQueue<T> queue) {
                _queue = queue;
                lock(_queue._putLock) {
                    lock(_queue._takeLock) {
                        _currentNode = _queue._head;
                        _startVersion = _queue._version;
                    }
                }
            }

            protected override T FetchCurrent()
            {
                return _currentElement;
            }

            protected override bool GoNext()
            {
                lock (_queue._putLock)
                {
                    lock (_queue._takeLock)
                    {
                        CheckChange();
                        _currentNode = _currentNode.next;
                        if (_currentNode == null) return false;
                        _currentElement = _currentNode.item;
                        return true;
                    }
                }
            }

            public override void Reset() {
                lock(_queue._putLock) {
                    lock(_queue._takeLock)
                    {
                        CheckChange();
                        _currentNode = _queue._head;
                    }
                }
            }

            private void CheckChange()
            {
                lock (_queue)
                {
                    if(_startVersion != _queue._version)
                        throw new InvalidOperationException(
                            "queue has changed during enumeration");
                }
            }
        }

        #endregion
    }
}