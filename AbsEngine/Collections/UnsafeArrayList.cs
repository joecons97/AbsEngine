using System.Collections;
using System.Runtime.CompilerServices;

namespace AbsEngine.Collections;

public class UnsafeArrayList : IEnumerable
{
    private object?[] _items; // Do not rename (binary serialization)
    private int _size; // Do not rename (binary serialization)
    private int _version; // Do not rename (binary serialization)

    private const int _defaultCapacity = 4;

    // Constructs a ArrayList. The list is initially empty and has a capacity
    // of zero. Upon adding the first element to the list the capacity is
    // increased to _defaultCapacity, and then increased in multiples of two as required.
    public UnsafeArrayList()
    {
        _items = Array.Empty<object>();
    }

    // Constructs a ArrayList with a given initial capacity. The list is
    // initially empty, but will have room for the given number of elements
    // before any reallocations are required.
    //
    public UnsafeArrayList(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity), "ArgumentOutOfRange_MustBeNonNegNum");

        if (capacity == 0)
            _items = Array.Empty<object>();
        else
            _items = new object[capacity];
    }

    // Constructs a ArrayList, copying the contents of the given collection. The
    // size and capacity of the new list will both be equal to the size of the
    // given collection.
    //
    public UnsafeArrayList(ICollection c)
    {
        ArgumentNullException.ThrowIfNull(c);

        int count = c.Count;
        if (count == 0)
        {
            _items = Array.Empty<object>();
        }
        else
        {
            _items = new object[count];
            AddRange(c);
        }
    }

    public T[] UnsafeConvert<T>()
    {
        return Unsafe.As<T[]>(_items);
    }

    // Gets and sets the capacity of this list.  The capacity is the size of
    // the internal array used to hold items.  When set, the internal
    // array of the list is reallocated to the given capacity.
    //
    public virtual int Capacity
    {
        get => _items.Length;
        set
        {
            if (value < _size)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "ArgumentOutOfRange_SmallCapacity");
            }

            // We don't want to update the version number when we change the capacity.
            // Some existing applications have dependency on this.
            if (value != _items.Length)
            {
                if (value > 0)
                {
                    object[] newItems = new object[value];
                    if (_size > 0)
                    {
                        Array.Copy(_items, newItems, _size);
                    }
                    _items = newItems;
                }
                else
                {
                    _items = new object[_defaultCapacity];
                }
            }
        }
    }

    // Read-only property describing how many elements are in the List.
    public virtual int Count => _size;

    public virtual bool IsFixedSize => false;


    // Is this ArrayList read-only?
    public virtual bool IsReadOnly => false;

    // Is this ArrayList synchronized (thread-safe)?
    public virtual bool IsSynchronized => false;

    // Synchronization root for this object.
    public virtual object SyncRoot => this;

    // Sets or Gets the element at the given index.
    //
    public virtual object? this[int index]
    {
        get
        {
            if (index < 0 || index >= _size) throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_IndexMustBeLess");
            return _items[index];
        }
        set
        {
            if (index < 0 || index >= _size) throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_IndexMustBeLess");
            _items[index] = value;
            _version++;
        }
    }

    // Adds the given object to the end of this list. The size of the list is
    // increased by one. If required, the capacity of the list is doubled
    // before adding the new element.
    //
    public virtual int Add(object? value)
    {
        if (_size == _items.Length) EnsureCapacity(_size + 1);
        _items[_size] = value;
        _version++;
        return _size++;
    }

    // Adds the elements of the given collection to the end of this list. If
    // required, the capacity of the list is increased to twice the previous
    // capacity or the new size, whichever is larger.
    //
    public virtual void AddRange(ICollection c)
    {
        InsertRange(_size, c);
    }

    // Clears the contents of ArrayList.
    public virtual void Clear()
    {
        if (_size > 0)
        {
            Array.Clear(_items, 0, _size); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
            _size = 0;
        }
        _version++;
    }

    // Contains returns true if the specified element is in the ArrayList.
    // It does a linear, O(n) search.  Equality is determined by calling
    // item.Equals().
    //
    public virtual bool Contains(object? item) => Array.IndexOf(_items, item, 0, _size) >= 0;

    // Copies this ArrayList into array, which must be of a
    // compatible array type.
    //
    public virtual void CopyTo(Array array) => CopyTo(array, 0);

    // Copies this ArrayList into array, which must be of a
    // compatible array type.
    //
    public virtual void CopyTo(Array array, int arrayIndex)
    {
        if ((array != null) && (array.Rank != 1))
            throw new ArgumentException("Arg_RankMultiDimNotSupported", nameof(array));

        // Delegate rest of error checking to Array.Copy.
        Array.Copy(_items, 0, array!, arrayIndex, _size);
    }

    // Copies a section of this list to the given array at the given index.
    //
    // The method uses the Array.Copy method to copy the elements.
    //
    public virtual void CopyTo(int index, Array array, int arrayIndex, int count)
    {
        if (_size - index < count)
            throw new ArgumentException("Argument_InvalidOffLen");
        if ((array != null) && (array.Rank != 1))
            throw new ArgumentException("Arg_RankMultiDimNotSupported", nameof(array));

        // Delegate rest of error checking to Array.Copy.
        Array.Copy(_items, index, array!, arrayIndex, count);
    }

    // Ensures that the capacity of this list is at least the given minimum
    // value. If the current capacity of the list is less than min, the
    // capacity is increased to twice the current capacity or to min,
    // whichever is larger.
    private void EnsureCapacity(int min)
    {
        if (_items.Length < min)
        {
            int newCapacity = _items.Length == 0 ? _defaultCapacity : _items.Length * 2;
            // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
            if (newCapacity < min) newCapacity = min;
            Capacity = newCapacity;
        }
    }

    // Returns the index of the first occurrence of a given value in a range of
    // this list. The list is searched forwards from beginning to end.
    // The elements of the list are compared to the given value using the
    // Object.Equals method.
    //
    // This method uses the Array.IndexOf method to perform the
    // search.
    //
    public virtual int IndexOf(object? value)
    {
        return Array.IndexOf((Array)_items, value, 0, _size);
    }

    // Returns the index of the first occurrence of a given value in a range of
    // this list. The list is searched forwards, starting at index
    // startIndex and ending at count number of elements. The
    // elements of the list are compared to the given value using the
    // Object.Equals method.
    //
    // This method uses the Array.IndexOf method to perform the
    // search.
    //
    public virtual int IndexOf(object? value, int startIndex)
    {
        if (startIndex > _size)
            throw new ArgumentOutOfRangeException(nameof(startIndex), "ArgumentOutOfRange_IndexMustBeLessOrEqual");
        return Array.IndexOf((Array)_items, value, startIndex, _size - startIndex);
    }

    // Returns the index of the first occurrence of a given value in a range of
    // this list. The list is searched forwards, starting at index
    // startIndex and up to count number of elements. The
    // elements of the list are compared to the given value using the
    // Object.Equals method.
    //
    // This method uses the Array.IndexOf method to perform the
    // search.
    //
    public virtual int IndexOf(object? value, int startIndex, int count)
    {
        if (startIndex > _size)
            throw new ArgumentOutOfRangeException(nameof(startIndex), "ArgumentOutOfRange_IndexMustBeLessOrEqual");
        if (count < 0 || startIndex > _size - count) throw new ArgumentOutOfRangeException(nameof(count), "ArgumentOutOfRange_Count");
        return Array.IndexOf((Array)_items, value, startIndex, count);
    }

    // Inserts an element into this list at a given index. The size of the list
    // is increased by one. If required, the capacity of the list is doubled
    // before inserting the new element.
    //
    public virtual void Insert(int index, object? value)
    {
        // Note that insertions at the end are legal.
        if (index < 0 || index > _size) throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_IndexMustBeLessOrEqual");

        if (_size == _items.Length) EnsureCapacity(_size + 1);
        if (index < _size)
        {
            Array.Copy(_items, index, _items, index + 1, _size - index);
        }
        _items[index] = value;
        _size++;
        _version++;
    }

    // Inserts the elements of the given collection at a given index. If
    // required, the capacity of the list is increased to twice the previous
    // capacity or the new size, whichever is larger.  Ranges may be added
    // to the end of the list by setting index to the ArrayList's size.
    //
    public virtual void InsertRange(int index, ICollection c)
    {
        ArgumentNullException.ThrowIfNull(c);

        if (index < 0 || index > _size) throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_IndexMustBeLessOrEqual");

        int count = c.Count;
        if (count > 0)
        {
            EnsureCapacity(_size + count);
            // shift existing items
            if (index < _size)
            {
                Array.Copy(_items, index, _items, index + count, _size - index);
            }

            object[] itemsToInsert = new object[count];
            c.CopyTo(itemsToInsert, 0);
            itemsToInsert.CopyTo(_items, index);
            _size += count;
            _version++;
        }
    }

    // Returns the index of the last occurrence of a given value in a range of
    // this list. The list is searched backwards, starting at the end
    // and ending at the first element in the list. The elements of the list
    // are compared to the given value using the Object.Equals method.
    //
    // This method uses the Array.LastIndexOf method to perform the
    // search.
    //
    public virtual int LastIndexOf(object? value)
    {
        return LastIndexOf(value, _size - 1, _size);
    }

    // Returns the index of the last occurrence of a given value in a range of
    // this list. The list is searched backwards, starting at index
    // startIndex and ending at the first element in the list. The
    // elements of the list are compared to the given value using the
    // Object.Equals method.
    //
    // This method uses the Array.LastIndexOf method to perform the
    // search.
    //
    public virtual int LastIndexOf(object? value, int startIndex)
    {
        if (startIndex >= _size)
            throw new ArgumentOutOfRangeException(nameof(startIndex), "ArgumentOutOfRange_IndexMustBeLess");
        return LastIndexOf(value, startIndex, startIndex + 1);
    }

    // Returns the index of the last occurrence of a given value in a range of
    // this list. The list is searched backwards, starting at index
    // startIndex and up to count elements. The elements of
    // the list are compared to the given value using the Object.Equals
    // method.
    //
    // This method uses the Array.LastIndexOf method to perform the
    // search.
    //
    public virtual int LastIndexOf(object? value, int startIndex, int count)
    {
        if (Count != 0)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (_size == 0)  // Special case for an empty list
            return -1;

        if (startIndex >= _size || count > startIndex + 1)
            throw new ArgumentOutOfRangeException(startIndex >= _size ? nameof(startIndex) : nameof(count), "SR.ArgumentOutOfRange_BiggerThanCollection");

        return Array.LastIndexOf((Array)_items, value, startIndex, count);
    }

    // Removes the element at the given index. The size of the list is
    // decreased by one.
    //
    public virtual void Remove(object? obj)
    {
        int index = IndexOf(obj);
        if (index >= 0)
            RemoveAt(index);
    }

    // Removes the element at the given index. The size of the list is
    // decreased by one.
    //
    public virtual void RemoveAt(int index)
    {
        if (index < 0 || index >= _size) throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_IndexMustBeLess");

        _size--;
        if (index < _size)
        {
            Array.Copy(_items, index + 1, _items, index, _size - index);
        }
        _items[_size] = null;
        _version++;
    }

    // Removes a range of elements from this list.
    //
    public virtual void RemoveRange(int index, int count)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (_size - index < count)
            throw new ArgumentException("Argument_InvalidOffLen");

        if (count > 0)
        {
            int i = _size;
            _size -= count;
            if (index < _size)
            {
                Array.Copy(_items, index + count, _items, index, _size - index);
            }
            while (i > _size) _items[--i] = null;
            _version++;
        }
    }

    // ToArray returns a new Object array containing the contents of the ArrayList.
    // This requires copying the ArrayList, which is an O(n) operation.
    public virtual object?[] ToArray()
    {
        if (_size == 0)
            return Array.Empty<object>();

        object?[] array = new object[_size];
        Array.Copy(_items, array, _size);
        return array;
    }

    public T?[] ToArray<T>()
    {
        if (_size == 0)
            return Array.Empty<T>();

        T?[] array = new T[_size];
        Array.Copy(_items, array, _size);
        return array;
    }

    public IEnumerator GetEnumerator()
    {
        return new UnsafeArrayListEnumeratorSimple(this);
    }

    private sealed class UnsafeArrayListEnumeratorSimple : IEnumerator, ICloneable
    {
        private readonly UnsafeArrayList _list;
        private int _index;
        private readonly int _version;
        private object? _currentElement;
        private readonly bool _isArrayList;
        // this object is used to indicate enumeration has not started or has terminated
        private static readonly object s_dummyObject = new object();

        internal UnsafeArrayListEnumeratorSimple(UnsafeArrayList list)
        {
            _list = list;
            _index = -1;
            _version = list._version;
            _isArrayList = (list.GetType() == typeof(ArrayList));
            _currentElement = s_dummyObject;
        }

        public object Clone() => MemberwiseClone();

        public bool MoveNext()
        {
            if (_version != _list._version)
            {
                throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
            }

            if (_isArrayList)
            {  // avoid calling virtual methods if we are operating on ArrayList to improve performance
                if (_index < _list._size - 1)
                {
                    _currentElement = _list._items[++_index];
                    return true;
                }
                else
                {
                    _currentElement = s_dummyObject;
                    _index = _list._size;
                    return false;
                }
            }
            else
            {
                if (_index < _list.Count - 1)
                {
                    _currentElement = _list[++_index];
                    return true;
                }
                else
                {
                    _index = _list.Count;
                    _currentElement = s_dummyObject;
                    return false;
                }
            }
        }

        public object? Current
        {
            get
            {
                object? temp = _currentElement;
                if (s_dummyObject == temp)
                { // check if enumeration has not started or has terminated
                    if (_index == -1)
                    {
                        throw new InvalidOperationException("InvalidOperation_EnumNotStarted");
                    }
                    else
                    {
                        throw new InvalidOperationException("InvalidOperation_EnumEnded");
                    }
                }

                return temp;
            }
        }

        public void Reset()
        {
            if (_version != _list._version)
            {
                throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
            }

            _currentElement = s_dummyObject;
            _index = -1;
        }
    }

}

