using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using CSLibrary.Constants;

namespace CSLibrary.Structures
{
    /// <summary>
    /// Tag lock structure, configure this before do tag lock
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TagLockParms
    {
        /// <summary>
        /// Structure size
        /// </summary>
        protected readonly UInt32 length = (UInt32)(16 + 20);
        /// <summary>
        /// The access password for the tags.  A value of zero indicates no 
        /// access password. 
        /// </summary>
        public UInt32 accessPassword;
        /// <summary>
        /// Number of retries attemp to read. This field must be between 0 and 15, inclusive.
        /// </summary>
        public UInt32 retryCount;
        /// <summary>
        /// The access permissions for the tag's kill password.  
        /// </summary>
        public Permission killPasswordPermissions = Permission.UNCHANGED;
        /// <summary>
        /// The access permissions for the tag's access password. 
        /// </summary>
        public Permission accessPasswordPermissions = Permission.UNCHANGED;
        /// <summary>
        /// The access permissions for the tag's EPC memory bank.  
        /// </summary>
        public Permission epcMemoryBankPermissions = Permission.UNCHANGED;
        /// <summary>
        /// The access permissions for the tag's TID memory bank.
        /// </summary>
        public Permission tidMemoryBankPermissions = Permission.UNCHANGED;
        /// <summary>
        /// The access permissions for the tag's user memory bank.
        /// </summary>
        public Permission userMemoryBankPermissions = Permission.UNCHANGED;
        /// <summary>
        /// The FFFFF Permanent Lock.
        /// </summary>
        public bool permanentLock = false;
        /// <summary>
        /// Flag - Zero or combination of  Select or Post-Match
        /// </summary>
        public SelectFlags flags = SelectFlags.UNKNOWN;
        /// <summary>
        /// constructor
        /// </summary>
        public TagLockParms()
        {
            // NOP
        }
    }

    /// <summary>
    /// block lock structure, configure this before do block lock
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TagBlockPermalockParms
    {
        /// <summary>
        /// Structure size
        /// </summary>
        public readonly UInt32 length = (UInt32)(24 + IntPtr.Size);
        /// <summary>
        /// The access password for the tags.  A value of zero indicates no 
        /// access password. 
        /// </summary>
        public UInt32 accessPassword;
        /// <summary>
        /// True to set permalock, otherwise read it state
        /// </summary>
        public bool setPermalock;
        /// <summary>
        /// Number of retries attemp to read. This field must be between 0 and 15, inclusive.
        /// </summary>
        public UInt32 retryCount;
        /// <summary>
        /// Flag - Zero or combination of  Select or Post-Match
        /// </summary>
        public SelectFlags flags = SelectFlags.UNKNOWN;
        /// <summary>
        /// 
        /// </summary>
        public UInt16 count;
        /// <summary>
        /// 
        /// </summary>
        public UInt16 offset;
        /// <summary>
        /// 
        /// </summary>
        public UInt16[] mask = new UInt16[0];
        /// <summary>
        /// constructor
        /// </summary>
        public TagBlockPermalockParms()
        {
            // NOP
        }
    }
}