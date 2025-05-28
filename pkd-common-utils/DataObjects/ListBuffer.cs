namespace pkd_common_utils.DataObjects;

/// <summary>
/// A blocking buffer implementation using a <see cref="List{T}"/> object for data management. This blocks any read/write actions
/// while performing any operation.
/// </summary>
/// <typeparam name="T">The type of data object to store in the list buffer.</typeparam>
public class ListBuffer<T>
{
    private readonly List<T> _list = [];
    private readonly object _lockObject = new();
    
    /// <summary>
    /// Contains the exception message as of the last read or write failure. This will be an empty string if the last
    /// method call was a success or no read/write has been attempted yet.
    /// </summary>
    public string ReadWriteFailureReason { get; private set; } = string.Empty;

    /// <summary>
    /// Try to add an item to the current collection. This action is blocked until the buffer is released and safe to write to.
    /// </summary>
    /// <param name="item">The object to add to the collection. Cannot be null.</param>
    /// <returns>true if the object was added successfully. False if not.</returns>
    /// <remarks>
    /// On a failed add the exception message is assigned to <see cref="ReadWriteFailureReason"/>. If the add was successful
    /// then <see cref="ReadWriteFailureReason"/> is set to the empty string.
    /// </remarks>
    public bool AddItem(T item)
    {
        var result = false;
        lock (_lockObject)
        {
            try
            {
                _list.Add(item);
                result = true;
                ReadWriteFailureReason = string.Empty;
            }
            catch (Exception e)
            {
                ReadWriteFailureReason = e.Message;
            }
        }

        return result;
    }
    
    /// <summary>
    /// Try to add a collection of items to the list buffer. On a failure a message is written to <see cref="ReadWriteFailureReason"/>.
    /// </summary>
    /// <param name="items">The collection of items to add.</param>
    /// <returns>true if the items were successfully added to the collection, false otherwise.</returns>
    public bool AddItems(IEnumerable<T> items)
    {
        var result = false;
        lock (_lockObject)
        {
            try
            {
                _list.AddRange(items);
                result = true;
                ReadWriteFailureReason = string.Empty;
            }
            catch (Exception e)
            {
                ReadWriteFailureReason = e.Message;
            }
        }

        return result;
    }

    /// <summary>
    /// Check the current list buffer to see if the given item is already in the collection. This calls the List.Contains()
    /// method. On any exception, the message is written to <see cref="ReadWriteFailureReason"/> and false is return.
    /// </summary>
    /// <param name="item">The object that will be checked against the list buffer.</param>
    /// <returns>True if the object exists in the collection, false otherwise.</returns>
    /// <remarks>If the evaluation successfully returns false (no object in collection), then <see cref="ReadWriteFailureReason"/>
    /// will be set to the empty string.</remarks>
    public bool CheckExists(T item)
    {
        var result = false;
        lock (_lockObject)
        {
            try
            {
                result = _list.Contains(item);
                ReadWriteFailureReason = string.Empty;
            }
            catch (Exception e)
            {
                ReadWriteFailureReason = e.Message;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Check to see if there is a matching sequence of elements in the internal buffer. The order of elements must match
    /// for this to return true.
    /// </summary>
    /// <param name="items">the sequence of items to compare.</param>
    /// <returns>true if there is a matching sequence in the buffer, false otherwise.</returns>
    public bool CheckExists(IList<T> items)
    {
        var result = false;
        lock (_lockObject)
        {
            try
            {
                if (_list.Count < items.Count) return false;
                for (var i = 0; i <= _list.Count - items.Count; i++)
                {
                    if (!_list.GetRange(i, items.Count).SequenceEqual(items)) continue;
                    result = true;
                    break;
                }
            }
            catch (Exception e)
            {
                ReadWriteFailureReason = e.Message;
            }
        }
        return result;
    }

    /// <summary>
    /// Get the current number of elements in the buffer.
    /// </summary>
    /// <returns>The current number of elements in the buffer.</returns>
    public int GetLength()
    {
        lock (_lockObject)
        {
            return _list.Count;
        }
    }
    
    /// <summary>
    /// Try to remove a section of the buffer by a given length. This will start at the first item in the collection (index 0)
    /// and remove all items up to the supplied length. A failure message is written to <see cref="ReadWriteFailureReason"/> if
    /// 'lenght' is less than zero or greater than the number of items in the buffer.
    /// </summary>
    /// <param name="length">The number if items to remove from the buffer. Cannot be less than zero or greater than the total number
    /// of items in the buffer.</param>
    /// <returns>a list containing all items removed or an empty list if an error is encountered.</returns>
    public List<T> RemoveByLength(int length)
    {
        if (length < 1)
        {
            ReadWriteFailureReason = "RemoveByLength() - Length must be greater than zero.";
            return [];
        }
        
        List<T> removed = [];
        lock (_lockObject)
        {
            if (_list.Count == 0 || length > _list.Count)
            {
                ReadWriteFailureReason = "RemoveByLength() - Length is greater than the number of items in the list.";
            }
            else
            {
                try
                {
                    removed = _list[..length];
                    _list.RemoveRange(0, length);
                    ReadWriteFailureReason = string.Empty;
                }
                catch (Exception e)
                {
                    ReadWriteFailureReason = e.Message;
                } 
            }
        }

        return removed;
    }

    /// <summary>
    /// Remove items from the buffer, starting at index 0, up to the first item that matches the supplied delimiter.
    /// The evaluation is conducted using the <see cref="List{T}"/> IndexOf() method./>
    /// </summary>
    /// <param name="delimiter">The item to look for when removing.</param>
    /// <returns>A list of items up to the first occurrence of the delimiter. Returns an empty list if there is no match or if
    /// an error is encountered.</returns>
    /// <remarks>Writes a message to <see cref="ReadWriteFailureReason"/> if any error is encountered.</remarks>
    public List<T> RemoveByDelimiter(T delimiter)
    {
        List<T> removed = [];
        lock (_lockObject)
        {
            try
            {
                var eolIndex = _list.IndexOf(delimiter);
                if (eolIndex < 0)
                {
                    ReadWriteFailureReason = "RemoveByDelimiter() - Delimiter not found in the current buffer.";
                }
                else
                {
                    eolIndex = eolIndex == 0 ? 0 : eolIndex + 1; // include delimiter
                    removed = _list[..eolIndex];
                    _list.RemoveRange(0, removed.Count);
                    ReadWriteFailureReason = string.Empty;
                }
            }
            catch (Exception e)
            {
                ReadWriteFailureReason = e.Message;
            }
        }

        return removed;
    }

    /// <summary>
    /// Get a copy of the elements in the buffer without removing them.
    /// </summary>
    /// <param name="length">The total number of elements to return.</param>
    /// <returns>
    /// the number of elements up to and including the element at length value. returns an empty array if length is
    /// less than zero.
    /// </returns>
    public T[] PeakByLength(int length)
    {
        var elements = new T[length];
        if (length < 1)
        {
            ReadWriteFailureReason = "PeakByLength() - Length must be greater than zero.";
            return elements;
        }

        lock (_lockObject)
        {
            try
            {
                _list.CopyTo( 0, elements, 0, length);
            }
            catch (Exception e)
            {
                ReadWriteFailureReason = e.Message;
            }
        }

        return elements;
    }
}