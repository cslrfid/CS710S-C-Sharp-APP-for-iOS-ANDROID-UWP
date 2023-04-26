using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using CSLibrary.Constants;
namespace CSLibrary.Structures
{
    /// <summary>
    /// Tag Kill structure, configure this before do tag kill
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TagKillParms
    {
/*        /// <summary>
        /// Structure size
        /// </summary>
        protected readonly UInt32 Length = 21;
*/
        /// <summary>
        /// The access password for the tags.  A value of zero indicates no 
        /// access password. 
        /// </summary>
        public UInt32 accessPassword;
        /// <summary>
        /// The kill password for the tags.  A value of zero indicates no 
        /// kill password. 
        /// </summary>
        public UInt32 killPassword;
        /// <summary>
        /// Number of retries attemp to read. This field must be between 0 and 15, inclusive.
        /// </summary>
        public UInt32 retryCount;
        /// <summary>
        /// Flag - Zero or combination of  Select or Post-Match
        /// </summary>
        public SelectFlags flags = SelectFlags.UNKNOWN;
        /// <summary>
        /// Extended Kill command
        /// </summary>
        public ExtendedKillCommand extCommand = ExtendedKillCommand.NORMAL;
        /// <summary>
        /// constructor
        /// </summary>
        public TagKillParms()
        {
            // NOP
        }
    }
}