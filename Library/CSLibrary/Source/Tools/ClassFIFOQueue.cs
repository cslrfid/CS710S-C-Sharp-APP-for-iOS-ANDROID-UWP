/*
Copyright (c) 2025 Convergence Systems Limited

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;

namespace CSLibrary.Tools
{
    /// <summary>
    /// High-performance thread-safe FIFO byte queue implemented without unsafe code or pinned buffers.
    /// This is a managed alternative to the unsafe HPFIFOQueue implementation and exposes the same surface
    /// behavior (Append/Peek/Read/Seek/Search/Clear/ToHeader) but uses safe array operations.
    /// </summary>
    public sealed class FIFOQueue
    {
        private readonly object _syncLock = new object();
        private readonly byte[] _buffer;
        private int _head;
        private int _count;
        private readonly int _capacity;

        /// <summary>
        /// Gets the total capacity of the FIFO queue
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        /// Gets the number of bytes currently stored in the queue
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Gets the available free space in the queue
        /// </summary>
        public int Available => _capacity - _count;

        /// <summary>
        /// Initializes a new instance of the FIFO queue with specified capacity.
        /// Throws ArgumentOutOfRangeException if capacity is not positive (keeps parity with original).
        /// </summary>
        /// <param name="capacity">Maximum number of bytes the queue can hold</param>
        public FIFOQueue(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");

            _capacity = capacity;
            _buffer = new byte[capacity];
            _head = 0;
            _count = 0;
        }

        public byte Append(byte[] data)
        {
            return Append(data, 0, data.Length);
        }

        public byte Append(byte[] data, int lenth)
        {
            return Append(data, 0, lenth);
        }

        /// <summary>
        /// Appends data to the end of the queue.
        /// Returns 1 if successful, 0 if insufficient space.
        /// Returns ArgumentNullException if data is null (keeps parity with original).
        /// </summary>
        public byte Append(byte[] data, int offset, int length)
        {
            if (data == null || length == 0)
                return 0;

            if (length < 0)
                length = data.Length;

            if (offset + length > data.Length)
                return 0;

            lock (_syncLock)
            {
                if (length > Available)
                    return 0;

                int tail = (_head + _count) % _capacity;
                int contig = _capacity - tail;
                int toCopy = length;

                if (contig >= toCopy)
                {
                    Buffer.BlockCopy(data, offset, _buffer, tail, toCopy);
                }
                else
                {
                    // first segment to end of buffer
                    Buffer.BlockCopy(data, offset, _buffer, tail, contig);
                    // remaining to start of buffer
                    Buffer.BlockCopy(data, offset + contig, _buffer, 0, toCopy - contig);
                }

                _count += toCopy;
                return 1;
            }
        }

        public byte[] Peek(int length)
        {
            return Peek(0, length);
        }

        /// <summary>
        /// Retrieves data from the queue without removing it.
        /// Returns null on invalid input.
        /// </summary>
        public byte[] Peek(int offset, int length)
        {
            if (offset < 0 || length < 0)
                return null;

            if (length == 0)
                return new byte[0];

            lock (_syncLock)
            {
                if (offset + length > _count)
                    return null;

                byte[] result = new byte[length];

                int sourceIndex = (_head + offset) % _capacity;
                int contig = Math.Min(length, _capacity - sourceIndex);

                if (contig == length)
                {
                    Buffer.BlockCopy(_buffer, sourceIndex, result, 0, length);
                }
                else
                {
                    Buffer.BlockCopy(_buffer, sourceIndex, result, 0, contig);
                    Buffer.BlockCopy(_buffer, 0, result, contig, length - contig);
                }

                return result;
            }
        }

        /// <summary>
        /// Advances the queue head position by the specified length.
        /// Returns actual number of bytes advanced, or -1 on invalid input.
        /// </summary>
        public int Seek(int length)
        {
            if (length < 0)
                return -1;

            lock (_syncLock)
            {
                if (length > _count)
                    return -1;

                _head = (_head + length) % _capacity;
                _count -= length;
                return length;
            }
        }

        /// <summary>
        /// Retrieves and removes data from the front of the queue.
        /// Returns null on invalid input.
        /// </summary>
        public byte[] Read(int length)
        {
            if (length < 0)
                return null;

            if (length == 0)
                return new byte[0];

            lock (_syncLock)
            {
                if (length > _count)
                    return null;

                byte[] result = new byte[length];

                int contig = Math.Min(length, _capacity - _head);

                if (contig == length)
                {
                    Buffer.BlockCopy(_buffer, _head, result, 0, length);
                }
                else
                {
                    Buffer.BlockCopy(_buffer, _head, result, 0, contig);
                    Buffer.BlockCopy(_buffer, 0, result, contig, length - contig);
                }

                _head = (_head + length) % _capacity;
                _count -= length;

                return result;
            }
        }

        /// <summary>
        /// Searches for the first occurrence of a specific byte.
        /// Returns zero-based position relative to head, or -1 if not found.
        /// </summary>
        public int Search(byte pattern)
        {
            lock (_syncLock)
            {
                if (_count < 1)
                    return -1;

                int segment1Length = Math.Min(_count, _capacity - _head);

                // search segment 1
                for (int i = 0; i < segment1Length; i++)
                    if (_buffer[_head + i] == pattern)
                        return i;

                // search segment 2 (wrapped)
                int segment2Length = _count - segment1Length;
                for (int i = 0; i < segment2Length; i++)
                    if (_buffer[i] == pattern)
                        return segment1Length + i;

                return -1;
            }
        }

        public int SearchMulti(byte [] pattern)
        {
            lock (_syncLock)
            {
                if (_count < 1)
                    return -1;

                int segment1Length = Math.Min(_count, _capacity - _head);

                // search segment 1
                for (int i = 0; i < segment1Length; i++)
                    for (int j = 0; j < pattern.Length; j++)
                        if (_buffer[_head + i] == pattern[j])
                            return i;

                // search segment 2 (wrapped)
                int segment2Length = _count - segment1Length;
                for (int i = 0; i < segment2Length; i++)
                    for (int j = 0; j < pattern.Length; j++)
                        if (_buffer[i] == pattern[j])
                            return segment1Length + i;

                return -1;
            }
        }

        /// <summary>
        /// Clears all data from the queue.
        /// </summary>
        public void Clear()
        {
            lock (_syncLock)
            {
                _head = 0;
                _count = 0;
            }
        }

        /// <summary>
        /// Advances the queue to the first occurrence of the specified header byte.
        /// Returns true if header was found and queue advanced; false if header not found (queue cleared).
        /// </summary>
        public bool ToHeader(byte header)
        {
            lock (_syncLock)
            {
                int pos = Search(header);
                if (pos == -1)
                {
                    Clear();
                    return false;
                }

                Seek(pos);
                return true;
            }
        }

        public bool ToMultiHeader(byte [] header)
        {
            lock (_syncLock)
            {
                int pos = SearchMulti (header);
                if (pos == -1)
                {
                    Clear();
                    return false;
                }

                Seek(pos);
                return true;
            }
        }
    }
}