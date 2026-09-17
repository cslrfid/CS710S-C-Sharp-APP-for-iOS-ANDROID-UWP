using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CSLibrary.Events
{
    using CSLibrary.Structures;
    using CSLibrary.Constants;

    /// <summary>
    /// Reader status callback event argument
    /// </summary>
    public class OnReaderStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Callback Tag Information
        /// </summary>
        public readonly object info;
        /// <summary>
        /// Async callback data type
        /// </summary>
        public readonly ReaderCallbackType type = ReaderCallbackType.UNKNOWN;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">Tag Information</param>
        /// <param name="type">Callback Type</param>
        public OnReaderStateChangedEventArgs(object info, ReaderCallbackType type)
        {
            this.info = info;
            this.type = type;
        }
    }

    /// <summary>
    /// Inventory or tag search callback event argument
    /// </summary>
    public class OnAsyncCallbackEventArgs : EventArgs
    {
        /// <summary>
        /// Callback Tag Information
        /// </summary>
        public readonly TagCallbackInfo info = new TagCallbackInfo();
        /// <summary>
        /// Async callback data type
        /// </summary>
        public readonly CallbackType type = CallbackType.UNKNOWN;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">Tag Information</param>
        /// <param name="type">Callback Type</param>
        public OnAsyncCallbackEventArgs(TagCallbackInfo info, CallbackType type)
        {
            this.info = info;
            this.type = type;
        }
    }
    /// <summary>
    /// Tag Access Completed Argument
    /// </summary>
    public class OnAccessCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Access Result
        /// </summary>     
        public readonly bool success = false;
        /// <summary>
        /// Access bank
        /// </summary>
        public readonly Bank bank = Bank.UNKNOWN;
        /// <summary>
        /// Access Type
        /// </summary>
        public readonly TagAccess access = TagAccess.UNKNOWN;
        /// <summary>
        /// Access Data only use for Tag Read operation
        /// </summary>
        public readonly IBANK data;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="success">Access Result</param>
        /// <param name="bank">Access bank</param>
        /// <param name="access">Access type</param>
        /// <param name="data">Access Data only use for Tag Read operation</param>
        //public OnAccessCompletedEventArgs(bool success, Bank bank, TagAccess access, IBANK data)
        public OnAccessCompletedEventArgs(bool success, Bank bank, TagAccess access, IBANK data)
        {
            this.access = access;
            this.success = success;
            this.bank = bank;
            this.data = data;
        }
    }

    /// <summary>
    /// FM13DT Tag Access Completed Argument
    /// </summary>
    public class OnFM13DTAccessCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Access Result
        /// </summary>     
        public readonly bool success = false;
        /// <summary>
        /// Access Type
        /// </summary>
        public readonly FM13DTAccess access = FM13DTAccess.UNKNOWN;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="success">Access Result</param>
        /// <param name="access">Access type</param>
        //public OnAccessCompletedEventArgs(bool success, Bank bank, TagAccess access, IBANK data)
        public OnFM13DTAccessCompletedEventArgs(FM13DTAccess access, bool success)
        {
            this.access = access;
            this.success = success;
        }
    }

    /// <summary>
    /// Reader Operation changed EventArgs
    /// </summary>
    public class OnStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Current operation state
        /// </summary>
        public readonly RFState state = RFState.IDLE;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state"></param>
        public OnStateChangedEventArgs(RFState state)
        {
            this.state = state;
        }
    }

    
    public class OnInventoryTagRateCallbackEventArgs : EventArgs
    {
        /// <summary>
        /// Current operation state
        /// </summary>
        public readonly uint inventoryTagRate = 0;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state"></param>
        public OnInventoryTagRateCallbackEventArgs(uint tagRate)
        {
            this.inventoryTagRate = tagRate;
        }
    }


}
