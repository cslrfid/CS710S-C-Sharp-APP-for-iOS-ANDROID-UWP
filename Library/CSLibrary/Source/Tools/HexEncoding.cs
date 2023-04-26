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
using System.Text;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CSLibrary.Tools
{
    /// <summary>
    /// For Backward Compatibility
    /// </summary>
    public class Hex : HexEncoding
    {
    }

    /// <summary>
    /// Summary description for HexEncoding.
    /// </summary>
    public class HexEncoding
    {

        public static bool IsHexDigit(char c)
        {
            return ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
        }

        public static long HostToNetworkOrder(long host)
        {
#if BIGENDIAN
            return host; 
#else
            return (((long)HostToNetworkOrder((int)host) & 0xFFFFFFFF) << 32)
                    | ((long)HostToNetworkOrder((int)(host >> 32)) & 0xFFFFFFFF);
#endif
        }
        public static int HostToNetworkOrder(int host)
        {
#if BIGENDIAN
            return host;
#else 
            return (((int)HostToNetworkOrder((short)host) & 0xFFFF) << 16)
                    | ((int)HostToNetworkOrder((short)(host >> 16)) & 0xFFFF);
#endif
        }
        public static short HostToNetworkOrder(short host)
        {
#if BIGENDIAN
            return host;
#else
            return (short)((((int)host & 0xFF) << 8) | (int)((host >> 8) & 0xFF));
#endif
        }
        public static long NetworkToHostOrder(long network)
        {
            return HostToNetworkOrder(network);
        }
        public static int NetworkToHostOrder(int network)
        {
            return HostToNetworkOrder(network);
        }
        public static short NetworkToHostOrder(short network)
        {
            return HostToNetworkOrder(network);
        }


        /// <summary>
        /// Creates a byte array from the hexadecimal string. Each two characters are combined
        /// to create one byte. First two hexadecimal characters become first byte in returned array.
        /// Non-hexadecimal characters are ignored. 
        /// </summary>
        /// <param name="hexString">string to convert to byte array</param>
        /// <returns>byte array, in the same left-to-right order as the hexString</returns>
        public static byte[] ToBytes(string hexString)//, out int discarded)
        {
            //discarded = 0;
            if (hexString == null || hexString.Length == 0)
                return new byte[0];
            string newString = "";
            char c;
            // remove all none A-F, 0-9, characters
            for (int i = 0; i < hexString.Length; i++)
            {
                c = hexString[i];
                if (IsHexDigit(c))
                    newString += c;
            }

            int byteLength = newString.Length / 2 + newString.Length % 2;
            byte[] bytes = new byte[byteLength];
            string hex = "";
            for (int i = 0; i < byteLength; i++)
            {
                if (i * 2 + 1 < newString.Length)
                    hex = new String(new Char[] { newString[i * 2], newString[i * 2 + 1] });
                else
                    hex = new String(new Char[] { newString[i * 2], '0' });
                bytes[i] = HexToByte(hex);
            }
            return bytes;
        }

        /// <summary>
        /// Creates a byte array from the hexadecimal string. Each two characters are combined
        /// to create one byte. First two hexadecimal characters become first byte in returned array.
        /// Non-hexadecimal characters are ignored. 
        /// </summary>
        /// <param name="hexString">string to convert to byte array</param>
        /// <returns>byte array, in the same left-to-right order as the hexString</returns>
        public static byte[] GenSelectMask(string hexString)
        {
            int max_mask_size = 32;
            //discarded = 0;
            if (hexString == null || hexString.Length == 0)
                return new byte[max_mask_size];
            string newString = "";
            char c;
            // remove all none A-F, 0-9, characters
            for (int i = 0; i < hexString.Length; i++)
            {
                c = hexString[i];
                if (IsHexDigit(c))
                    newString += c;
            }

            int byteLength = newString.Length / 2 + newString.Length % 2;
            byte[] bytes = new byte[max_mask_size];
            string hex = "";
            for (int i = 0; i < byteLength; i++)
            {
                if (i * 2 + 1 < newString.Length)
                    hex = new String(new Char[] { newString[i * 2], newString[i * 2 + 1] });
                else
                    hex = new String(new Char[] { newString[i * 2], '0' });
                bytes[i] = HexToByte(hex);
            }
            for (int i = byteLength; i < max_mask_size; i++)
            {
                bytes[i] = 0x00;
            }
            return bytes;
        }
        /// <summary>
        /// Creates a byte array from the hexadecimal string. Each two characters are combined
        /// to create one byte. First two hexadecimal characters become first byte in returned array.
        /// Non-hexadecimal characters are ignored. 
        /// </summary>
        /// <param name="hexString">string to convert to byte array</param>
        /// <returns>byte array, in the same left-to-right order as the hexString</returns>
        public static byte[] GenPostMatchMask(string hexString)
        {
            int max_mask_size = 62;
            //discarded = 0;
            if (hexString == null || hexString.Length == 0)
                return new byte[max_mask_size];
            string newString = "";
            char c;
            // remove all none A-F, 0-9, characters
            for (int i = 0; i < hexString.Length; i++)
            {
                c = hexString[i];
                if (IsHexDigit(c))
                    newString += c;
            }

            int byteLength = newString.Length / 2 + newString.Length % 2;
            byte[] bytes = new byte[max_mask_size];
            string hex = "";
            for (int i = 0; i < byteLength; i++)
            {
                if (i * 2 + 1 < newString.Length)
                    hex = new String(new Char[] { newString[i * 2], newString[i * 2 + 1] });
                else
                    hex = new String(new Char[] { newString[i * 2], '0' });
                bytes[i] = HexToByte(hex);
            }
            for (int i = byteLength; i < max_mask_size; i++)
            {
                bytes[i] = 0x00;
            }
            return bytes;
        }
        /// <summary>
        /// Convent ushort array to byte array
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] ToBytes(ushort[] data)
        {
            return ToBytes(data, data.Length);
        }
        /// <summary>
        /// Convent ushort array to byte array
        /// </summary>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte[] ToBytes(ushort[] data, int count)
        {
            if (data == null || data.Length == 0 || count == 0)
                return new byte[0];
            int bytes = count << 1;

            byte[] reda = new byte[bytes];
            for (int i = 0; i < count; i++)
            {
                reda[i * 2] = (Byte)((data[i] >> 8) & 0xff);
                reda[i * 2 + 1] = (Byte)(data[i] & 0xff);
            }
            //byte[] reda = new byte[bytes];
            //Buffer.BlockCopy(data, 0, reda, 0, bytes);
            return reda;
        }
        /// <summary>
        /// return a byte from string
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte ToByte(string hexString)//, out int discarded)
        {
            //discarded = 0;
            if (hexString == null || hexString.Length > 2 || hexString.Length == 0)
                return new byte();
            string newString = "";
            char c;
            // remove all none A-F, 0-9, characters
            for (int i = 0; i < hexString.Length; i++)
            {
                c = hexString[i];
                if (IsHexDigit(c))
                    newString += c;
            }

            string hex = "";
            if (newString.Length == 2)
                hex = new String(new Char[] { newString[0], newString[1] });
            else
                hex = new String(new Char[] { '0', newString[0] });
            return HexToByte(hex);
        }
        /// <summary>
        /// return ushort from byte string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static ushort ToUshort(string str)
        {
            if (str == null || str.Length == 0)
                return 0x0;//throw new Reader.Exception.ReaderException("HexEncoding.GetUshort input null");
            return ushort.Parse(str, System.Globalization.NumberStyles.HexNumber);
        }
        /// <summary>
        /// return ushort array from byte array
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static ushort[] ToUshorts(byte[] Input)
        {
            if (Input == null)
                return null;//throw new Reader.Exception.ReaderException("HexEncoding.GetUshort input null");
            ushort[] Output = new ushort[Input.Length / 2 + Input.Length % 2];
            int j = 0;
            for (int i = 0; i < Output.Length; i++)
            {
                if (j + 1 < Input.Length)
                    Output[i] = (ushort)(Input[j] << 8 | Input[j + 1]);
                else
                    Output[i] = (ushort)(Input[j] << 8 | 0x0);
                j += 2;
            }
            return Output;
        }
        
		/// <summary>
        /// return short array from byte array
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static short[] ToShorts(byte[] Input)
        {
            if (Input == null)
                return null;//throw new Reader.Exception.ReaderException("HexEncoding.GetUshort input null");
            short[] Output = new short[Input.Length / 2 + Input.Length % 2];
            int j = 0;
            for (int i = 0; i < Output.Length; i++)
            {
                if (j + 1 < Input.Length)
                    Output[i] = (short)(Input[j] << 8 | Input[j + 1]);
                else
                    Output[i] = (short)(Input[j] << 8 | 0x0);
                j += 2;
            }
            return Output;
        }
        
		/// <summary>
        /// return ushort array from string input
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static ushort[] ToUshorts(string Input)
        {
            string newString = "";
            char c;
            // remove all none A-F, 0-9, characters
            for (int i = 0; i < Input.Length; i++)
            {
                c = Input[i];
                if (IsHexDigit(c))
                    newString += c;
            }
            int fullUshortLength = newString.Length / 4;
            int leftUshortLength = newString.Length % 4;
            ushort[] ushorts = new ushort[fullUshortLength + (leftUshortLength > 0 ? 1 : 0)];
            string hex = "";
            int j = 0;
            for (int i = 0; i < ushorts.Length; i++)
            {
                if (i < fullUshortLength)
                {
                    hex = new String(new Char[] { newString[j], newString[j + 1], newString[j + 2], newString[j + 3] });
                }
                else
                {
                    switch (leftUshortLength)
                    {
                        case 1:
                            hex = new String(new Char[] { newString[j], '0', '0', '0' });
                            break;
                        case 2: hex = new String(new Char[] { newString[j], newString[j + 1], '0', '0' });
                            break;
                        case 3: hex = new String(new Char[] { newString[j], newString[j + 1], newString[j + 2], '0' });
                            break;
                        default: break;
                    }

                }
                ushorts[i] = HexToUshort(hex);
                j = j + 4;
            }
            return ushorts;
        }

        /// <summary>
        /// return UInt32 array from string input
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static UInt32[] ToUInt32s(string Input)
        {
            string newString = "";
            char c;
            // remove all none A-F, 0-9, characters
            for (int i = 0; i < Input.Length; i++)
            {
                c = Input[i];
                if (IsHexDigit(c))
                    newString += c;
            }
            int fullUshortLength = newString.Length / 8;
            int leftUshortLength = newString.Length % 8;
            UInt32 [] UInt32s = new UInt32[fullUshortLength + (leftUshortLength > 0 ? 1 : 0)];
            string hex = "";
            int j = 0;
            for (int i = 0; i < UInt32s.Length; i++)
            {
                if (i < fullUshortLength)
                {
                    hex = new String(new Char[] { newString[j], newString[j + 1], newString[j + 2], newString[j + 3], newString[j + 4], newString[j + 5], newString[j + 6], newString[j + 7] });
                }
                else
                {
                    hex = "";
                    for (int cnt = 0; cnt < leftUshortLength; cnt++)
                        hex += newString[j + cnt];

                    for (int cnt = 0; cnt < 8 - leftUshortLength; cnt++)
                        hex += '0';
                }
                UInt32s[i] = HexToUInt32(hex);
                j = j + 8;
            }
            return UInt32s;
        }

        /// <summary>
        /// Byte to String Conversion
        /// </summary>
        /// <param name="bytes">Input Byte Array</param>
        /// <param name="offset">Start offset</param>
        /// <param name="count">Number of Count to converse</param>
        /// <returns>Return a String</returns>
        public static string ToString(byte[] bytes, int offset = 0, int count = -1)
        {
			if (bytes == null || bytes.Length == 0)
				return "";

			if (count == -1)
				count = bytes.Length;

			string hexString = "";
            for (int i = offset, j = 0; (i < bytes.Length && j < count); i++, j++)
            {
                hexString += bytes[i].ToString("X2");
            }
            return hexString;
        }

		/// <summary>
		/// ushort to String Conversion
		/// </summary>
		/// <param name="data">source data</param>
		/// <param name="offset">Start offset</param>
		/// <param name="count">Number of Count to converse</param>
		/// <returns></returns>
		public static string ToString(ushort[] data, int offset = 0, int count = -1)
        {
			if (data == null || count == 0)
				return "";

			if (count == -1)
				count = data.Length;

			string hexString = "";
            for (int i = offset, j = 0; (i < data.Length && j < count); i++, j++)
            {
                hexString += data[i].ToString("X4");
            }

			return hexString;
        }
        /// <summary>
        /// Determines if given string is in proper hexadecimal string format
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static bool IsHexFormat(string hexString)
        {
            bool hexFormat = true;

            foreach (char digit in hexString)
            {
                if (!IsHexDigit(digit))
                {
                    hexFormat = false;
                    break;
                }
            }
            return hexFormat;
        }
        /*
        /// <summary>
        /// Returns true if c is a hexadecimal digit (A-F, a-f, 0-9)
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns>true if hex digit, false if not</returns>
        public static bool Uri.IsHexDigit(Char c)
        {
            int numChar;
            int numA = 65;// Convert.ToInt32('A');
            int num1 = 48;// Convert.ToInt32('0');
            c = Char.ToUpper(c);
            numChar = Convert.ToInt32(c);
            if (numChar >= numA && numChar < (numA + 6))
                return true;
            if (numChar >= num1 && numChar < (num1 + 10))
                return true;
            return false;
        }*/
        /// <summary>
        /// Converts 1 or 2 character string into equivalant byte value
        /// </summary>
        /// <param name="hex">1 or 2 character string</param>
        /// <returns>byte</returns>
        private static byte HexToByte(string hex)
        {
            if (hex.Length > 2 || hex.Length <= 0)
                throw new ArgumentException("hex must be 1 or 2 characters in length");
            byte newByte = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return newByte;
        }
        private static ushort HexToUshort(string hex)
        {
            if (hex.Length > 4 || hex.Length <= 0)
                throw new ArgumentException("hex must be 1 ~ 4 characters in length");
            ushort newByte = ushort.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return newByte;
        }
        private static UInt32 HexToUInt32(string hex)
        {
            if (hex.Length > 8 || hex.Length <= 0)
                throw new ArgumentException("hex must be 1 ~ 8 characters in length");
            UInt32 newByte = UInt32.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return newByte;
        }
        /// <summary>
        /// Compare Hex String, ie, source = "11ffaa", target = "11FFaa", it will return true
        /// </summary>
        /// <param name="source">Source Hex string</param>
        /// <param name="target">Target comparing Hex string</param>
        /// <returns></returns>
        public static bool Compare(string source, string target)
        {
            return ToBytes(source).Equals(ToBytes(target));
        }
        /// <summary>
        /// Compare Hex
        /// </summary>
        /// <param name="source">Source Hex</param>
        /// <param name="target">Target comparing Hex</param>
        /// <returns></returns>
        public static bool Compare(Byte[] source, Byte[] target)
        {
            return source.Equals(target);
        }
        /// <summary>
        /// Compare Hex
        /// </summary>
        /// <param name="source">Source Hex</param>
        /// <param name="target">Target comparing Hex</param>
        /// <param name="size">Size to compare</param>
        /// <returns></returns>
        public static bool Compare(Byte[] source, Byte[] target, int size)
        {
            if (size == 0)
                return true;

            if (source == null || target == null || size > Math.Min(source.Length, target.Length))
                return false;

            for (int cnt = 0; cnt < size; cnt++)
                if (source[cnt] != target[cnt])
                    return false;

            return true;
            
            
            /*
            if (source == null && target == null)
                return true;
            if (source == null)
                return false;
            if (target == null)
                return false;
            if (size > Math.Min(source.Length, target.Length) || target.Length != source.Length)
                return false;
            if (Win32.memcmp(source, target, source.Length) != 0)
            {
                return false;
            }
            return true;
*/
        }

        /// <summary>
        /// Convert 2 Bytes to one short
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Int16 ToInt16(byte[] buffer, int offset)
        {
            if (buffer == null || buffer.Length == 0)
                return 0;
            Int16 result = NetworkToHostOrder(BitConverter.ToInt16(buffer, offset));
            //offset += 2;
            return result;
        }

        /// <summary>
        /// Convert 2 Bytes to one short
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static UInt16 ToUInt16(byte[] buffer, int offset)
        {
            if (buffer == null || buffer.Length == 0)
                return 0;
            return (UInt16)ToInt16(buffer, offset);
        }

        /*
        /// <summary>
        /// Convert 4 Bytes to one uint32
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static UInt32 ToUInt32(byte[] buffer, int offset)
        {
            if (buffer == null || buffer.Length == 0)
                return 0;
            Int32 result = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, offset));
            //offset += 4;
            return (UInt32)result;
        }
        */

        /// <summary>
        /// Convert string to one uint16
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static UInt16 ToUInt16(string hexString)
        {
            if (hexString == null || hexString.Length == 0)
                return 0;
            return UInt16.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
        }

        /// <summary>
        /// Convert string to one uint32
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static UInt32 ToUInt32(string hexString)
        {
            if (hexString == null || hexString.Length == 0)
                return 0;
            return UInt32.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
        }

        public static UInt16 LSBToUInt16(byte[] data, int index)
        {
            UInt16 value = (UInt16)(data[index] | data[index + 1] << 8);
            return value;
        }

        public static UInt16 MSBToUInt16(byte[] data, int index)
        {
            UInt16 value = (UInt16)(data[index] << 8 | data[index + 1]);
            return value;
        }

        public static UInt32 LSBToUInt32(byte[] data, int index)
        {
            uint value = (uint)(data[index] | data[index + 1] << 8 | data[index + 2] << 16 | data[index + 3] << 24);
            return value;
        }

        public static UInt32 MSBToUInt32(byte[] data, int index)
        {
            uint value = (uint)(data[index] << 24 | data[index + 1] << 16 | data[index + 2] << 8 | data[index + 3]);
            return value;
        }

        public static UInt64 LSBToUInt64(byte[] data, int index)
        {
            UInt64 value = (uint)(data[index] | data[index + 1] << 8 | data[index + 2] << 16 | data[index + 3] << 24 | data[index + 4] << 32 | data[index + 5] << 40 | data[index + 6] << 48 | data[index + 7] << 56);
            return value;
        }

        public static UInt64 MSBToUInt64(byte[] data, int index)
        {
            UInt64 value = (uint)(data[index] << 56 | data[index + 1] << 48 | data[index + 2] << 40 | data[index + 3] << 32 | data[index + 4] << 24 | data[index + 5] << 16 | data[index + 6] << 8 | data[index + 7]);
            return value;
        }

        public static UInt16 [] MSBToUInt16Array (byte[] data, int index, int size)
        {
            UInt16[] values = new UInt16[size];

            for (int cnt = 0; cnt < size; cnt++)
            {
                values[cnt] = MSBToUInt16(data, index);
                index += 2;
            }

            return values;
        }


        /*
        /// <summary>
        /// Convert 4 bytes to long
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static UInt64 ToUInt64(byte[] buffer, int offset)
        {
            if (buffer == null || buffer.Length == 0)
                return 0;
            Int64 result = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer, offset));
            //offset += 8;
            return (UInt64)result;
        }
        */
        /// <summary>
        /// Copy One array to another array
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="count"></param>
        public static void Copy
                    (
                    Byte[] src,
                    Byte[] dst,
                    int count
                    )
        {
            Copy(src, 0, dst, 0, count);
        }
        /// <summary>
        /// Copy One array to another array
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="dstIndex"></param>
        /// <param name="count"></param>
        public static void Copy
                    (
                    Byte[] src,
                    Byte[] dst,
                    int dstIndex,
                    int count
                    )
        {
            Copy(src, 0, dst, dstIndex, count);
        }
        /// <summary>
        /// Copy One array to another array
        /// </summary>
        /// <param name="src"></param>
        /// <param name="srcIndex"></param>
        /// <param name="dst"></param>
        /// <param name="dstIndex"></param>
        /// <param name="count"></param>
        public static void Copy
            (
            Byte[] src,
            int srcIndex,
            Byte[] dst,
            int dstIndex,
            int count
            )
        {

            Copy(src, srcIndex, dst, dstIndex, count);
            
#if nouse
            const int block = 32;
            if (src == null || srcIndex < 0 ||
                dst == null || dstIndex < 0 || count < 0)
            {
                throw new ArgumentException();
            }
            int srcLen = src.Length;
            int dstLen = dst.Length;
            if (srcLen - srcIndex < count ||
                dstLen - dstIndex < count)
            {
                throw new ArgumentException();
            }


            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection.          
            fixed (byte* pSrc = src, pDst = dst)
            {
                byte* ps = pSrc;
                byte* pd = pDst;

                // Loop over the count in blocks of 4 bytes, copying an
                // integer (4 bytes) at a time:
                for (int n = 0; n < count / block; n++)
                {
                    *((int*)pd) = *((int*)ps);
                    pd += block;
                    ps += block;
                }

                // Complete the copy by moving any bytes that weren't
                // moved in blocks of 4:
                for (int n = 0; n < count % block; n++)
                {
                    *pd = *ps;
                    pd++;
                    ps++;
                }
            }
#endif
        }

        /// <summary>
        /// Generic compare two array
        /// </summary>
        /// <typeparam name="T">Type of array</typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Compare<T>(T[] a, T[] b)
        {
            if (a.Length != b.Length)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            for (int i = 0; i < a.Length; i++)
            {
                if (!comparer.Equals(a[i], b[i]))
                    return false;
            }
            return true;

        }
        /// <summary>
        /// Get bit length from string
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static UInt32 GetBitCount(string hexString)
        {
            if (hexString == null || hexString.Length == 0)
            {
                return 0;
            }

            string newString = String.Empty;
            char c;
            // remove all none A-F, 0-9, characters
            for (int i = 0; i < hexString.Length; i++)
            {
                c = hexString[i];
                if (IsHexDigit(c))
                    newString += c;
            }

            return (uint)(newString.Length * 4);
        }

        /// <summary>
        /// Get bit length from string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static UInt32 GetBitCount(Byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return 0;
            }
            return (uint)(bytes.Length * 8);
        }

        /// <summary>
        /// Get Word length from string
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static ushort GetWordCount(string hexString)
        {
            if (hexString == null || hexString.Length == 0)
            {
                return 0;
            }
            string newString = String.Empty;
            char c;
            // remove all none A-F, 0-9, characters
            for (int i = 0; i < hexString.Length; i++)
            {
                c = hexString[i];
                if (IsHexDigit(c))
                    newString += c;
            }
            return (ushort)ToUshorts(newString).Length;
        }
        /// <summary>
        /// GetByteCount
        /// </summary>
        /// <param name="hexString">Input Hex String</param>
        /// <returns>Number of Byte Count</returns>
        public static int GetByteCount(string hexString)
        {
            int numHexChars = 0;
            char c;
            // remove all none A-F, 0-9, characters
            for (int i = 0; i < hexString.Length; i++)
            {
                c = hexString[i];
                if (IsHexDigit(c))
                    numHexChars++;
            }
            // if odd number of characters, discard last character
            if (numHexChars % 2 != 0)
            {
                numHexChars--;
            }
            return numHexChars / 2; // 2 characters per byte
        }
        /// <summary>
        /// Convert to binary format
        /// </summary>
        /// <param name="source"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string ToBinary(byte[] source, uint offset, uint length)
        {
            BitArray bit = new BitArray(source);
            string bitString = bit.ToString();
            bitString = bitString.Remove(0, (int)offset);
            if (bitString.Length > length)
            {
                bitString = bitString.Remove((int)length, (int)(bitString.Length - length));
            }
            return bitString;
        }
    }
}
