using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.Constants
{
    /// <summary>
    /// Select Flags
    /// </summary>
    [Flags]
    public enum SelectFlags
    {
        /// <summary>
        /// Normal Inventory
        /// </summary>
        //NORMAL = 0x0000000,
        ZERO = 0x0000000,
        /// <summary>
        /// Use Select Criteria
        /// </summary>
        SELECT = 0x00000001,
        /// <summary>
        /// Use Post-Match Criteria
        /// </summary>
        POSTMATCH = 0x00000002,
        /// <summary>
        /// Using Post-Match Criterion
        /// </summary>
        POST_MATCH = 0x2,
        /// <summary>
        /// Using Post-Match Criterion
        /// </summary>
        FILTER = 0x3,
        /// <summary>
        /// Disable Inventory
        /// </summary>
        DISABLE_INVENTORY = 0x00000004,
        /// <summary>
        /// Read 1 bank after Inventory
        /// </summary>
        READ1BANK = 0x00000008,
        /// <summary>
        /// Read 2 bank after Inventory
        /// </summary>
        READ2BANK = 0x00000010,
        /// <summary>
        /// Unknown
        /// </summary>
        UNKNOWN = 0xffff,
    }
}
