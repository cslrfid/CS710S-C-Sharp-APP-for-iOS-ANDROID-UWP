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

/*
 * CSL RFID Reader hardware specification
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSLibrary.Tools
{
    /// <summary>
    /// Provides hardware specification information and configuration details for CSL RFID readers.
    /// This static class contains device models, chipset types, and antenna configurations for different CSL reader models.
    /// </summary>
    /// <remarks>
    /// The DeviceInformation class serves as a central repository for CSL reader hardware specifications,
    /// enabling applications to determine device capabilities such as antenna configurations, chipset types,
    /// and form factors (fixed vs handheld) based on the device model code.
    /// All antenna numbers are zero-based throughout this class.
    /// </remarks>
    static class DeviceInformation
    {
        /// <summary>
        /// Defines the different RFID chipset types used in CSL reader devices.
        /// </summary>
        /// <remarks>
        /// Each chipset type represents a different generation of RFID technology with varying capabilities,
        /// performance characteristics, and feature sets.
        /// </remarks>
        public enum CHIPSET
        {
            /// <summary>
            /// First generation R1000 chipset for basic RFID operations.
            /// </summary>
            R1000,
            
            /// <summary>
            /// Second generation R2000 chipset with enhanced performance and features.
            /// </summary>
            R2000,
            
            /// <summary>
            /// Latest generation E710/E910 chipset offering advanced RFID capabilities and improved efficiency.
            /// </summary>
            Ex10
        }

        /// <summary>
        /// Defines the supported CSL RFID reader device models with their corresponding numeric codes.
        /// </summary>
        /// <remarks>
        /// Each model represents a specific CSL reader device with unique characteristics such as
        /// form factor (handheld vs fixed), antenna configuration, and chipset type.
        /// The numeric values correspond to the OEM codes stored in the device firmware.
        /// </remarks>
        public enum MODEL
        {
            /// <summary>CS101 handheld RFID reader (Code: 0)</summary>
            CS101 = 0,
            /// <summary>CS203 fixed RFID reader (Code: 1)</summary>
            CS203 = 1,
            /// <summary>CS333 fixed RFID reader (Code: 2)</summary>
            CS333 = 2,
            /// <summary>CS468 fixed RFID reader with 16 antenna support (Code: 3)</summary>
            CS468 = 3,
            /// <summary>CS468INT fixed RFID reader international version (Code: 5)</summary>
            CS468INT = 5,
            /// <summary>CS463 fixed RFID reader with 4 antennas (Code: 6)</summary>
            CS463 = 6,
            /// <summary>CS469 fixed RFID reader (Code: 7)</summary>
            CS469 = 7,
            /// <summary>CS208 fixed RFID reader (Code: 8)</summary>
            CS208 = 8,
            /// <summary>CS209 fixed RFID reader (Code: 9)</summary>
            CS209 = 9,
            /// <summary>CS103 handheld RFID reader (Code: 10)</summary>
            CS103 = 10,
            /// <summary>CS108 handheld RFID reader (Code: 11)</summary>
            CS108 = 11,
            /// <summary>CS206 fixed RFID reader (Code: 12)</summary>
            CS206 = 12,
            /// <summary>CS468X fixed RFID reader enhanced version (Code: 13)</summary>
            CS468X = 13,
            /// <summary>CS203X fixed RFID reader enhanced version (Code: 14)</summary>
            CS203X = 14,
            /// <summary>CS468XJ fixed RFID reader Japan version (Code: 15)</summary>
            CS468XJ = 15,
            /// <summary>CS710S handheld RFID reader with Ex10 chipset (Code: 32)</summary>
            CS710S = 32,
            /// <summary>CS203XL fixed RFID reader long range version (Code: 33)</summary>
            CS203XL = 33,
            /// <summary>Unknown or unrecognized device model (Code: 255)</summary>
            UNKNOWN = 0xff
        }

        public enum INTERFACE
        {
            TCP,
            Bluetooth,
            Serial,
            USB
        }

        /// <summary>
        /// Contains comprehensive hardware specification information for a specific CSL RFID reader device.
        /// </summary>
        /// <remarks>
        /// This class encapsulates all the essential hardware characteristics needed to properly
        /// configure and operate a CSL RFID reader, including antenna configuration, chipset type,
        /// and form factor information. All antenna numbers use zero-based indexing.
        /// </remarks>
        public class DEVICEINFO
        {
            /// <summary>
            /// Gets or sets the OEM model code that identifies the specific device model.
            /// </summary>
            /// <value>A MODEL enumeration value representing the device type.</value>
            public MODEL OemModel;    // code stored in OEM

            /// <summary>
            /// Gets or sets the RFID chipset type used in this device.
            /// </summary>
            /// <value>A CHIPSET enumeration value indicating the chipset generation (R1000, R2000, or Ex10).</value>
            public CHIPSET Chipset;     // Chipset
            
            /// <summary>
            /// Gets or sets the total number of antennas supported by this device.
            /// </summary>
            /// <value>An integer representing the maximum number of antennas that can be connected.</value>
            public int TotalAntenna; // Total Antenna
            
            /// <summary>
            /// Gets or sets the first antenna number in the valid range (zero-based).
            /// </summary>
            /// <value>An integer representing the starting antenna index, typically 0.</value>
            public int FirstAntenna; // First Antenna Number from 0
            
            /// <summary>
            /// Gets or sets the last antenna number in the valid range (zero-based).
            /// </summary>
            /// <value>An integer representing the ending antenna index.</value>
            public int LastAntenna; // Last Antenna Number from 0
            
            /// <summary>
            /// Gets or sets the default antenna number to use when no specific antenna is specified (zero-based).
            /// </summary>
            /// <value>An integer representing the preferred antenna index for operations.</value>
            public int DefaultAntenna; // Default Antenna (from 0)

            public INTERFACE[] SupportedInterfaces;

            /// <summary>
            /// Gets or sets a value indicating whether this device is a fixed reader or handheld device.
            /// </summary>
            /// <value>true if the device is a fixed reader; false if it is a handheld device.</value>
            public bool FixedReader; // true = Fixed, false = Handheld
        }

        /// <summary>
        /// Internal dictionary that maps OEM model codes to their corresponding device information.
        /// All antenna numbers are zero-based throughout the configuration data.
        /// </summary>
        /// <remarks>
        /// This dictionary serves as the master configuration repository for all supported CSL reader models.
        /// Each entry maps a byte-value model code to a complete DEVICEINFO structure containing
        /// hardware specifications, antenna configurations, and operational characteristics.
        /// </remarks>
        // all antenna number is zero based
        private static readonly Dictionary<Byte, DEVICEINFO> deviceInfoDict = new Dictionary<Byte, DEVICEINFO>
        {
            { (Byte)MODEL.CS101, new DEVICEINFO { OemModel = MODEL.CS101,  Chipset = CHIPSET.R1000, TotalAntenna = 1, FirstAntenna = 0, LastAntenna = 0, DefaultAntenna = 0, FixedReader = false } },
            { (Byte)MODEL.CS203, new DEVICEINFO { OemModel = MODEL.CS203,  Chipset = CHIPSET.R1000, TotalAntenna = 2, FirstAntenna = 2, LastAntenna = 3, DefaultAntenna = 3, FixedReader = true } },
            { (Byte)MODEL.CS333, new DEVICEINFO { OemModel = MODEL.CS333,  Chipset = CHIPSET.R2000, TotalAntenna = 0, FirstAntenna = 0, LastAntenna = 0, DefaultAntenna = 0, FixedReader = true } },
            { (Byte)MODEL.CS468, new DEVICEINFO { OemModel = MODEL.CS468,  Chipset = CHIPSET.R1000, TotalAntenna = 16, FirstAntenna = 0, LastAntenna = 0, DefaultAntenna = 0, FixedReader = true } },
            { (Byte)MODEL.CS468INT, new DEVICEINFO { OemModel = MODEL.CS468INT, Chipset = CHIPSET.R1000, TotalAntenna = 16, FirstAntenna = 0, LastAntenna = 0, DefaultAntenna = 0, FixedReader = true } },
            { (Byte)MODEL.CS463, new DEVICEINFO { OemModel = MODEL.CS463,  Chipset = CHIPSET.R2000, TotalAntenna = 4, FirstAntenna = 0, LastAntenna = 3, DefaultAntenna = 0, FixedReader = true } },
            { (Byte)MODEL.CS469, new DEVICEINFO { OemModel = MODEL.CS469,  Chipset = CHIPSET.R1000, TotalAntenna = 1, FirstAntenna = 0, LastAntenna = 0, DefaultAntenna = 0, FixedReader = true } },
            { (Byte)MODEL.CS208, new DEVICEINFO { OemModel = MODEL.CS208,  Chipset = CHIPSET.R1000, TotalAntenna = 1, FirstAntenna = 0, LastAntenna = 3, DefaultAntenna = 0, FixedReader = true } },
            { (Byte)MODEL.CS209, new DEVICEINFO { OemModel = MODEL.CS209,  Chipset = CHIPSET.R2000, TotalAntenna = 1, FirstAntenna = 0, LastAntenna = 0, DefaultAntenna = 0, FixedReader = true } },
            { (Byte)MODEL.CS103, new DEVICEINFO { OemModel = MODEL.CS103,  Chipset = CHIPSET.R2000, TotalAntenna = 0, FirstAntenna = 0, LastAntenna = 3, DefaultAntenna = 0, FixedReader = false } },
            { (Byte)MODEL.CS108, new DEVICEINFO { OemModel = MODEL.CS108,  Chipset = CHIPSET.R2000, TotalAntenna = 1, FirstAntenna = 0, LastAntenna = 0, DefaultAntenna = 0, FixedReader = false } },
            { (Byte)MODEL.CS206, new DEVICEINFO { OemModel = MODEL.CS206,  Chipset = CHIPSET.R2000, TotalAntenna = 0, FirstAntenna = 0, LastAntenna = 3, DefaultAntenna = 0, FixedReader = true } },
            { (Byte)MODEL.CS468X, new DEVICEINFO { OemModel = MODEL.CS468X, Chipset = CHIPSET.R2000, TotalAntenna = 16, FirstAntenna = 0, LastAntenna = 0, DefaultAntenna = 0, FixedReader = true } },
            { (Byte)MODEL.CS203X, new DEVICEINFO { OemModel = MODEL.CS203X, Chipset = CHIPSET.R2000, TotalAntenna = 1, FirstAntenna = 0, LastAntenna = 3, DefaultAntenna = 0, FixedReader = true } },
            { (Byte)MODEL.CS468XJ, new DEVICEINFO { OemModel = MODEL.CS468XJ,Chipset = CHIPSET.R2000, TotalAntenna = 16, FirstAntenna = 0, LastAntenna = 0, DefaultAntenna = 0, FixedReader = true } },
            { (Byte)MODEL.CS710S, new DEVICEINFO { OemModel = MODEL.CS710S, Chipset = CHIPSET.Ex10, TotalAntenna = 1, FirstAntenna = 0, LastAntenna = 0, DefaultAntenna = 0, FixedReader = false } },
            { (Byte)MODEL.CS203XL, new DEVICEINFO { OemModel = MODEL.CS203XL, Chipset = CHIPSET.Ex10, TotalAntenna = 2, FirstAntenna = 0, LastAntenna = 1, DefaultAntenna = 1, FixedReader = true } }
        };

        /// <summary>
        /// Gets an array containing device information for all supported CSL reader models.
        /// </summary>
        /// <value>
        /// An array of DEVICEINFO objects representing the complete hardware specifications
        /// for all supported device models in the CSL reader product line.
        /// </value>
        /// <remarks>
        /// This property provides convenient access to all device configurations without
        /// requiring knowledge of specific model codes. Useful for enumeration and
        /// device discovery scenarios.
        /// </remarks>
        public static DEVICEINFO[] deviceInfo = deviceInfoDict.Values.ToArray();

        /// <summary>
        /// Retrieves device information for a specific CSL reader model using its OEM code.
        /// </summary>
        /// <param name="oemCode">The OEM model code as stored in the device firmware.</param>
        /// <returns>
        /// A DEVICEINFO object containing the hardware specifications for the specified model,
        /// or null if the OEM code is not recognized or supported.
        /// </returns>
        /// <remarks>
        /// This method provides the primary mechanism for obtaining device-specific configuration
        /// information based on the OEM code read from the reader hardware. The OEM code is
        /// typically retrieved during device initialization and used to configure the software
        /// for optimal operation with the specific hardware model.
        /// </remarks>
        /// <example>
        /// <code>
        /// byte oemCode = 11; // CS108 model code
        /// DEVICEINFO info = DeviceInformation.GetDeviceInfoO(oemCode);
        /// if (info != null)
        /// {
        ///     Console.WriteLine($"Model: {info.OemModel}, Chipset: {info.Chipset}");
        ///     Console.WriteLine($"Antennas: {info.TotalAntenna}, Fixed: {info.FixedReader}");
        /// }
        /// </code>
        /// </example>
        public static DEVICEINFO GetDeviceInfoO(Byte oemCode)
        {
            deviceInfoDict.TryGetValue(oemCode, out DEVICEINFO info);
            return info; // null = not found
        }
    }
}
