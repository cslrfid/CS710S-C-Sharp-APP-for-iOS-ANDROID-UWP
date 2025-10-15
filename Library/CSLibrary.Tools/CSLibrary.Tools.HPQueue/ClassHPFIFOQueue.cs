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

/* Framework requirements by Mephist
1. wrtie c# 7.3 code of byte fifo buffer class library with detail English comment for manual/documents generator
1.1 namespace "CSLibrary.Tools" and class name "HPFIFOQueue"  
2. fastest and High Performance and use unsafe pointer, Thread-Safe, cross platform, NO IDisposable
3. All APIs, needs verify input parameters for over or under Queue size, use return null or -1 and NO ArgumentOutOfRangeException if error, Thread-Safe (Lock once at the outer layer if needed, internal functions do not lock again)
3.1 byte Append (byte [], int length)
3.2 byte [] Peek (int offset, int length)
3.3 int Seek (int length)
3.4 byte [] Read (int length) // Peek(internal) and Seek(internal) data
3.5 int Search (byte) // pure point e.g. *ptr++ == patten, segment1 search from "first position of data" to "end of queue", and then segment2 search from "start of queue" to "last data position" if needed.
3.6 void Clear () // clear queue
3.7 bool ToHeader (byte header) // Search(internal) header and Seek(internal) all data before header, if no header then call Clear(internal)
*/


using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CSLibrary.Tools
{
    /// <summary>
    /// High-performance thread-safe FIFO byte queue using unsafe pointer operations
    /// </summary>
    public sealed class HPFIFOQueue
    {
        private readonly object _syncLock = new object();
        private readonly byte[] _buffer;
        private unsafe byte* _pBuffer;
        private int _head;
        private int _count;
        private readonly int _capacity;
        private readonly GCHandle _bufferHandle;

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
        /// Initializes a new instance of the FIFO queue with specified capacity
        /// </summary>
        /// <param name="capacity">Maximum number of bytes the queue can hold</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is not positive</exception>
        public unsafe HPFIFOQueue(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");

            _capacity = capacity;
            _buffer = new byte[capacity];
            _bufferHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
            _pBuffer = (byte*)_bufferHandle.AddrOfPinnedObject();
            _head = 0;
            _count = 0;
        }

        /// <summary>
        /// Appends data to the end of the queue using memory copy
        /// </summary>
        /// <param name="data">Byte array to append</param>
        /// <returns>1 if successful, 0 if insufficient space</returns>
        /// <exception cref="ArgumentNullException">Thrown when input data is null</exception>
        public unsafe byte Append(byte[] data, int dataLength)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (dataLength == 0)
                return 1;

            lock (_syncLock)
            {
                if (dataLength > Available)
                    return 0;

                int tail = (_head + _count) % _capacity;
                int contig = _capacity - tail;
                int toCopy = dataLength;

                if (contig >= toCopy)
                {
                    fixed (byte* pData = data)
                    {
                        Buffer.MemoryCopy(
                            pData,
                            _pBuffer + tail,
                            contig,
                            toCopy
                        );
                    }
                }
                else
                {
                    fixed (byte* pData = data)
                    {
                        // Copy first segment to end of buffer
                        Buffer.MemoryCopy(
                            pData,
                            _pBuffer + tail,
                            contig,
                            contig
                        );

                        // Copy remaining segment to buffer start
                        Buffer.MemoryCopy(
                            pData + contig,
                            _pBuffer,
                            _capacity,
                            toCopy - contig
                        );
                    }
                }

                _count += toCopy;
                return 1;
            }
        }

        /// <summary>
        /// Retrieves data from the queue without removing it
        /// </summary>
        /// <param name="offset">Starting offset from current head position</param>
        /// <param name="length">Number of bytes to retrieve</param>
        /// <returns>Byte array containing requested data</returns>
        public unsafe byte[] Peek(int offset, int length)
        {
            lock (_syncLock)
            {
                if (offset < 0 || offset >= _count)
                    return null;

                if (length < 0)
                    return null;

                if (offset + length > _count)
                    return null;

                byte[] result = new byte[length];
                int sourceIndex = (_head + offset) % _capacity;
                int contig = Math.Min(length, _capacity - sourceIndex);

                fixed (byte* pResult = result)
                {
                    if (contig == length)
                    {
                        Buffer.MemoryCopy(
                            _pBuffer + sourceIndex,
                            pResult,
                            length,
                            length
                        );
                    }
                    else
                    {
                        // Copy contiguous segment
                        Buffer.MemoryCopy(
                            _pBuffer + sourceIndex,
                            pResult,
                            contig,
                            contig
                        );

                        // Copy wrapped segment
                        Buffer.MemoryCopy(
                            _pBuffer,
                            pResult + contig,
                            length - contig,
                            length - contig
                        );
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Advances the queue head position by the specified length
        /// </summary>
        /// <param name="length">Number of bytes to advance</param>
        /// <returns>Actual number of bytes advanced</returns>
        public int Seek(int length)
        {
            lock (_syncLock)
            {
                if (length < 0)
                    return -1;

                if (length > _count)
                    return -1;

                _head = (_head + length) % _capacity;
                _count -= length;
                return length;
            }
        }

        /// <summary>
        /// Retrieves and removes data from the front of the queue
        /// </summary>
        /// <param name="length">Number of bytes to read</param>
        /// <returns>Byte array containing read data</returns>
        public unsafe byte[] Read(int length)
        {
            lock (_syncLock)
            {
                if (length < 0)
                    return null;

                if (length > _count)
                    return null;

                byte[] result = new byte[length];
                int contig = Math.Min(length, _capacity - _head);

                fixed (byte* pResult = result)
                {
                    if (contig == length)
                    {
                        Buffer.MemoryCopy(
                            _pBuffer + _head,
                            pResult,
                            length,
                            length
                        );
                    }
                    else
                    {
                        // Copy contiguous segment
                        Buffer.MemoryCopy(
                            _pBuffer + _head,
                            pResult,
                            contig,
                            contig
                        );

                        // Copy wrapped segment
                        Buffer.MemoryCopy(
                            _pBuffer,
                            pResult + contig,
                            length - contig,
                            length - contig
                        );
                    }
                }

                // Update queue state
                _head = (_head + length) % _capacity;
                _count -= length;

                return result;
            }
        }

        /// <summary>
        /// Searches for the first occurrence of a specific byte using pointer arithmetic
        /// </summary>
        /// <param name="pattern">Byte value to search for</param>
        /// <returns>
        /// Zero-based position of the byte relative to current head, 
        /// or -1 if not found
        /// </returns>
        public unsafe int Search(byte pattern)
        {
            lock (_syncLock)
            {
                if (_count == 0)
                    return -1;

                int segment1Length = Math.Min(_count, _capacity - _head);
                byte* pCurrent = _pBuffer + _head;
                byte* pEnd1 = pCurrent + segment1Length;

                // Search first contiguous segment
                while (pCurrent < pEnd1)
                {
                    if (*pCurrent == pattern)
                        return (int)(pCurrent - (_pBuffer + _head));
                    pCurrent++;
                }

                // Search second segment if needed
                int segment2Length = _count - segment1Length;
                if (segment2Length > 0)
                {
                    pCurrent = _pBuffer;
                    byte* pEnd2 = _pBuffer + segment2Length;
                    while (pCurrent < pEnd2)
                    {
                        if (*pCurrent == pattern)
                            return segment1Length + (int)(pCurrent - _pBuffer);
                        pCurrent++;
                    }
                }

                return -1;
            }
        }

        /// <summary>
        /// Clears all data from the queue
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
        /// Advances the queue to the first occurrence of the specified header byte
        /// </summary>
        /// <param name="header">Header byte to search for</param>
        /// <returns>
        /// true if header was found and queue advanced, 
        /// false if header not found (queue is cleared)
        /// </returns>
        public bool ToHeader(byte header)
        {
            lock (_syncLock)
            {
                int pos = SearchInternal(header);
                if (pos == -1)
                {
                    ClearInternal();
                    return false;
                }

                SeekInternal(pos);
                return true;
            }
        }

        #region Unsafe Internal Helpers
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe int SearchInternal(byte pattern)
        {
            if (_count == 0)
                return -1;

            int segment1Length = Math.Min(_count, _capacity - _head);
            byte* pCurrent = _pBuffer + _head;
            byte* pEnd1 = pCurrent + segment1Length;

            while (pCurrent < pEnd1)
            {
                if (*pCurrent == pattern)
                    return (int)(pCurrent - (_pBuffer + _head));
                pCurrent++;
            }

            int segment2Length = _count - segment1Length;
            if (segment2Length > 0)
            {
                pCurrent = _pBuffer;
                byte* pEnd2 = _pBuffer + segment2Length;
                while (pCurrent < pEnd2)
                {
                    if (*pCurrent == pattern)
                        return segment1Length + (int)(pCurrent - _pBuffer);
                    pCurrent++;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SeekInternal(int length)
        {
            _head = (_head + length) % _capacity;
            _count -= length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearInternal()
        {
            _head = 0;
            _count = 0;
        }
        #endregion
    }
}
