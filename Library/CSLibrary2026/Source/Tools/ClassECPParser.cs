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
    public class EpcParser
    {
        /// <summary>
        /// Protocol Control (PC) structure - 16-bit word containing protocol control information
        /// </summary>
        public struct PC
        {
            /// <summary>
            /// Raw 16-bit PC value
            /// </summary>
            public ushort Value { get; set; }

            /// <summary>
            /// EPC Length field (bits 11-15): Number of 16-bit words in EPC
            /// </summary>
            public byte Length => (byte)((Value >> 11) & 0x1F);

            /// <summary>
            /// Reserved for User Memory (bit 10): Indicates if User Memory is available
            /// </summary>
            public bool RUM => (Value & 0x0400) != 0;

            /// <summary>
            /// XPC Indicator (bit 9): Indicates presence of XPC word
            /// </summary>
            public bool XI => (Value & 0x0200) != 0;

            /// <summary>
            /// Toggle bit (bit 8): Used for inventory rounds
            /// </summary>
            public bool T => (Value & 0x0100) != 0;

            /// <summary>
            /// Reserved for Future Use (bits 0-7)
            /// </summary>
            public byte RFU => (byte)(Value & 0xFF);

            /// <summary>
            /// Creates PC structure from raw 16-bit value
            /// </summary>
            public PC(ushort value)
            {
                Value = value;
            }
        }

        /// <summary>
        /// PC_W1 structure - First word (16 bits) following PC in EPC data
        /// Contains Extended EPC Bank (XEB) and various protocol control flags
        /// </summary>
        public struct PC_W1
        {
            /// <summary>
            /// Raw 16-bit value of W1
            /// </summary>
            public ushort Value { get; set; }

            /// <summary>
            /// Extended EPC Bank (bit 15): Indicates presence of extended EPC bank
            /// </summary>
            public bool XEB => (Value & 0x8000) != 0;

            /// <summary>
            /// Reserved for Future Use (bits 12-14)
            /// </summary>
            public byte RFU_14_12 => (byte)((Value >> 12) & 0x07);

            /// <summary>
            /// Stored Access (bit 11): Indicates if tag supports stored access passwords
            /// </summary>
            public bool SA => (Value & 0x0800) != 0;

            /// <summary>
            /// Stored Select (bit 10): Indicates if tag supports stored select masks
            /// </summary>
            public bool SS => (Value & 0x0400) != 0;

            /// <summary>
            /// File Structure (bit 9): Indicates if tag uses file structure
            /// </summary>
            public bool FS => (Value & 0x0200) != 0;

            /// <summary>
            /// Stored Number (bit 8): Indicates if tag stores numbering information
            /// </summary>
            public bool SN => (Value & 0x0100) != 0;

            /// <summary>
            /// BlockWrite (bit 7): Indicates if tag supports BlockWrite command
            /// </summary>
            public bool B => (Value & 0x0080) != 0;

            /// <summary>
            /// Cryptographic Suite (bit 6): Indicates cryptographic capabilities
            /// </summary>
            public bool C => (Value & 0x0040) != 0;

            /// <summary>
            /// Short Living (bit 5): Indicates if tag is short-lived
            /// </summary>
            public bool SLI => (Value & 0x0020) != 0;

            /// <summary>
            /// Tag Number (bit 4): Part of tag numbering scheme
            /// </summary>
            public bool TN => (Value & 0x0010) != 0;

            /// <summary>
            /// UHF (bit 3): Indicates UHF band operation
            /// </summary>
            public bool U => (Value & 0x0008) != 0;

            /// <summary>
            /// Kill (bit 2): Indicates if tag supports Kill command
            /// </summary>
            public bool K => (Value & 0x0004) != 0;

            /// <summary>
            /// No Recommendation (bit 1): Indicates no recommendation for this bit
            /// </summary>
            public bool NR => (Value & 0x0002) != 0;

            /// <summary>
            /// Handle (bit 0): Indicates if tag supports Handle
            /// </summary>
            public bool H => (Value & 0x0001) != 0;

            /// <summary>
            /// Creates PC_W1 structure from raw 16-bit value
            /// </summary>
            public PC_W1(ushort value)
            {
                Value = value;
            }
        }

        /// <summary>
        /// PC_W2 structure - Second word (16 bits) following PC_W1 in EPC data
        /// Contains Sensor Command and Sensor Type information
        /// </summary>
        public struct PC_W2
        {
            /// <summary>
            /// Raw 16-bit value of W2
            /// </summary>
            public ushort Value { get; set; }

            /// <summary>
            /// Sensor Command (bits 8-15): Command code for sensor operations
            /// </summary>
            public byte SensorCommand => (byte)((Value >> 8) & 0xFF);

            /// <summary>
            /// Sensor Type (bits 0-7): Type identifier for the sensor
            /// </summary>
            public byte SensorType => (byte)(Value & 0xFF);

            /// <summary>
            /// Creates PC_W2 structure from raw 16-bit value
            /// </summary>
            public PC_W2(ushort value)
            {
                Value = value;
            }
        }

        public PC pc;
        public PC_W1 w1;
        public PC_W2 w2;
        public byte[] EPC;
        public int packetByteSize;

        // Bank 1 data with PC included
        public EpcParser(byte[] Data, int offset)
        {
            if (Data.Length < (offset + 2))
            {
                packetByteSize = 0;
                return;
            }

            int epcOffset = 0;
            int epcLengthByte;

            pc = new PC((UInt16)(Data[offset] << 8 | Data[offset + 1]));
            epcLengthByte = pc.Length * 2;
            packetByteSize = epcLengthByte + 2;

            if (Data.Length < (offset + packetByteSize))
            {
                packetByteSize = 0;
                return; 
            }

            if (pc.XI)
            {
                epcOffset += 2;
                w1 = new PC_W1((UInt16)(Data[offset + 2] << 8 | Data[offset + 3]));
                if (w1.XEB)
                {
                    epcOffset += 2;
                    w2 = new PC_W2((UInt16)(Data[offset + 4] << 8 | Data[offset + 5]));
                }
            }

            EPC = new byte[epcLengthByte - epcOffset];
            Array.Copy(Data, offset + epcOffset + 2, EPC, 0, EPC.Length);
        }
    }
}
