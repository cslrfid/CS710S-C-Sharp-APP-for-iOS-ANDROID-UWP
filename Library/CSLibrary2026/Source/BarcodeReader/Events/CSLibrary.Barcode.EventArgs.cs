using System;
using System.Collections.Generic;
using System.Text;

using CSLibrary.Barcode.Structures;
using CSLibrary.Barcode.Constants;

namespace CSLibrary.Barcode
{
    /// <summary>
    /// Barcode Event Argument
    /// </summary>
    public class BarcodeEventArgs : EventArgs
    {
        private MessageBase m_msg = null;
        private MessageType m_type = MessageType.ERR_MSG;
        private string m_error = String.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        public BarcodeEventArgs(MessageType type, MessageBase msg)
        {
            m_type = type;
            m_msg = msg;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="error"></param>
        public BarcodeEventArgs(MessageType type, string error)
        {
            m_type = type;
            m_error = error;
        }

        /// <summary>
        /// Decoded Barcode Message
        /// </summary>
        public MessageBase Message
        {
            get
            {
                return m_msg;
            }

        }
        /// <summary>
        /// Capture result
        /// </summary>
        public MessageType MessageType
        {
            get { return m_type; }
        }
        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage
        {
            get { return m_error; }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class BarcodeStateEventArgs : EventArgs
    {
        private BarcodeState m_state = BarcodeState.IDLE;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state"></param>
        public BarcodeStateEventArgs(BarcodeState state)
        {
            m_state = state;
        }
        /// <summary>
        /// Current operation state
        /// </summary>
        public BarcodeState State
        {
            get { return m_state; }
        }
    }
}
