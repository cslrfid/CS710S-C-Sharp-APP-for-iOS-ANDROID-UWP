/*
Copyright (c) 2018 Convergence Systems Limited

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSLibrary.Tools
{
    public class Queue
    {
        private int MAXBUFFERSIZE;
        private byte[] _dataStream;
        object _dataStreamLock = new object();
        private int _dataStreamStartPoint = 0;
        private int _dataStreamSize = 0;

        public Queue(int size = 1024)
        {
            MAXBUFFERSIZE = size;
            _dataStream = new byte[MAXBUFFERSIZE];
        }

        ~Queue()
        {
        }

        public int length
        {
            get { return _dataStreamSize; }
        }

        public void Clear()
        {
            lock (_dataStreamLock)
            {
                _dataStreamStartPoint = 0;
                _dataStreamSize = 0;
            }
        }

        /// <summary>
        /// all data will be clear
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool ReSize(int size)
        {
            lock (_dataStreamLock)
            {
                try
                {
                    _dataStream = new byte[size];
                    _dataStreamStartPoint = 0;
                    _dataStreamSize = 0;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            return true;
        }

        public bool DataIn(byte[] data, int offset = 0, int size = -1)
        {
			if (size < 0)
				size = data.Length - offset;

			if (size == 0)
				return true;

			if (_dataStreamSize + size > MAXBUFFERSIZE)
				return false;

			lock (_dataStreamLock)
			{
				if (_dataStreamStartPoint + _dataStreamSize + size < MAXBUFFERSIZE)
				{
					Array.Copy(data, offset, _dataStream, _dataStreamStartPoint + _dataStreamSize, size);
				}
				else if ((_dataStreamStartPoint + _dataStreamSize) >= MAXBUFFERSIZE)
                {
                    Array.Copy(data, offset, _dataStream, ((_dataStreamStartPoint + _dataStreamSize) - MAXBUFFERSIZE), size);
                }
                else
                {
					int headerLength = MAXBUFFERSIZE - _dataStreamStartPoint - _dataStreamSize;
					int footerlength = size - headerLength;

					Array.Copy(data, 0, _dataStream, _dataStreamStartPoint + _dataStreamSize, headerLength);
					Array.Copy(data, headerLength, _dataStream, 0, footerlength);
				}

				_dataStreamSize += size;
			}

			return true;
        }

#if !oldcode
		public bool DataDel(int dataDelectLength)
		{
			lock (_dataStreamLock)
			{
				if (_dataStreamSize == 0)
					return true;

				if (dataDelectLength > _dataStreamSize)
					return false;

				_dataStreamSize -= dataDelectLength;
				if (_dataStreamSize == 0)
				{
					_dataStreamStartPoint = 0;
				}
				else if (_dataStreamStartPoint + dataDelectLength < MAXBUFFERSIZE)
				{
					_dataStreamStartPoint += dataDelectLength;
				}
				else
				{
					_dataStreamStartPoint = dataDelectLength - (MAXBUFFERSIZE - _dataStreamStartPoint);
				}

				return true;
			}
		}
#else
		public bool DataDel(int dataDelectLength)
        {
			lock (_dataStreamLock)
			{
				if (_dataStreamSize == 0)
                    return true;

				if (dataDelectLength > _dataStreamSize)
					return false;

				if (_dataStreamStartPoint + dataDelectLength < MAXBUFFERSIZE)
				{
					_dataStreamStartPoint += dataDelectLength;
				}
				else
				{
					int headerLength = MAXBUFFERSIZE - _dataStreamStartPoint;
					int footerlength = dataDelectLength - headerLength;

					_dataStreamStartPoint = footerlength;
				}

				_dataStreamSize -= dataDelectLength;
				if (_dataStreamSize == 0)
					_dataStreamStartPoint = 0;

				return true;
			}
		}
#endif

		public byte[] DataPreOut(int outDataLength = 1)
		{
			lock (_dataStreamLock)
			{
				byte[] outData;

				if (_dataStreamSize == 0)
					return new byte[0];

				if (outDataLength < _dataStreamSize)
				{
					outData = new byte[outDataLength];
				}
				else
				{
					outData = new byte[_dataStreamSize];
				}

				if (_dataStreamStartPoint + outData.Length < MAXBUFFERSIZE)
				{
					Array.Copy(_dataStream, _dataStreamStartPoint, outData, 0, outData.Length);
				}
				else
				{
					int headerLength = MAXBUFFERSIZE - _dataStreamStartPoint;
					int footerlength = outData.Length - headerLength;

					Array.Copy(_dataStream, _dataStreamStartPoint, outData, 0, headerLength);
					Array.Copy(_dataStream, 0, outData, headerLength, footerlength);
				}

				return outData;
			}
		}

#if !oldcode
		public byte[] DataOut(int outDataLength = 1)
		{
			byte[] outData = DataPreOut (outDataLength);

			if (outData.Length != 0)
				DataDel(outData.Length);

			return outData;
		}
#else
		public byte[] DataOut(int outDataLength)
        {
            lock (_dataStreamLock)
            {
                byte[] outData;

                if (_dataStreamSize == 0)
                    return new byte[0];

                if (outDataLength < _dataStreamSize)
                {
                    outData = new byte[outDataLength];
                }
                else
                {
                    outData = new byte[_dataStreamSize];
                }

                if (_dataStreamStartPoint + outData.Length < MAXBUFFERSIZE)
                {
                    Array.Copy(_dataStream, _dataStreamStartPoint, outData, 0, outData.Length);

                    _dataStreamStartPoint += outData.Length;
                }
                else
                {
                    int headerLength = MAXBUFFERSIZE - _dataStreamStartPoint;
                    int footerlength = outData.Length - headerLength;

                    Array.Copy(_dataStream, _dataStreamStartPoint, outData, 0, headerLength);
                    Array.Copy(_dataStream, 0, outData, headerLength, footerlength);

                    _dataStreamStartPoint = footerlength;
                }

                _dataStreamSize -= outData.Length;
                if (_dataStreamSize == 0)
                    _dataStreamStartPoint = 0;

                return outData;
            }
        }
#endif

        /*
		private void Defragment()
        {
            if (_dataStreamStartPoint != 0 && _dataStreamSize != 0)
            {
                byte[] newStream = new byte[_dataStream.Length];
                int headerLength = MAXBUFFERSIZE - _dataStreamStartPoint;
                int footerlength = _dataStreamSize - headerLength;

                Array.Copy(_dataStream, _dataStreamStartPoint, newStream, 0, headerLength);
                Array.Copy(_dataStream, 0, newStream, headerLength, footerlength);

                _dataStream = newStream;
                _dataStreamStartPoint = 0;
            }
        }
        */

        private void Defragment()
        {
            try
            {
                if (_dataStreamStartPoint != 0 && _dataStreamSize != 0)
                {
                    if ((_dataStreamStartPoint + _dataStreamSize) > _dataStream.Length)
                    {
                        byte[] newStream = new byte[_dataStream.Length];
                        int headerLength = MAXBUFFERSIZE - _dataStreamStartPoint;
                        int footerlength = _dataStreamSize - headerLength;

                        Array.Copy(_dataStream, _dataStreamStartPoint, newStream, 0, headerLength);
                        Array.Copy(_dataStream, 0, newStream, headerLength, footerlength);

                        _dataStream = newStream;
                        _dataStreamStartPoint = 0;
                    }
                    else
                    {
                        byte[] newStream = new byte[_dataStream.Length];

                        Array.Copy(_dataStream, _dataStreamStartPoint, newStream, 0, _dataStreamSize);

                        _dataStream = newStream;
                        _dataStreamStartPoint = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("Defragment Error : " + ex.Message);
            }
        }


        /// <summary>
        /// find data in array
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int IndexOf (byte [] data)
        {
            if (data.Length > _dataStreamSize)
                return -1;

            lock (_dataStreamLock)
            {
                if (_dataStreamStartPoint + _dataStreamSize >= MAXBUFFERSIZE)
                    Defragment();

                return Array.IndexOf(_dataStream, data, _dataStreamStartPoint, _dataStreamSize);
            }
        }

        /// <summary>
        /// find data in array
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int IndexOf(byte data)
        {
            lock (_dataStreamLock)
            {
                if (_dataStreamSize == 0)
                return -1;

                Defragment();
                return Array.IndexOf(_dataStream, data, 0, _dataStreamSize);
            }
        }

        /// <summary>
        /// Special function for Inventory
        /// </summary>
        /// <returns></returns>
        public int IndexOfValidInventoryResponsePacket()
        {
            return -1;
        }

        /// <summary>
        /// Special function for Inventory
        /// </summary>
        /// <returns></returns>
        public int IndexOfTagResponsePacket(int offSet = 0)
        {
            const int PKTVER = 0x03;
            const int PKTTYP1 = 0x05;
            const int PKTTYP2 = 0x80;
            const int PKTREV1 = 0x00;
            const int PKTREV2 = 0x00;

            int headerIndex;

            lock (_dataStreamLock)
            {
                if (_dataStreamSize < 8)
                return -1;

                Defragment();
                headerIndex = Array.IndexOf(_dataStream, PKTVER, offSet, _dataStreamSize);

                if (headerIndex >= 0)
                {
                    if (_dataStream[headerIndex + 2] == PKTTYP1 &&
                        _dataStream[headerIndex + 3] == PKTTYP2 &&
                        _dataStream[headerIndex + 6] == PKTREV1 &&
                        _dataStream[headerIndex + 7] == PKTREV2)
                        return headerIndex;
                }
            }

            return -1;
        }

        public void Skip(int skipSize)
        {
            lock (_dataStreamLock)
            {
                if (skipSize >= _dataStreamSize)
                {
                    Clear ();
                    return;
                }

                if (_dataStreamStartPoint + skipSize < MAXBUFFERSIZE)
                {
                    _dataStreamStartPoint += skipSize;
                }
                else
                {
                    _dataStreamStartPoint = skipSize - (MAXBUFFERSIZE - _dataStreamStartPoint);
                }

                _dataStreamSize -= skipSize;
                return;
            }
        }

        public int TrimRFIDPacket (int mode)
        {
            // delete data
            lock (_dataStreamLock)
            {
                int index;

                Defragment();

                // find new pkt_ver
                for (index = 1; index < _dataStreamSize; index++)
                {
                    if (mode == 0)
                    {
                        if (_dataStream[index] == 0x00 || _dataStream[index] == 0x40 || _dataStream[index] == 0x70)
                            break;
                    }
                    else
                    {
                        if ((_dataStream[index] >= 0x01 && _dataStream[index] <= 0x04) || _dataStream[index] == 0x40)
                            break;
                    }
                }

                if (index == _dataStreamSize)
                {
                    _dataStreamSize = 0;
                }
                else
                {
                    _dataStreamStartPoint = index;
                    _dataStreamSize -= index;
                }
            }

            return -1;
        }
    }
}
