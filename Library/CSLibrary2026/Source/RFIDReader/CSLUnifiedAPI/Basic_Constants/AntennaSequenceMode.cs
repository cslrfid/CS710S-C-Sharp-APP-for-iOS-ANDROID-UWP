using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.Constants
{
    /// <summary>
    /// AntennaSequenceMode
    /// </summary>
    [Flags]
    public enum AntennaSequenceMode
    {
        /// <summary>
        /// Normal mode
        /// </summary>
        NORMAL,
        /// <summary>
        /// Sequence Mode
        /// </summary>
        SEQUENCE,
        /// <summary>
        /// Smart check mode
        /// </summary>
        SMART_CHECK,
        /// <summary>
        /// Combination of Sequence and Smart Check
        /// </summary>
        SEQUENCE_SMART_CHECK,
        /// <summary>
        /// Unknown
        /// </summary>
        UNKNOWN = 0x4
    }
}
