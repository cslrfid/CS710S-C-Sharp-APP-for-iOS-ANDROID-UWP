/*
Copyright (c) 2023 Convergence Systems Limited

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
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;

#if WindowsCE
using CSLibrary.Threading;
#endif


namespace CSLibrary.Net
{
    #region Enum
    /// <summary>
    /// Current Packet Recevice mode
    /// </summary>
    public enum RecvOperation
    {
        /// <summary>
        /// Searching device
        /// </summary>
        SEARCH,
        /// <summary>
        /// Assigment operation
        /// </summary>
        ASSIGN,
        /// <summary>
        /// Update in progress
        /// </summary>
        UPDATE,
        /// <summary>
        /// Idle
        /// </summary>
        IDLE,
        /// <summary>
        /// 
        /// </summary>
        CLOSED,
    }
    /// <summary>
    /// Assignment Result
    /// </summary>
    public enum Result
    {
        /// <summary>
        /// No error.
        /// </summary>
        OK,
        /// <summary>
        /// Gernal fail.
        /// </summary>
        FAIL,
        /// <summary>
        /// Operation already in progress. Please stop operation and retry again.
        /// </summary>
        OPERATION_BUSY,
        /// <summary>
        /// target MAC address not found in current search list, 
        /// please try SearchDevice() first or check you network connection carefully.
        /// </summary>
        DATA_NOT_FOUND,
        /// <summary>
        /// Invalid parameter.
        /// </summary>
        INVALID_PARAMETER,
        /// <summary>
        /// Assign configure is same as current configure.
        /// </summary>
        NO_CHANGED,
        /// <summary>
        /// fail to build assign buffer internallay.
        /// </summary>
        BUILD_ASSIGN_BUFFER_FAIL,
        /// <summary>
        /// fail to send assignment packet.
        /// </summary>
        SEND_ASSIGN_FAIL,
        /// <summary>
        /// Unknown error.
        /// </summary>
        UNKNOWN,
    }

    /// <summary>
    /// Result
    /// </summary>
    public enum AssignResult
    {
        /// <summary>
        /// Accepted from device
        /// </summary>
        ACCEPTED,
        /// <summary>
        /// Rejected from device
        /// </summary>
        REJECTED,
        /// <summary>
        /// response timeout
        /// </summary>
        TIMEOUT,
        /// <summary>
        /// assignment started, please wait to finished
        /// </summary>
        STARTED,
        /// <summary>
        /// Unknown
        /// </summary>
        UNKNOWN,
    }

    /// <summary>
    /// Update Result
    /// </summary>
    public enum UpdateResult
    {
        /// <summary>
        /// Update success
        /// </summary>
        SUCCESS,
        /// <summary>
        /// Update fail
        /// </summary>
        FAIL
    }

    public enum ApiMode
    {
        HIGHLEVEL,
        LOWLEVEL,
        UNKNOWN
    }


    enum EBOOT
    {
        // Define packet types
        RRQ =    0x01,
        WRQ =    0x02,
        DATA =   0x03,
        ACK =    0x04,
        ERROR =  0x05,
    }


    enum EBOOT_ERR
    {
        // Error Codes
        NOT_DEFINED              = 0, // Not defined, see error message (if any).
        INVALID_OPCODE           = 1, // File not found.
        INVALID_BLOCK_NUMBER     = 2, // Access violation.
        INVALID_BLOCK_LENGTH     = 3, // Disk full or allocation exceeded.
        FLASH_ERROR              = 4, // Illegal operation.
        INVALID_PACKET_SIZE      = 5, // Unknown transfer ID.
    }

    /// <summary>
    /// TFTP opcodes
    /// </summary>
    enum Opcodes
    {
        Unknown = 0,
        Read = 1,
        Write = 2,
        Data = 3,
        Ack = 4,
        Error = 5
    }

    /// <summary>
    /// TFTP modes
    /// </summary>
    enum Modes
    {
        Unknown = 0,
        NetAscii = 1,
        Octet = 2,
        Mail = 3
    }

    #endregion

    /// <summary>
    /// Search device on ethernet
    /// </summary>
    public class NetFinder : IDisposable
    {
        private object m_lock = new object();
        private Socket m_broadcast = null;
        private Socket m_tcpsocket = null;
        private DeviceCollection m_devices = new DeviceCollection();

        private int m_rand = 0;
        private bool m_stop = true;
        private bool m_stoped = true;
        private bool m_research = false;
        private Timer m_timeout;
        private uint u_timeout = 1000;  // 8000
        private RecvOperation m_operation = RecvOperation.CLOSED;
        private string m_lastErrorMessage = "";

        private BackgroundWorker bWorkerEboot = new BackgroundWorker();
        private BackgroundWorker bWorkerImage = new BackgroundWorker();

        private AssignInfo assignInfo = new AssignInfo();

        private IPAddress m_targetSearch = new IPAddress(0);

#if NOUSE
        private bool m_connected = false;
        private uint u_timeout = 5000;
        /// <summary>
        /// Connect timeout
        /// </summary>
        public uint ConnectTimeout
        {
            get { lock (m_lock)return u_timeout; }
            set { lock (m_lock)u_timeout = value; }
        }
        /// <summary>
        /// Connected?
        /// </summary>
        public bool IsConnected
        {
            get { lock (m_lock) { return m_connected; } }
        }

        private bool l_connected
        {
            get { lock (m_lock) { return m_connected; } }
            set { lock (m_lock) { m_connected = value; } }
        }
        /// <summary>
        /// Connect device callback event
        /// </summary>
        public event EventHandler<ResultArgs> OnConnectCompleted;
#endif

        /// <summary>
        /// Get current recevice mode operation
        /// </summary>
        public RecvOperation Operation
        {
            get { lock (m_lock) { return m_operation; } }
            set { lock (m_lock) { m_operation = value; } }
        }
        /// <summary>
        /// Get last error message
        /// </summary>
        public String LastError
        {
            get { lock (m_lock) { return m_lastErrorMessage; } }
            protected set { lock (m_lock) { m_lastErrorMessage = value; } }
        }
        /// <summary>
        /// Get Radios list in the same network
        /// </summary>
        public List<DeviceInfomation> Radios
        {
            get { lock (m_lock) { return m_devices; } }
        }

        public int Count
        {
            get { return m_devices.Count; }
        }

        /// <summary>
        /// Search device callback event
        /// </summary>
        public event EventHandler<DeviceFinderArgs> OnSearchCompleted;
        /// <summary>
        /// Assign device callback event
        /// </summary>
        public event EventHandler<ResultArgs> OnAssignCompleted;
        /// <summary>
        /// Update eboot/image callback event
        /// </summary>
        public event EventHandler<UpdateResultArgs> OnUpdateCompleted;
        /// <summary>
        /// Update total update percent
        /// </summary>
        public event EventHandler<UpdatePercentArgs> OnUpdatePercent;

        public int memcmp(byte[] b1, byte[] b2, int length)
        {
            int result = 0;

            if (b1.Length < length || b2.Length < length)
            {
                if (b1.Length > b2.Length)
                    return 1;
                else
                    return -1;
            }

            for (int i = 0; i < length; i++)
            {
                if (b1[i] != b2[i])
                {
                    result = (int)(b1[i] - b2[i]);
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public NetFinder()
        {
            try
            {
                m_timeout = new Timer(new TimerCallback(TimeoutCb), null, Timeout.Infinite, Timeout.Infinite);

                m_broadcast = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                m_broadcast.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                //m_broadcast.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.BlockSource, 1);
                //m_broadcast.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
#if WIN32
                m_broadcast.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
#endif
                //Start Listening here
                Operation = RecvOperation.IDLE;

                Thread run = new Thread(new ThreadStart(RecvThread));
                run.IsBackground = true;
                run.Start();

                //initial background worker
                bWorkerEboot.DoWork += new DoWorkEventHandler(bWorker_DoWork);
                bWorkerEboot.ProgressChanged += new ProgressChangedEventHandler(bWorker_ProgressChanged);
                bWorkerEboot.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bWorker_RunWorkerCompleted);
                bWorkerEboot.WorkerReportsProgress = true;
                bWorkerEboot.WorkerSupportsCancellation = true;

                bWorkerImage.DoWork += new DoWorkEventHandler(bWorkerImage_DoWork);
                bWorkerImage.ProgressChanged += new ProgressChangedEventHandler(bWorkerImage_ProgressChanged);
                bWorkerImage.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bWorkerImage_RunWorkerCompleted);
                bWorkerImage.WorkerReportsProgress = true;
                bWorkerImage.WorkerSupportsCancellation = false;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }

        }

        /// <summary>
        /// Dispose resource
        /// </summary>
        public void Dispose()
        {
            if (bWorkerEboot != null)
            {
                bWorkerEboot.DoWork -= new DoWorkEventHandler(bWorker_DoWork);
                bWorkerEboot.ProgressChanged -= new ProgressChangedEventHandler(bWorker_ProgressChanged);
                bWorkerEboot.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(bWorker_RunWorkerCompleted);
            }
            if (bWorkerImage != null)
            {
                bWorkerImage.DoWork -= new DoWorkEventHandler(bWorkerImage_DoWork);
                bWorkerImage.ProgressChanged -= new ProgressChangedEventHandler(bWorkerImage_ProgressChanged);
                bWorkerImage.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(bWorkerImage_RunWorkerCompleted);
            }
            if (m_broadcast != null)
            {
                //Stop Operation before dispose
                Stop();

                Operation = RecvOperation.CLOSED;

                Thread.Sleep(1);
                m_broadcast.Shutdown(SocketShutdown.Both);
                m_broadcast.Close();
                m_broadcast = null;
            }
            if (m_timeout != null)
            {
                m_timeout.Dispose();
            }
        }
        /// <summary>
        /// Start to search USB device first and search on ethernet continuously until Stop function called.
        /// </summary>
        public void SearchDevice()
        {
            SearchSerialDeivce();
            SearchUsbDevice();
            SearchDevice(IPAddress.Broadcast);
        }

        /// <summary>
        /// Start to search device on ethernet continuously until Stop function called.
        /// </summary>
        public void SearchDevice(IPAddress ip)
        {
            if (Operation != RecvOperation.IDLE)
            {
                return;
            }

            if (ip == null)
                return;

            m_targetSearch.Address = ip.Address;
            new Thread(new ThreadStart(StartDeviceDiscovery)).Start();
        }

        /// <summary>
        /// Start to search device on USB.
        /// </summary>
        public void SearchUsbDevice()
        {
            HighLevelInterface Reader = new HighLevelInterface();

            List<string> UsbDevices = new List<string>();

            Reader.GetUsbDevicesList(ref UsbDevices);

            for (int cnt = 0; cnt < UsbDevices.Count; cnt++)
            {
                DeviceInfomation entry = new DeviceInfomation();

                entry.Mode = Mode.NormalUsb;
                entry.DeviceName = "USB" + UsbDevices[cnt];
                entry.Description = "RFID Reader detected in USB port";
                Debug.WriteLine(String.Format("add device listdevice_name {0}", entry.DeviceName));
                m_devices.Add(entry);

                RaiseEvent<DeviceFinderArgs>(OnSearchCompleted, this, new DeviceFinderArgs(entry));
            }
        }

        /// <summary>
        /// Start to search device on Serial Ports.
        /// </summary>
        public void SearchSerialDeivce()
        {
            HighLevelInterface Reader = new HighLevelInterface();

            List<string> SerialDevices = new List<string>();

            Reader.GetSerialDevicesList(ref SerialDevices);

            for (int cnt = 0; cnt < SerialDevices.Count; cnt++)
            {
                DeviceInfomation entry = new DeviceInfomation();

                entry.Mode = Mode.NormalSerial;
                entry.DeviceName = SerialDevices[cnt];
                entry.Description = "RFID Reader detected in Serial port";
                Debug.WriteLine(String.Format("add device listdevice_name {0}", entry.DeviceName));
                m_devices.Add(entry);

                RaiseEvent<DeviceFinderArgs>(OnSearchCompleted, this, new DeviceFinderArgs(entry));
            }
        }

        
        /// <summary>
        /// Start to re-search device on ethernet continuously until Stop function called.
        /// </summary>
        public void ResearchDevice()
        {
            if (Operation == RecvOperation.SEARCH)
                lock (m_lock) m_research = true;
        }
        /// <summary>
        /// Change Target device IP, trusted server address
        /// </summary>
        /// <param name="targetMacAddress"></param>
        /// <param name="ipAddressTo"></param>
        /// <param name="trustedServerAddress"></param>
        /// <param name="trustedServerEnabled"></param>
        /// <returns></returns>
        /// <remarks>You must call SearchDevice() before call this function.</remarks>
        public Result AssignDevice(Byte[] targetMacAddress, Byte[] ipAddressTo, Byte[] trustedServerAddress, bool trustedServerEnabled)
        {
            if (Operation != RecvOperation.IDLE)
            {
                return Result.OPERATION_BUSY;
            }

            if (ipAddressTo == null || ipAddressTo.Length != 4 || 
                targetMacAddress == null || targetMacAddress.Length != 6)
                return Result.INVALID_PARAMETER;

            DeviceInfomation entry = GetTargetEntry(targetMacAddress);

            if (entry == null)
                return Result.DATA_NOT_FOUND;
            else
            {
//                bool checkModified = Win32Wrapper.memcmp(ipAddressTo, entry.IPAddress.Address, 4) != 0 ||
//                                    Win32Wrapper.memcmp(trustedServerAddress, entry.TrustedServer.Address, 4) != 0 ||
//                                    trustedServerEnabled != entry.TrustedServerEnabled;
                bool checkModified = memcmp(ipAddressTo, entry.IPAddress.Address, 4) != 0 ||
                    memcmp(trustedServerAddress, entry.TrustedServer.Address, 4) != 0 ||
                    trustedServerEnabled != entry.TrustedServerEnabled;

                if (!checkModified)
                    return Result.NO_CHANGED;
            }

            RaiseEvent<ResultArgs>(OnAssignCompleted, this, new ResultArgs(AssignResult.STARTED));

            Operation = RecvOperation.ASSIGN;

            return SendAssignment(entry.IPAddress.Address, ipAddressTo, targetMacAddress, entry.DeviceName, entry.DHCPEnabled, entry.DHCPRetry, trustedServerAddress, trustedServerEnabled, entry.SubnetMask.Address, entry.Gateway.Address, entry.GatewayCheckResetMode);
        }

        /// <summary>
        /// Assign target device information
        /// </summary>
        /// <param name="targetMacAddress"></param>
        /// <param name="ipAddressTo"></param>
        /// <param name="DHCPRetry"></param>
        /// <param name="DHCPEnabled"></param>
        /// <returns></returns>
        /// <remarks>You must call SearchDevice() before call this function.</remarks>
        public Result AssignDevice(Byte[] targetMacAddress, Byte[] ipAddressTo, byte DHCPRetry, bool DHCPEnabled)
        {
            if (Operation != RecvOperation.IDLE)
            {
                return Result.OPERATION_BUSY;
            }

            if (ipAddressTo == null || ipAddressTo.Length != 4 || targetMacAddress == null || targetMacAddress.Length != 6)
                return Result.INVALID_PARAMETER;

            DeviceInfomation entry = GetTargetEntry(targetMacAddress);

            if (entry == null)
                return Result.DATA_NOT_FOUND;
            else
            {
                bool checkModified = memcmp(ipAddressTo, entry.IPAddress.Address, 4) != 0 ||
                                    DHCPEnabled != entry.DHCPEnabled ||
                                    DHCPRetry != entry.DHCPRetry;

                if (!checkModified)
                    return Result.NO_CHANGED;
            }

            RaiseEvent<ResultArgs>(OnAssignCompleted, this, new ResultArgs(AssignResult.STARTED));

            Operation = RecvOperation.ASSIGN;

            return SendAssignment(entry.IPAddress.Address, ipAddressTo, targetMacAddress, entry.DeviceName, DHCPEnabled, DHCPRetry, entry.TrustedServer.Address, entry.TrustedServerEnabled, entry.SubnetMask.Address, entry.Gateway.Address, entry.GatewayCheckResetMode);
        }
        /// <summary>
        /// Assign target device information
        /// </summary>
        /// <param name="targetMacAddress"></param>
        /// <param name="ipAddressTo"></param>
        /// <param name="deviceName"></param>
        /// <param name="DHCPRetry"></param>
        /// <param name="DHCPEnabled"></param>
        /// <returns></returns>
        /// <remarks>You must call SearchDevice() before call this function.</remarks>
        public Result AssignDevice(Byte[] targetMacAddress, Byte[] ipAddressTo, string deviceName, byte DHCPRetry, bool DHCPEnabled)
        {
            if (Operation != RecvOperation.IDLE)
            {
                return Result.OPERATION_BUSY;
            }

            if (ipAddressTo == null || ipAddressTo.Length != 4 || deviceName == null || deviceName.Length > 31)
                return Result.INVALID_PARAMETER;

            DeviceInfomation entry = GetTargetEntry(targetMacAddress);

            if (entry == null)
                return Result.DATA_NOT_FOUND;
            else
            {
                bool checkModified = memcmp(ipAddressTo, entry.IPAddress.Address, 4) != 0 ||
                                    deviceName != entry.DeviceName ||
                                    DHCPEnabled != entry.DHCPEnabled ||
                                    DHCPRetry != entry.DHCPRetry;

                if (!checkModified)
                    return Result.NO_CHANGED;
            }

            RaiseEvent<ResultArgs>(OnAssignCompleted, this, new ResultArgs(AssignResult.STARTED));

            Operation = RecvOperation.ASSIGN;

            return SendAssignment(entry.IPAddress.Address, ipAddressTo, targetMacAddress, deviceName, DHCPEnabled, DHCPRetry, entry.TrustedServer.Address, entry.TrustedServerEnabled, entry.SubnetMask.Address, entry.Gateway.Address, entry.GatewayCheckResetMode);
        }

        /// <summary>
        /// Assign target device information
        /// </summary>
        /// <param name="targetMacAddress">Target device address(You must call SearchDevice() to discovery all device first)</param>
        /// <param name="ipAddressTo">Changable IP only use if DHCPEnabled is <c>False</c></param>
        /// <param name="deviceName">target device name</param>
        /// <param name="DHCPRetry">DHCP retry count</param>
        /// <param name="DHCPEnabled">If set to True, DHCP will assign dynamic IP.</param>
        /// <param name="trustedServerAddress">Trusted Server Address</param>
        /// <param name="trustedServerEnabled">If set to True, target device can only connect to trusted server</param>
        /// <param name="subnet">Subnet Mask</param>
        /// <param name="gateway">Gateway</param>
        /// <returns></returns>
        public Result AssignDevice(Byte[] targetMacAddress, Byte[] ipAddressTo, string deviceName, byte DHCPRetry, bool DHCPEnabled, Byte[] trustedServerAddress, bool trustedServerEnabled, Byte[] subnet, Byte[] gateway, int GatewayCheckResetMode)
        {
            if (Operation != RecvOperation.IDLE)
            {
                return Result.OPERATION_BUSY;
            }

            if (ipAddressTo == null || ipAddressTo.Length != 4 || deviceName == null || deviceName.Length > 31)
                return Result.INVALID_PARAMETER;

            DeviceInfomation entry = GetTargetEntry(targetMacAddress);

            if (entry == null)
                return Result.DATA_NOT_FOUND;
            else
            {
                bool checkModified = memcmp(ipAddressTo, entry.IPAddress.Address, 4) != 0 ||
                                    deviceName != entry.DeviceName ||
                                    DHCPEnabled != entry.DHCPEnabled ||
                                    DHCPRetry != entry.DHCPRetry ||
                                    memcmp(trustedServerAddress, entry.TrustedServer.Address, 4) != 0 ||
                                    trustedServerEnabled != entry.TrustedServerEnabled ||
                                    memcmp(subnet, entry.SubnetMask.Address, 4) != 0 ||
                                    memcmp(gateway, entry.Gateway.Address, 4) != 0 ||
                                    GatewayCheckResetMode != entry.GatewayCheckResetMode;

                if (!checkModified)
                    return Result.NO_CHANGED;
            }

            RaiseEvent<ResultArgs>(OnAssignCompleted, this, new ResultArgs(AssignResult.STARTED));

            Operation = RecvOperation.ASSIGN;

            return SendAssignment(entry.IPAddress.Address, ipAddressTo, targetMacAddress, deviceName, DHCPEnabled, DHCPRetry, trustedServerAddress, trustedServerEnabled,subnet, gateway, GatewayCheckResetMode);
        }
        private DeviceInfomation GetTargetEntry(Byte[] TargetMAC)
        {
            // Check the mac address of each cell in the table
            return m_devices.Find(delegate(DeviceInfomation info) { return info.MACAddress.Equals(TargetMAC); });
        }
#if NOUSE

        /// <summary>
        /// Connect to Target CS203 Device. Notes : change from UDP mode to TCP Mode. 
        /// After that other netfinder will not find this device any more until exit the program or reboot device
        /// </summary>
        /// <param name="CS203IP"></param>
        /// <returns></returns>
        public bool ConnectDevice(Byte[] CS203IP)
        {
            if (Operation != RecvOperation.IDLE)
            {
                return false;
            }

            /*if (!m_netdisplay.IsCellSelected())
                return false;*/

            Operation = RecvOperation.CONNECT;

            return Connect(CS203IP);
        }
        /// <summary>
        /// Connect to Target CS203 Device. Notes : change from UDP mode to TCP Mode. 
        /// After that other netfinder will not find this device any more until exit the program or reboot device
        /// </summary>
        /// <param name="CS203IP"></param>
        /// <returns></returns>
        public bool ConnectDevice(String CS203IP)
        {
            if (Operation != RecvOperation.IDLE)
            {
                return false;
            }

            /*if (!m_netdisplay.IsCellSelected())
                return false;*/

            Operation = RecvOperation.CONNECT;

            return Connect(CS203IP);
        }
        /*/// <summary>
        /// Connect to Target CS203 Device. Notes : change from UDP mode to TCP Mode. 
        /// After that other netfinder will not find this device any more until exit the program or reboot device
        /// </summary>
        /// <returns></returns>
        public bool ConnectDevice()
        {
            if (Operation != RecvOperation.IDLE)
            {
                return false;
            }

            if (!m_netdisplay.IsCellSelected())
                return false;

            Byte[] IP = new byte[4];

            if (!m_netdisplay.GetIPAddress(ref IP))
                return false;

            Operation = RecvOperation.CONNECT;

            return Connect(IP);
        }*/
        

#endif
        private void TimeoutCb(Object stateInfo)
        {
            switch (Operation)
            {
                case RecvOperation.ASSIGN:
                    Operation = RecvOperation.IDLE;
                    RaiseEvent<ResultArgs>(OnAssignCompleted, this, new ResultArgs(AssignResult.TIMEOUT));
                    break;
#if NOUSE
                case RecvOperation.CONNECT:
                    Operation = RecvOperation.IDLE;
                    RaiseEvent<ResultArgs>(OnConnectCompleted, this, new ResultArgs(Result.TIMEOUT));
                    break;
#endif
                default: break;
            }
        }

        /// <summary>
        /// Stop to search
        /// </summary>
        public void Stop()
        {
            Operation = RecvOperation.IDLE;

            lock (m_lock)
            {

                m_stop = true;

                while (!m_stoped)
                {
                    Thread.Sleep(1);
                }
            }
        }
        /// <summary>
        /// Clear all device list
        /// </summary>
        public void ClearDeviceList()
        {
            m_devices.Clear();
        }
#if NOUSE
        /// <summary>
        /// Connect to Target Device. Notes : change from UDP mode to TCP Mode. 
        /// After that other netfinder will not find this device any more until exit the program
        /// </summary>
        /// <param name="IP"></param>
        /// <returns>True if send success</returns>
        private bool Connect(byte[] IP)
        {
            if (IP == null || IP.Length != 4)
                return false;
            byte[] buff = new byte[] { 3, 0, 0, 0, 0, 0 };
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, 3040);

            buff[2] = IP[0];
            buff[3] = IP[1];
            buff[4] = IP[2];
            buff[5] = IP[3];

            try
            {
                Operation = RecvOperation.CONNECT;

                l_connected = false;

                m_timeout.Change(u_timeout, Timeout.Infinite); //Start timeout here

                broadcast.SendTo(buff, endPoint);

                rfid.Linkage.Sleep(100);
            }
            catch (System.Exception e)
            {
                //    m_networkerror = 1;
                Debug.WriteLine(e.Message + " Unable To Send Packet. Please Check Network Connection.");

                m_timeout.Change(Timeout.Infinite, Timeout.Infinite); //Start timeout here

                return false;
            }
            return true;
        }

        /// <summary>
        /// Connect to Target Device. Notes : change from UDP mode to TCP Mode. 
        /// After that other netfinder will not find this device any more until exit the program
        /// </summary>
        /// <param name="IP"></param>
        /// <returns>True if send success</returns>
        private bool Connect(string IP)
        {
            if (IP == null)
                return false;
            string[] sip = IP.Split(new char[] { '.' });
            if (sip.Length != 4)
                return false;

            byte[] Sendbuf = new byte[] { 3, 0, 0, 0, 0, 0 };
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, 3040);

            Sendbuf[2] = Byte.Parse(sip[0]);
            Sendbuf[3] = Byte.Parse(sip[1]);
            Sendbuf[4] = Byte.Parse(sip[2]);
            Sendbuf[5] = Byte.Parse(sip[3]);

            try
            {
                Operation = RecvOperation.CONNECT;

                l_connected = false;

                m_timeout.Change(u_timeout, Timeout.Infinite); //Start timeout here

                broadcast.SendTo(Sendbuf, endPoint);

                rfid.Linkage.Sleep(100);

            }
            catch (System.Exception e)
            {
                //    m_networkerror = 1;
                Debug.WriteLine(e.Message + " Unable To Send Packet. Please Check Network Connection.");

                m_timeout.Change(Timeout.Infinite, Timeout.Infinite); //Start timeout here

                return false;
            }

            return true;
        }
#endif
        private void RecvThread()
        {
            while (true)
            {

                switch (Operation)
                {
                    case RecvOperation.ASSIGN:
                        ReceivePacketFromAssign();
                        break;
#if NOUSE
                    case RecvOperation.CONNECT:
                        ReceivePacketFromConnect();
                        break;
#endif
                    case RecvOperation.SEARCH:
                        ReceivePacketFromSearch();
                        break;
                    case RecvOperation.IDLE:
                        Thread.Sleep(1);
                        break;
                    case RecvOperation.CLOSED:
                        return;
                }

            }
        }

        private void StartDeviceDiscovery()
        {
            m_stoped = false;

            m_stop = false;

            while (true)
            {
                SendBroadcast();

                do
                {
                    //rfid.Linkage.Sleep(1000);
                    Thread.Sleep(1000);

                    ResendBroadcast();

                    if (m_research)
                        break;

                } while (!m_stop);

                m_research = false;

                if (m_stop)
                    break;
            }
            m_stoped = true;

        }

        /*public int SelectDeviceIndex
        {
            get { return m_netdisplay.SelectDeviceIndex; }
            set { m_netdisplay.SelectDeviceIndex = value; }
        }

        public byte[] GetDeviceIP()
        {
            byte[] ip = null;
            m_netdisplay.GetIPAddress(ref ip);
            return ip;
        }

        public string GetDeviceName()
        {
            string name = null;
            m_netdisplay.GetDeviceName(ref name);
            return name;
        }

        public byte GetTimeout()
        {
            int timeout = 0;
            m_netdisplay.GetTimeout(ref timeout);
            return (byte)timeout;
        }

        public string GetSelectDeviceIP()
        {
            byte[] ip = new byte[4];
            if (m_netdisplay.GetIPAddress(ref ip))
                return string.Format("{0}.{1}.{2}.{3}", ip[0], ip[1], ip[2], ip[3]);
            else
                return null;
        }*/


        private void SendAllBroadcast(byte[] buff)
        {
            IPEndPoint endPoint;

            try
            {
#if WindowsCE
                endPoint = new IPEndPoint(m_targetSearch, 3040);
                m_broadcast.SendTo(buff, endPoint);
#else
                if (m_targetSearch.Address != IPAddress.Broadcast.Address)
                {
                    endPoint = new IPEndPoint(m_targetSearch, 3040);

                    //byte[] buff = new byte[] { 0, 0, 0, 0 };

                    //buff[2] = (byte)(m_rand >> 8);
                    //buff[3] = (byte)(m_rand & 0x00FF);

                    m_broadcast.SendTo(buff, endPoint);
                }
                else
                {
                    foreach (System.Net.NetworkInformation.NetworkInterface f in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (f.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet || f.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211)
                        {
                            if (f.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up)
                                continue;

                            System.Net.NetworkInformation.IPInterfaceProperties ipInterface = f.GetIPProperties();

                            foreach (System.Net.NetworkInformation.UnicastIPAddressInformation unicastAddress in ipInterface.UnicastAddresses)
                            {
                                if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    byte[] complementedMaskBytes = new byte[4];
                                    byte[] broadcastIPBytes = new byte[4];

                                    for (int i = 0; i < 4; i++)
                                    {
                                        complementedMaskBytes[i] = (byte)~(unicastAddress.IPv4Mask.GetAddressBytes()[i]);
                                        broadcastIPBytes[i] = (byte)((unicastAddress.Address.GetAddressBytes()[i]) | complementedMaskBytes[i]);
                                    }

                                    endPoint = new IPEndPoint(new IPAddress(broadcastIPBytes), 3040);
                                    m_broadcast.SendTo(buff, endPoint);
                                }
                            }
                        }
                    }
                }
#endif
            }
            catch (System.Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
        
        private void SendBroadcast()
        {
            Random rand = new Random();
            byte[] buff = new byte[] { 0, 0, 0, 0 };

            m_rand = rand.Next(1, 0x7fff); // rand returns a number between 0 and 0x7FFF

            buff[2] = (byte)(m_rand >> 8);
            buff[3] = (byte)(m_rand & 0x00FF);

            SendAllBroadcast(buff);

            //Bug :: move here to aviod exception throw by system
            Operation = RecvOperation.SEARCH;
        }

        private void ResendBroadcast()
        {
            //IPEndPoint endPoint = new IPEndPoint(m_targetSearch, 3040);
            //IPEndPoint endPoint;

            Random rand = new Random();
            byte[] buff = new byte[] { 0, 0, 0, 0 };

            m_rand = rand.Next(1, 0x7fff); // rand returns a number between 0 and 0x7FFF

            buff[2] = (byte)(m_rand >> 8);
            buff[3] = (byte)(m_rand & 0x00FF);

            SendAllBroadcast(buff);
        }

        private Result SendAssignment(Byte[] targetIPAddress, Byte[] changedIPAddress, Byte[] macAddress, string deviceName, bool dhcpEnabled, byte dhcpRetry, Byte[] trustedServerAddress, bool trustedServerEnabled, Byte[] subnet, byte[] gateway, int gatewayCheckReset)
        {
            SaveBeforeSuccess(changedIPAddress, macAddress, deviceName, dhcpEnabled, dhcpRetry, trustedServerAddress, trustedServerEnabled, subnet, gateway, gatewayCheckReset);
            byte[] m_assignment_buff = BuildAssignBuffer(changedIPAddress, deviceName, dhcpEnabled, macAddress, dhcpRetry, trustedServerAddress, trustedServerEnabled, subnet, gateway, gatewayCheckReset);

            if (m_assignment_buff == null)
                return Result.BUILD_ASSIGN_BUFFER_FAIL;

            //-----------------------------------------------------------------
            // Send the packet directly to the device's MAC address (use new IP address)
            //-----------------------------------------------------------------
            IPEndPoint endPoint = new IPEndPoint(new IPAddress(targetIPAddress), 3040);

            try
            {
                m_timeout.Change(u_timeout/*System.Threading.Timeout.Infinite*/, System.Threading.Timeout.Infinite);

                m_broadcast.SendTo(m_assignment_buff, endPoint);
            }
            catch (System.Exception e)
            {
                m_timeout.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                Debug.WriteLine(e.Message);

                return Result.SEND_ASSIGN_FAIL;
            }
            return Result.OK;
        }

        private void SaveBeforeSuccess(Byte[] changedIP, Byte[] macAddress, string deviceName, bool dhcpEnabled, byte dhcpRetry, Byte[] trustedServerAddress, bool trustedServerEnabled, Byte[] subnet, Byte[] gateway, int gatewayCheckResetMode)
        {
            assignInfo.DeviceName = deviceName;
            assignInfo.IPAddress.Address = (Byte[])changedIP.Clone();
            assignInfo.MACAddress.Address = (Byte[])macAddress.Clone();
            assignInfo.DHCPEnabled = dhcpEnabled;
            assignInfo.DHCPRetry = dhcpRetry;
            assignInfo.TrustedAddress.Address = (Byte[])trustedServerAddress.Clone();
            assignInfo.TrustedEnabled = trustedServerEnabled;
            assignInfo.SubnetMask.Address = (Byte[])subnet.Clone();
            assignInfo.Gateway.Address = (Byte[])gateway.Clone();
            assignInfo.GatewayCheckResetMode = gatewayCheckResetMode;
        }

        private void SaveAfterSuccess()
        {
            int index = m_devices.FindIndex(delegate(DeviceInfomation info) { return info.MACAddress.Equals(assignInfo.MACAddress); });
            if (index >= 0)
            {
                m_devices[index].DeviceName = assignInfo.DeviceName;
                m_devices[index].DHCPEnabled = assignInfo.DHCPEnabled;
                m_devices[index].DHCPRetry = assignInfo.DHCPRetry;
                m_devices[index].IPAddress.Address = (Byte[])assignInfo.IPAddress.Address.Clone();
                m_devices[index].MACAddress.Address = (Byte[])assignInfo.MACAddress.Address.Clone();
                m_devices[index].TrustedServer.Address = (Byte[])assignInfo.TrustedAddress.Address.Clone();
                m_devices[index].TrustedServerEnabled = assignInfo.TrustedEnabled;
                m_devices[index].SubnetMask.Address = (Byte[])assignInfo.SubnetMask.Address.Clone();
                m_devices[index].Gateway.Address = (Byte[])assignInfo.Gateway.Address.Clone();
                m_devices[index].GatewayCheckResetMode = assignInfo.GatewayCheckResetMode;
            }
        }

        private byte[] BuildAssignBuffer(byte[] CS203IP, string deviceName, bool DHCP_ENABLE, byte[] MAC, byte DHCP_RETRY, byte[] TRUSTED_IP, bool TRUSTED_ENABLE, byte[] subnet, byte[] gateway, int gatewayCheckReset)
        {
            //Check Input Value
            if (CS203IP == null || CS203IP.Length != 4 ||
                MAC == null || MAC.Length != 6 || 
                TRUSTED_IP == null || TRUSTED_IP.Length != 4 ||
                subnet == null || subnet.Length !=4 || 
                gateway ==null || gateway.Length != 4)
                return null;

            byte[] IP = new byte[4]; // contains the old IP address
            byte[] buf = new byte[17];
            byte[] m_assignment_buff;
            
            if (gatewayCheckReset < 0) // if not valid
                m_assignment_buff = new byte[64];
            else
                m_assignment_buff = new byte[65];

            int i;
            Random rand = new Random();

            //-----------------------------------------------------------------
            // Fill buffer with Assignment Packet
            //-----------------------------------------------------------------
            //
            Array.Clear(m_assignment_buff, 0, m_assignment_buff.Length);

            m_rand = rand.Next(1, 0x7fff); // rand returns a number between 0 and 0x7FFF

            Debug.WriteLine(string.Format("BuildAssignBuffer:Random {0:x}", m_rand));

            m_assignment_buff[0] = 0x02;
            m_assignment_buff[1] = 0x00;
            m_assignment_buff[2] = (byte)(m_rand >> 8);
            m_assignment_buff[3] = (byte)(m_rand & 0x00FF);


            // Get IP Address, Subnet, and Gateway
            Array.Copy(CS203IP, 0, m_assignment_buff, 4, 4);    //m_assignment_buff[4] ~ [7]
            Array.Copy(TRUSTED_IP, 0, m_assignment_buff, 8, 4);   //m_assignment_buff[8] ~ [11]


            m_assignment_buff[12] = 0;  //reserved
            m_assignment_buff[13] = 0;  //reserved

            m_assignment_buff[14] = (byte)(DHCP_RETRY & 0xff);

            m_assignment_buff[15] = (byte)(DHCP_ENABLE ? 0x0 : 0x1);

            //MAC address (buff 16 - 21)
            Array.Copy(MAC, 0, m_assignment_buff, 16, 6);    //m_assignment_buff[16] ~ [21]

            m_assignment_buff[22] = (byte)(TRUSTED_ENABLE ? 0x1 : 0x0);
            m_assignment_buff[23] = 0;  //reserved
            
            System.Text.ASCIIEncoding en = new ASCIIEncoding();
            Byte[] bytes = en.GetBytes(deviceName);

            Array.Copy(bytes, 0, m_assignment_buff, 24, bytes.Length);    //m_assignment_buff[16] ~ [21]

            m_assignment_buff[55] = (byte)'\0';

            /* new added */
            // Set Subnet, and Gateway
            Array.Copy(subnet, 0, m_assignment_buff, 56, 4);    //m_assignment_buff[56] ~ [59]
            Array.Copy(gateway, 0, m_assignment_buff, 60, 4);   //m_assignment_buff[60] ~ [63]

            if (gatewayCheckReset >= 0)
                m_assignment_buff[64] = (byte)gatewayCheckReset;

            return m_assignment_buff;
        }

        private bool ReceivePacketFromSearch()
        {

            byte[] buffer = new byte[4096];
            int num_bytes = 0;

            //---------------------------------------------------------------
            // Receive Packet from Buffer
            //---------------------------------------------------------------

            // IP and Port of sender.
            IPEndPoint sender = new IPEndPoint(IPAddress.Broadcast, 0);
            EndPoint senderRemote = (EndPoint)sender;

            try
            {
                num_bytes = m_broadcast.ReceiveFrom(buffer, ref senderRemote);
            }
            catch (System.Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            // Check for errors
            if (num_bytes >= 10000)
            {

                // Handle Error
                Debug.WriteLine("Error receiving data");
                return false;

            }
            else
            {

                // Handle Error
                Debug.WriteLine("Received Packet");
            }

            //---------------------------------------------------------------
            // Verify Packet
            //---------------------------------------------------------------
            //
            //	Check minimum packet size
            //  Check packet type field (must be 0x01)
            //	Verify random number
            //
            //
            if ((num_bytes >= 32) && (buffer[0] == 0x01) && (buffer[2] == (m_rand >> 8))
                                                         && (buffer[3] == (m_rand & 0x00FF))
              )
            {
                //---------------------------------------------------------------
                // Check if entry already exists
                //---------------------------------------------------------------
                byte[] mac = new byte[6];
                byte[] table_mac = new byte[6];

                // Fill <mac> with the mac address from the packet

                //memcpy(mac, &buffer[14], 6);
                Buffer.BlockCopy(buffer, 14, mac, 0, 6);

                // Check the mac address of each cell in the table
                int index = m_devices.FindIndex(delegate(DeviceInfomation info) { return info.MACAddress.Equals(mac); });
                if (index >= 0) return false;


                //---------------------------------------------------------------
                // Add Entry
                //---------------------------------------------------------------

                DeviceInfomation entry = new DeviceInfomation();

                entry.Mode = Enum.IsDefined(typeof(Mode), (int)buffer[1]) ? (Mode)buffer[1] : Mode.Unknown;
                entry.Port = (ushort)((IPEndPoint)senderRemote).Port;// destPort;

                int i = 4; // Start buffer index at 4

                entry.TimeElapsedPowerOn.days = ((uint)buffer[i++] << 8);
                entry.TimeElapsedPowerOn.days |= buffer[i++];

                entry.TimeElapsedPowerOn.hours = buffer[i++];
                entry.TimeElapsedPowerOn.minutes = buffer[i++];


                entry.TimeElapsedNetwork.days = ((uint)buffer[i++] << 8);
                entry.TimeElapsedNetwork.days |= buffer[i++];

                entry.TimeElapsedNetwork.hours = buffer[i++];
                entry.TimeElapsedNetwork.minutes = buffer[i++];

                entry.TimeElapsedPowerOn.seconds = buffer[i++];
                entry.TimeElapsedNetwork.seconds = buffer[i++];

                entry.MACAddress.Address[0] = buffer[i++];
                entry.MACAddress.Address[1] = buffer[i++];
                entry.MACAddress.Address[2] = buffer[i++];
                entry.MACAddress.Address[3] = buffer[i++];
                entry.MACAddress.Address[4] = buffer[i++];
                entry.MACAddress.Address[5] = buffer[i++];

                entry.IPAddress.Address[0] = buffer[i++];
                entry.IPAddress.Address[1] = buffer[i++];
                entry.IPAddress.Address[2] = buffer[i++];
                entry.IPAddress.Address[3] = buffer[i++];

                entry.TrustedServer.Address[0] = buffer[i++];
                entry.TrustedServer.Address[1] = buffer[i++];
                entry.TrustedServer.Address[2] = buffer[i++];
                entry.TrustedServer.Address[3] = buffer[i++];
                entry.TrustedServerEnabled = buffer[i++] != 0;

#if true
                entry.ConnectMode = buffer[i++];
                bool supportGateway = true;
#else
                //Skip 1 byte
                bool supportGateway = (buffer[i++] == 0x2);
#endif
                //entry.serverport = (ushort)(buffer[i++] << 8 | buffer[i++]);
                //entry.serverport[1] = buffer[i++];
                entry.DHCPRetry = buffer[i++];
                //entry.gateway[3] = buffer[i++];
                entry.DHCPEnabled = (buffer[i++] == 0);

                if (supportGateway)
                {
                    entry.SubnetMask.Address[0] = buffer[i++];
                    entry.SubnetMask.Address[1] = buffer[i++];
                    entry.SubnetMask.Address[2] = buffer[i++];
                    entry.SubnetMask.Address[3] = buffer[i++];

                    entry.Gateway.Address[0] = buffer[i++];
                    entry.Gateway.Address[1] = buffer[i++];
                    entry.Gateway.Address[2] = buffer[i++];
                    entry.Gateway.Address[3] = buffer[i++];
                }

                byte[] temp = new byte[buffer.Length - i];
                Buffer.BlockCopy(buffer, i, temp, 0, temp.Length);

                System.Text.Encoding enc = System.Text.Encoding.ASCII;

                string myString = enc.GetString(temp, 0, buffer.Length - i);

                string[] str = myString.Split(new char[] { '\0' });

                entry.DeviceName = str[0];

                entry.Description = str[1];

                entry.TimeElapsedPowerOn.name = str[2];

                entry.TimeElapsedNetwork.name = str[3];

                if (buffer[num_bytes - 2] == 00)
                {
                    if (buffer[num_bytes - 1] == 1)
                        entry.GatewayCheckResetMode = 1;
                    else
                        entry.GatewayCheckResetMode = 0;
                }
                else
                    entry.GatewayCheckResetMode = -1;

                // Add the entry

                Debug.WriteLine(String.Format("add device listdevice_name {0}", entry.DeviceName));

                m_devices.Add(entry);

                RaiseEvent<DeviceFinderArgs>(OnSearchCompleted, this, new DeviceFinderArgs(entry));

                return true;
            }
            else if ((buffer[0] == 0x00) && (buffer[2] == (m_rand >> 8))
                                       && (buffer[3] == (m_rand & 0x00FF)))
            {
                // Discard Packet
                // We have received an identity request from another host.
                // This is a very rare case in which the PC randomly chooses port 3040 
                // for the netfinder app and the same random number is chosen.
                //return ;
            }
            else
            {
                Debug.WriteLine("Invalid Search Packet Received or Random Number Mismatch");
                return false;
            }
            return false;
        }

        private bool ReceivePacketFromAssign()
        {
            byte[] buffer = new byte[0xb9];
            int num_bytes;

            //---------------------------------------------------------------
            // Receive Packet from Buffer
            //---------------------------------------------------------------

#if !old
            
            // IP and Port of sender.
            IPEndPoint sender = new IPEndPoint(IPAddress.Broadcast, 0);
            EndPoint senderRemote = (EndPoint)sender;

            try
            {
                num_bytes = m_broadcast.ReceiveFrom(buffer, ref senderRemote);
            }
            catch (System.Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }

#else
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 3040);
            EndPoint senderRemote = (EndPoint)sender;
            UdpClient uclinet = new UdpClient(3040);
            
            buffer = uclinet.Receive(ref sender);
            num_bytes = buffer.Length;

#endif
            // Check for errors
            if (num_bytes >= 10000)
            {

                // Handle Error
                Debug.WriteLine("Error receiving data");
                return false;

            }
            else
            {

                // Handle Error
                Debug.WriteLine("Received Packet");

            }

            //---------------------------------------------------------------
            // Verify Packet
            //---------------------------------------------------------------
            bool fromSameTarget = memcmp(assignInfo.IPAddress.Address , ((IPEndPoint)senderRemote).Address.GetAddressBytes(), 4) == 0;
            Debug.WriteLine(String.Format("Recevice from Assign@IP source ={0}, checksum = {1:x} {2}", senderRemote, buffer[2] << 8 | buffer[3], (buffer[2] == (m_rand >> 8)) && (buffer[3] == (m_rand & 0x00FF))));
            if ((num_bytes == 4) && (buffer[0] == 0x03) &&
                (buffer[2] == (m_rand >> 8)) && (buffer[3] == (m_rand & 0x00FF)) &&
                fromSameTarget)
            {

                Operation = RecvOperation.IDLE;

                // Decode ACK type
                switch (buffer[1])
                {
                    case 0x01: // Programming Sucessful

                        lock (m_lock)
                        {
                            m_timeout.Change(System.Threading.Timeout.Infinite, Timeout.Infinite);
                        }
                        /* Update change to curent list*/
                        SaveAfterSuccess();

                        Debug.WriteLine("Assign Sucess.");
                        RaiseEvent<ResultArgs>(OnAssignCompleted, this, new ResultArgs(AssignResult.ACCEPTED));
                        break;
                    case 0x00: // Address Rejected due to mismatched MAC address

                        lock (m_lock)
                        {
                            m_timeout.Change(System.Threading.Timeout.Infinite, Timeout.Infinite);
                        }
                        Debug.WriteLine("Assign Fail.");
                        RaiseEvent<ResultArgs>(OnAssignCompleted, this, new ResultArgs(AssignResult.REJECTED));
                        break;
                    case 0xFF: // Unknown Error

                        lock (m_lock)
                        {
                            m_timeout.Change(System.Threading.Timeout.Infinite, Timeout.Infinite);
                        }
                        Debug.WriteLine("Unknown Error");
                        RaiseEvent<ResultArgs>(OnAssignCompleted, this, new ResultArgs(AssignResult.UNKNOWN));
                        break;
                }

            }
            else
            {
                Debug.WriteLine("Invalid Assign Packet Received");
                return false;
            }
            return false;
        }

        #region AsyncUpdateEboot
        /// <summary>
        /// Async update Eboot
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="ebootfile"></param>
        public void AsyncUpdateEboot(String IP, String ebootfile)
        {
            IpClass ips = new IpClass();
            ips.IP = IP;
            ips.ebootfile = ebootfile;
            ips.Port = 1515;

            bWorkerEboot.RunWorkerAsync(ips);
        }

        void bWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RaiseEvent<UpdateResultArgs>(OnUpdateCompleted, this, new UpdateResultArgs((bool)e.Result ? UpdateResult.SUCCESS : UpdateResult.FAIL));
        }

        void bWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //RaiseEvent<UpdatePercentArgs>(OnUpdatePercent, this, new UpdatePercentArgs(e.ProgressPercentage));
            //Threadsafe event trigger
            EventHandler<UpdatePercentArgs> updateEvt;
            lock (m_lock)
            {
                updateEvt = OnUpdatePercent;
            }
            if (updateEvt != null)
            {
                updateEvt(null, new UpdatePercentArgs(e.ProgressPercentage));
            }

        }

        void bWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;

            if (worker == null)
            {
                LastError = "BackgroundWorker equal to NULL";
                return;
            }

            IpClass ipclass = (IpClass)e.Argument;

            bool success = false;
            int totalpercent = 0;
            byte[] Mode1 = new byte[] { (byte)'E', (byte)'B', (byte)'O', (byte)'O', (byte)'T', (byte)'U', (byte)'P', (byte)'G', (byte)'R', (byte)'A', (byte)'D', (byte)'E' };
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ipclass.IP), ipclass.Port);
            FileStream fs = null;
            try
            {

                if (Operation != RecvOperation.IDLE)
                    throw new Exception("Please stop other operation first");

                Operation = RecvOperation.UPDATE;

                m_tcpsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                m_tcpsocket.Connect(endpoint);
                m_tcpsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 0);
                //m_tcpsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 2000);


        //private void TCP_SetKeepAlive(Socket s, ulong keepalive_time, ulong keepalive_interval)
        {
            Socket s = m_tcpsocket;
            ulong keepalive_time = 500U;
            ulong keepalive_interval = 500U;

            ulong[] input_params = new ulong[3];
            byte[] keep_alive = new byte[12];

            if (keepalive_time == 0 || keepalive_interval == 0)
                input_params[0] = 0;
            else
                input_params[0] = 1;
            input_params[1] = keepalive_time;
            input_params[2] = keepalive_interval;

            for (int cnt = 0; cnt < input_params.Length; cnt++)
            {
                keep_alive[cnt * 4 + 3] = (byte)(input_params[cnt] >> 24 & 0xff);
                keep_alive[cnt * 4 + 2] = (byte)(input_params[cnt] >> 16 & 0xff);
                keep_alive[cnt * 4 + 1] = (byte)(input_params[cnt] >> 8 & 0xff);
                keep_alive[cnt * 4 + 0] = (byte)(input_params[cnt] & 0xff);
            }

#if WIN32
            s.IOControl(IOControlCode.KeepAliveValues, keep_alive, null);
#elif WindowsCE
            const int SIO_KEEPALIVE_VALS = -1744830460; // 0x98000004;
            s.IOControl(SIO_KEEPALIVE_VALS, keep_alive, null);
#endif
        }







                m_tcpsocket.Blocking = true;

                // open the source file for reading
                fs = new FileStream(ipclass.ebootfile, FileMode.Open);
                //start eboot update
                byte[] OutBuf = new byte[132];
                byte[] InBuf = new byte[128];
                int block = 0;
                //Trigger Eboot update mode
                m_tcpsocket.SendTo(Mode1, endpoint);
                //Trigger Eboot update mode check?
                if (DecodeAck(block, endpoint))
                {
                    fs.Seek(0x800, SeekOrigin.Begin);
                    for (block = 1; block < 113; block++)
                    {

                        fs.Read(OutBuf, 4, 128);

                        OutBuf[0] = 0;
                        OutBuf[1] = (byte)EBOOT.DATA;
                        OutBuf[2] = 0;
                        OutBuf[3] = (byte)block;

                        m_tcpsocket.SendTo(OutBuf, endpoint);

                        if (!DecodeAck(block, endpoint))
                        {
                            throw new Exception(LastError);
                        }

                        totalpercent = (block * 100 / 112);

                        worker.ReportProgress(totalpercent);

                        //Thread.Sleep(1);//Sleep 1 milsec to let it update progress

                        Debug.WriteLine("Total percent = " + totalpercent.ToString());
                    }

                    //byte[] done = new byte[4];

                    //m_tcpsocket.SendTo(done, endpoint);// send a dummy packet to finsih
                    LastError = "Update Success";
                    success = true;
                    //m_tcpsocket.SendTo(Mode2, endpoint);
                }
                else
                {
                    throw new Exception(LastError);
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }

            if (m_tcpsocket != null)
            {
                m_tcpsocket.Close();
                m_tcpsocket = null;
            }

            if (fs != null)
            {
                fs.Close();
                fs = null;
            }
            e.Result = success;
            Operation = RecvOperation.IDLE;
        }

        struct IpClass
        {
            public String IP;
            public Int32 Port;
            public String ebootfile;
        }

        private bool DecodeAck(int block, IPEndPoint ep)
        {
            bool ret = false;
            EBOOT type = EBOOT.ACK;
            EBOOT_ERR code = EBOOT_ERR.NOT_DEFINED; ;
            byte[] ch = new byte[4];
            int num_bytes;

            EndPoint end = ep as EndPoint;

            num_bytes = m_tcpsocket.ReceiveFrom(ch, ref end);

            type = (EBOOT)ch[1];
            code = (EBOOT_ERR)(ch[3]);

            if (type == EBOOT.ACK)
            {
                if ((int)ch[3] == block)
                    ret = true;
            }
            else if (type == EBOOT.ERROR)
            {
                switch (code)
                {
                    case EBOOT_ERR.NOT_DEFINED:
                        LastError = ("Error: NOT DEFINED");
                        break;
                    case EBOOT_ERR.INVALID_OPCODE:
                        LastError = ("Error: INVALID OPCODE");
                        break;
                    case EBOOT_ERR.INVALID_BLOCK_NUMBER:
                        LastError = String.Format("Error: INVALID BLOCK NUMBER {0}", block);
                        break;
                    case EBOOT_ERR.INVALID_BLOCK_LENGTH:
                        LastError = ("Error: INVALID BLOCK LENGTH");
                        break;
                    case EBOOT_ERR.FLASH_ERROR:
                        LastError = ("Error: FLASH ERROR");
                        break;
                    case EBOOT_ERR.INVALID_PACKET_SIZE:
                        LastError = ("Error: INVALID PACKET SIZE");
                        break;
                    default:
                        LastError = ("Error: UNKNOW");
                        break;
                }
            }

            return ret;
        }
        #endregion

        #region Update Image
        struct UImage
        {
            public String ip;
            public String image;
        }
        void bWorkerImage_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RaiseEvent<UpdateResultArgs>(OnUpdateCompleted, this, new UpdateResultArgs((bool)e.Result ? UpdateResult.SUCCESS : UpdateResult.FAIL));
        }

        void bWorkerImage_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //RaiseEvent<UpdatePercentArgs>(OnUpdatePercent, null, new UpdatePercentArgs(e.ProgressPercentage));
            //Threadsafe event trigger
            EventHandler<UpdatePercentArgs> updateEvt;
            lock (m_lock)
            {
                updateEvt = OnUpdatePercent;
            }
            if (updateEvt != null)
            {
                updateEvt(null, new UpdatePercentArgs(e.ProgressPercentage));
            }
        }

        void bWorkerImage_DoWork(object sender, DoWorkEventArgs e)
        {
            UImage image = (UImage)e.Argument;
            Put(sender, e, image.ip, 69, "boot.img", image.image, Modes.Octet);
        }
        /// <summary>
        /// Async update image
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="imagefile"></param>
        public void AsyncUpdateImage(String IP, String imagefile)
        {
            UImage image;
            image.ip = IP;
            image.image = imagefile;
            bWorkerImage.RunWorkerAsync(image);
        }

        private void Put(object sender, DoWorkEventArgs e, string tftpServer, int tftpPort, string remoteFile, string localFile, Modes tftpMode)
        {
            bool success = false;
            BinaryReader fileStream = null;
            Socket tftpSocket = null;
            try
            {
                if (Operation != RecvOperation.IDLE)
                    throw new Exception("Please stop other operation first");

                Operation = RecvOperation.UPDATE;

                BackgroundWorker worker = sender as BackgroundWorker;

                long totalpercent = 0, prevpercent = -1, totalsent = 0;
                int len = 0;
                int packetNr = 0;
                byte[] sndBuffer = CreateRequestPacket(Opcodes.Write, remoteFile, tftpMode);
                byte[] rcvBuffer = new byte[516];

                fileStream = new BinaryReader(new FileStream(localFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                //IPHostEntry hostEntry = Dns.GetHostEntry(tftpServer);
                IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(tftpServer)/*hostEntry.AddressList[0]*/, tftpPort);
                EndPoint dataEP = (EndPoint)serverEP;

                worker.ReportProgress(0);
                
                tftpSocket = new Socket(serverEP.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
#if WIN32                
                tftpSocket.ReceiveTimeout  = 5000;
                tftpSocket.SendTimeout = 5000;
#endif                
                //tftpSocket.SetSocketOption(SocketOptionLevel.Udp, SocketOptionName.SendBuffer, 0);
                // Request Writing to TFTP Server
                tftpSocket.SendTo(sndBuffer, sndBuffer.Length, SocketFlags.None, serverEP);
                //tftpSocket.SendTo(sndBuffer, dataEP);
                //tftpSocket.SetSocketOption(SocketOptionLevel.Udp, SocketOptionName.ReceiveTimeout, 1000);
                len = tftpSocket.ReceiveFrom(rcvBuffer, ref dataEP);

                // keep track of the TID 
                serverEP.Port = ((IPEndPoint)dataEP).Port;

                while (true)
                {
                    // handle any kind of error 
                    if (((Opcodes)rcvBuffer[1]) == Opcodes.Error)
                    {
                        fileStream.Close();
                        tftpSocket.Close();
                        throw new TFTPException(((rcvBuffer[2] << 8) & 0xff00) | rcvBuffer[3], Encoding.ASCII.GetString(rcvBuffer, 4, rcvBuffer.Length - 5).Trim('\0'));
                    }

                    // expect the next packet ack
                    if ((((Opcodes)rcvBuffer[1]) == Opcodes.Ack) && (((rcvBuffer[2] << 8) & 0xff00) | rcvBuffer[3]) == packetNr)
                    {
                        int retry = 0;

                        sndBuffer = CreateDataPacket(++packetNr, fileStream.ReadBytes(512));

                    tftpack:
                        tftpSocket.SendTo(sndBuffer, sndBuffer.Length, SocketFlags.None, serverEP);

                        try
                        {
                            len = 0;
                            len = tftpSocket.ReceiveFrom(rcvBuffer, ref dataEP);
                        }
                        catch (Exception ex)
                        {
                        }

                        if (len == 0)
                        {
                            if (retry++ < 10)
                                goto tftpack;
                            else
                                throw new TFTPException(0xffff, "Packet Lost");
                        }
                        totalsent += 512;
                    }

                    totalpercent = (totalsent * 100 / fileStream.BaseStream.Length);

                    if (totalpercent != prevpercent)
                    {
                        worker.ReportProgress((int)totalpercent);
                        //Thread.Sleep(1);//Sleep 1 milsec to let it update progress
                        prevpercent = totalpercent;
                    }

                    Debug.WriteLine("Total percent = " + totalpercent.ToString());
                    // we are done
                    if (sndBuffer.Length < 516)
                    {
                        break;
                    }

/*                    
                    else
                    {
                        len = tftpSocket.ReceiveFrom(rcvBuffer, ref dataEP);

                        
                        int cnt = 0;

                    tftpack:
                        try
                        {
                        }
                        catch (Exception ex)
                        {
                        }

                        if (len == 0)
                        {
                            if (cnt++ < 10)
                            {
                                tftpSocket.SendTo(sndBuffer, sndBuffer.Length, SocketFlags.None, serverEP);
                                goto tftpack;
                            }
                            else
                            {
                                throw new TFTPException(0xffff, "Packet Lost");
                            }
                        }
                    }
 */
                }

                success = true;

            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }
            if (tftpSocket != null)
            {
                tftpSocket.Close();
            }
            if (fileStream != null)
            {
                fileStream.Close();
            }
            e.Result = success;
            Operation = RecvOperation.IDLE;
        }

#if SyncMode
        /// <summary>
        /// Get API Mode
        /// </summary>
        public ApiMode GetApiMode (string IP)
        {
            byte[] CMDBuf = new byte[10];
            UdpClient UdpCMD = new UdpClient();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), 3041);
#if WIN32
            IPAddress hostIP = Dns.GetHostAddresses(IP)[0];
#elif WindowsCE
            IPAddress hostIP = Dns.GetHostEntry(IP).AddressList[0];
#else
#error Not Support This Paltform
#endif
            IPAddress hostIP = System.Net.IPAddress.Parse(IP);
            
            CMDBuf[0] = 0x80;
            CMDBuf[1] = (byte)(hostIP.Address & 0xff);
            CMDBuf[2] = (byte)((hostIP.Address >> 8) & 0xff);
            CMDBuf[3] = (byte)((hostIP.Address >> 16) & 0xff);
            CMDBuf[4] = (byte)((hostIP.Address >> 24) & 0xff);
            CMDBuf[5] = 0x01;
            CMDBuf[6] = 0x0e;
            UdpCMD.Send(CMDBuf, 7, RemoteIpEndPoint);

            Byte[] receiveBytes = UdpCMD.Receive(ref RemoteIpEndPoint);

            if (receiveBytes[4]  == 1)
                return ApiMode.LOWLEVEL;
            else
                return ApiMode.HIGHLEVEL;
        }

        
        /// <summary>
        /// Set Device Mode (mode 0 = high, 1 = low)
        /// </summary>
        public bool SetApiMode (string IP, ApiMode Mode)
        {
            byte[] CMDBuf = new byte[10];
            UdpClient UdpCMD = new UdpClient();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), 3041);
/*
#if WIN32
            IPAddress hostIP = Dns.GetHostAddresses(IP)[0];
#elif WindowsCE
            IPAddress hostIP = Dns.GetHostEntry(IP).AddressList[0];
#else
#error Not Support This Paltform
#endif
*/
            IPAddress hostIP = System.Net.IPAddress.Parse(IP);

            CMDBuf[0] = 0x80;
            CMDBuf[1] = (byte)(hostIP.Address & 0xff);
            CMDBuf[2] = (byte)((hostIP.Address >> 8) & 0xff);
            CMDBuf[3] = (byte)((hostIP.Address >> 16) & 0xff);
            CMDBuf[4] = (byte)((hostIP.Address >> 24) & 0xff);
            CMDBuf[5] = 0x01;
            if (Mode == ApiMode.HIGHLEVEL)
                CMDBuf[6] = 0x0d;
            else
                CMDBuf[6] = 0x0c;
            UdpCMD.Send(CMDBuf, 7, RemoteIpEndPoint);

            Byte[] receiveBytes = UdpCMD.Receive(ref RemoteIpEndPoint);

            Thread.Sleep(2000); // delay for reset machine

            return true;
        }

        public bool GetApiMode(string IP, out ApiMode Mode)
        {
            byte[] CMDBuf = new byte[10];
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), 3041);
            UdpClient UdpCMD = new UdpClient();
            DateTime timeout;

            IPAddress hostIP = System.Net.IPAddress.Parse(IP);
            CMDBuf[0] = 0x80;
            CMDBuf[1] = (byte)(hostIP.Address & 0xff);
            CMDBuf[2] = (byte)((hostIP.Address >> 8) & 0xff);
            CMDBuf[3] = (byte)((hostIP.Address >> 16) & 0xff);
            CMDBuf[4] = (byte)((hostIP.Address >> 24) & 0xff);
            CMDBuf[5] = 0x01;
            CMDBuf[6] = 0x0e;

            UdpState s = new UdpState();
            s.e = RemoteIpEndPoint;
            s.u = UdpCMD;
            APIModeMessageReceived = false;

            timeout = DateTime.Now.AddSeconds(1);
            UdpCMD.Send(CMDBuf, 7, RemoteIpEndPoint);
            //IAsyncResult AsyncResult= UdpCMD.BeginReceive(new AsyncCallback(GetApiModeCallBack), s);
            IAsyncResult AsyncResult = UdpCMD.BeginReceive(null, null);
            do
            {
                Thread.Sleep(100);

                if (AsyncResult.IsCompleted)
                {
                    APIModeMessage = UdpCMD.EndReceive(AsyncResult, ref RemoteIpEndPoint);
                    if (APIModeMessage[4] == 1)
                        Mode = ApiMode.LOWLEVEL;
                    else
                        Mode = ApiMode.HIGHLEVEL;

                    return true;
                }
            } while (DateTime.Now < timeout);
            
            Mode = ApiMode.UNKNOWN;
            return false;
        }

        class UdpState
        {
            public IPEndPoint e ;
            public UdpClient u ;
        }

        private static bool APIModeMessageReceived = false;
        Byte[] APIModeMessage;
        void GetApiModeCallBack (IAsyncResult ar)
        {
            UdpClient u = (UdpClient)((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;

            //APIModeMessage = u.EndReceive(ar, ref e);
            //APIModeMessageReceived = true;
        }
#endif

        /// <summary>
        /// Get API Mode
        /// </summary>
        public ApiMode GetApiMode (string IP)
        {
            byte[] CMDBuf = new byte[10];
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(IP), 3041);
            UdpClient UdpCMD = new UdpClient();
            DateTime timeout;

            Byte[] APIModeMessage = new byte[10];

            IPAddress hostIP = System.Net.IPAddress.Parse(IP);
            CMDBuf[0] = 0x80;
            CMDBuf[1] = (byte)(hostIP.Address & 0xff);
            CMDBuf[2] = (byte)((hostIP.Address >> 8) & 0xff);
            CMDBuf[3] = (byte)((hostIP.Address >> 16) & 0xff);
            CMDBuf[4] = (byte)((hostIP.Address >> 24) & 0xff);
            CMDBuf[5] = 0x01;
            CMDBuf[6] = 0x0e;

            timeout = DateTime.Now.AddSeconds (1);
            UdpCMD.Send(CMDBuf, 7, RemoteIpEndPoint);
            //IAsyncResult AsyncResult = UdpCMD.BeginReceive(null, null);

            IAsyncResult AsyncResult = UdpCMD.Client.BeginReceive(APIModeMessage, 0, APIModeMessage.Length, SocketFlags.None, null, null);
            do
            {
                Thread.Sleep(100);

                if (AsyncResult.IsCompleted)
                {
                    //Byte[] APIModeMessage = UdpCMD.Client.EndReceive(AsyncResult, ref RemoteIpEndPoint);
                    int rs = UdpCMD.Client.EndReceive(AsyncResult);

                    if (APIModeMessage[4] == 1)
                        return ApiMode.LOWLEVEL;
                    else
                        return ApiMode.HIGHLEVEL;
                }
            } while (DateTime.Now < timeout);

            return ApiMode.UNKNOWN;
        }
        public bool GetApiMode(string IP, out ApiMode Mode)
        {
            byte[] CMDBuf = new byte[10];
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(IP), 3041);
            UdpClient UdpCMD = new UdpClient();
            DateTime timeout;

            Byte[] APIModeMessage = new byte[10];

            IPAddress hostIP = System.Net.IPAddress.Parse(IP);
            CMDBuf[0] = 0x80;
            CMDBuf[1] = (byte)(hostIP.Address & 0xff);
            CMDBuf[2] = (byte)((hostIP.Address >> 8) & 0xff);
            CMDBuf[3] = (byte)((hostIP.Address >> 16) & 0xff);
            CMDBuf[4] = (byte)((hostIP.Address >> 24) & 0xff);
            CMDBuf[5] = 0x01;
            CMDBuf[6] = 0x0e;

            timeout = DateTime.Now.AddSeconds(1);
            UdpCMD.Send(CMDBuf, 7, RemoteIpEndPoint);
            //IAsyncResult AsyncResult = UdpCMD.BeginReceive(null, null);
            IAsyncResult AsyncResult = UdpCMD.Client.BeginReceive(APIModeMessage, 0, APIModeMessage.Length, SocketFlags.None, null, null);
            do
            {
                Thread.Sleep(100);

                if (AsyncResult.IsCompleted)
                {
                    int rs = UdpCMD.Client.EndReceive(AsyncResult);
                    if (APIModeMessage[4] == 1)
                        Mode = ApiMode.LOWLEVEL;
                    else
                        Mode = ApiMode.HIGHLEVEL;

                    return true;
                }
            } while (DateTime.Now < timeout);
            
            Mode = ApiMode.UNKNOWN;
            return false;
        }

        /// <summary>
        /// Set Device Mode (mode 0 = high, 1 = low)
        /// </summary>
        public bool SetApiMode(string IP, ApiMode Mode)
        {
            byte[] CMDBuf = new byte[10];
            UdpClient UdpCMD = new UdpClient();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(IP), 3041);
            IPAddress hostIP = System.Net.IPAddress.Parse(IP);
            DateTime timeout;

            byte[] SetAPIModeMessage = new byte[10];

            CMDBuf[0] = 0x80;
            CMDBuf[1] = (byte)(hostIP.Address & 0xff);
            CMDBuf[2] = (byte)((hostIP.Address >> 8) & 0xff);
            CMDBuf[3] = (byte)((hostIP.Address >> 16) & 0xff);
            CMDBuf[4] = (byte)((hostIP.Address >> 24) & 0xff);
            CMDBuf[5] = 0x01;
            CMDBuf[6] = (Mode == ApiMode.HIGHLEVEL) ? (byte)0x0d : (byte)0x0c;

            timeout = DateTime.Now.AddSeconds(6);
            UdpCMD.Send(CMDBuf, 7, RemoteIpEndPoint);
            //IAsyncResult AsyncResult = UdpCMD.BeginReceive(null, null);
            IAsyncResult AsyncResult = UdpCMD.Client.BeginReceive(SetAPIModeMessage, 0, SetAPIModeMessage.Length, SocketFlags.None, null, null);
            do
            {
                Thread.Sleep(100);

                if (AsyncResult.IsCompleted)
                {
                    int rs = UdpCMD.Client.EndReceive(AsyncResult);
                    Thread.Sleep(2000); // delay for reset machine
                    return true;
                }
            } while (DateTime.Now < timeout);

            return false;            
        }



        /// <summary>
        /// Get Read Tag GPO Indication Status
        /// </summary>
        public int GetTagIndicationMode(string IP)
        {
            byte[] CMDBuf = new byte[10];
            UdpClient UdpCMD = new UdpClient();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(IP), 3041);
/*
 * #if WIN32
            IPAddress hostIP = Dns.GetHostAddresses(IP)[0];
#elif WindowsCE
            IPAddress hostIP = Dns.GetHostEntry(IP).AddressList[0];
#else
#error Not Support This Paltform
#endif
*/
            IPAddress hostIP = System.Net.IPAddress.Parse(IP);
            CMDBuf[0] = 0x80;
            CMDBuf[1] = (byte)(hostIP.Address & 0xff);
            CMDBuf[2] = (byte)((hostIP.Address >> 8) & 0xff);
            CMDBuf[3] = (byte)((hostIP.Address >> 16) & 0xff);
            CMDBuf[4] = (byte)((hostIP.Address >> 24) & 0xff);
            CMDBuf[5] = 0x01;
            CMDBuf[6] = 0x11;
            UdpCMD.Send(CMDBuf, 7, RemoteIpEndPoint);

            Byte[] receiveBytes = UdpCMD.Receive(ref RemoteIpEndPoint);

            return receiveBytes[4];
        }

        /// <summary>
        /// 
        /// Set Read Tag GPO Indication Status
        /// </summary>
        public bool SetTagIndicationMode(string IP, byte Mode)
        {
            byte[] CMDBuf = new byte[10];
            UdpClient UdpCMD = new UdpClient();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(IP), 3041);
/*#if WIN32
            IPAddress hostIP = Dns.GetHostAddresses(IP)[0];
#elif WindowsCE
            IPAddress hostIP = Dns.GetHostEntry(IP).AddressList[0];
#else
#error Not Support This Paltform
#endif
*/
            IPAddress hostIP = System.Net.IPAddress.Parse(IP);

            if (Mode < 0 || Mode > 2)
                return false;

            CMDBuf[0] = 0x80;
            CMDBuf[1] = (byte)(hostIP.Address & 0xff);
            CMDBuf[2] = (byte)((hostIP.Address >> 8) & 0xff);
            CMDBuf[3] = (byte)((hostIP.Address >> 16) & 0xff);
            CMDBuf[4] = (byte)((hostIP.Address >> 24) & 0xff);
            CMDBuf[5] = 0x01;
            CMDBuf[6] = 0x12;
            CMDBuf[7] = Mode;
            UdpCMD.Send(CMDBuf, 8, RemoteIpEndPoint);

            Byte[] receiveBytes = UdpCMD.Receive(ref RemoteIpEndPoint);

            return true;
        }

        /// <summary>
        /// Get POE Information
        /// </summary>
        public Result GetPoeInfo(string IP, out byte Type, out UInt16 RequestedPower, out UInt16 AllocatedPower)
        {
            byte[] CMDBuf = new byte[10];
            UdpClient UdpCMD = new UdpClient();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(IP), 3041);
            //IPAddress hostIP = Dns.GetHostAddresses(IP)[0];
            IPAddress hostIP = System.Net.IPAddress.Parse(IP);

            CMDBuf[0] = 0x80;
            CMDBuf[1] = (byte)(hostIP.Address & 0xff);
            CMDBuf[2] = (byte)((hostIP.Address >> 8) & 0xff);
            CMDBuf[3] = (byte)((hostIP.Address >> 16) & 0xff);
            CMDBuf[4] = (byte)((hostIP.Address >> 24) & 0xff);
            CMDBuf[5] = 0x01;
            CMDBuf[6] = 0x13;
            UdpCMD.Send(CMDBuf, 7, RemoteIpEndPoint);

            Byte[] receiveBytes = UdpCMD.Receive(ref RemoteIpEndPoint);

            Type = receiveBytes[4];
            RequestedPower = (UInt16)(receiveBytes[5] << 8 | receiveBytes[6]);
            AllocatedPower = (UInt16)(receiveBytes[7] << 8 | receiveBytes[8]);

            return Result.OK;
        }

        /// <summary>
        /// Set POE Request Power 0.1w pre step
        /// </summary>
        public Result SetPoePower(string IP, UInt16 power)
        {
            byte[] CMDBuf = new byte[10];
            UdpClient UdpCMD = new UdpClient();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(IP), 3041);
            //IPAddress hostIP = Dns.GetHostAddresses(IP)[0];
            IPAddress hostIP = System.Net.IPAddress.Parse(IP);

            CMDBuf[0] = 0x80;
            CMDBuf[1] = (byte)(hostIP.Address & 0xff);
            CMDBuf[2] = (byte)((hostIP.Address >> 8) & 0xff);
            CMDBuf[3] = (byte)((hostIP.Address >> 16) & 0xff);
            CMDBuf[4] = (byte)((hostIP.Address >> 24) & 0xff);
            CMDBuf[5] = 0x03;
            CMDBuf[6] = 0x14;
            CMDBuf[7] = (byte)(power >> 8);
            CMDBuf[8] = (byte)(power);
            UdpCMD.Send(CMDBuf, 9, RemoteIpEndPoint);

            Byte[] receiveBytes = UdpCMD.Receive(ref RemoteIpEndPoint);

            return Result.OK;
        }

        #endregion

        #region -=[ Private Member ]=-

        /// <summary>
        /// Creates the request packet.
        /// </summary>
        /// <param name="opCode">The op code.</param>
        /// <param name="remoteFile">The remote file.</param>
        /// <param name="tftpMode">The TFTP mode.</param>
        /// <returns>the ack packet</returns>
        private byte[] CreateRequestPacket(Opcodes opCode, string remoteFile, Modes tftpMode)
        {
            // Create new Byte array to hold Initial 
            // Read Request Packet
            int pos = 0;
            string modeAscii = tftpMode.ToString().ToLower(System.Globalization.CultureInfo.InvariantCulture);
            byte[] ret = new byte[modeAscii.Length + remoteFile.Length + 4];

            // Set first Opcode of packet to indicate
            // if this is a read request or write request
            ret[pos++] = 0;
            ret[pos++] = (byte)opCode;

            // Convert Filename to a char array
            pos += Encoding.ASCII.GetBytes(remoteFile, 0, remoteFile.Length, ret, pos);
            ret[pos++] = 0;
            pos += Encoding.ASCII.GetBytes(modeAscii, 0, modeAscii.Length, ret, pos);
            ret[pos] = 0;

            return ret;
        }

        /// <summary>
        /// Creates the data packet.
        /// </summary>
        /// <param name="blockNr">The packet nr.</param>
        /// <param name="data">The data.</param>
        /// <returns>the data packet</returns>
        private byte[] CreateDataPacket(int blockNr, byte[] data)
        {
            // Create Byte array to hold ack packet
            byte[] ret = new byte[4 + data.Length];

            // Set first Opcode of packet to TFTP_ACK
            ret[0] = 0;
            ret[1] = (byte)Opcodes.Data;
            ret[2] = (byte)((blockNr >> 8) & 0xff);
            ret[3] = (byte)(blockNr & 0xff);
            Array.Copy(data, 0, ret, 4, data.Length);
            return ret;
        }

        /// <summary>
        /// Creates the ack packet.
        /// </summary>
        /// <param name="blockNr">The block nr.</param>
        /// <returns>the ack packet</returns>
        private byte[] CreateAckPacket(int blockNr)
        {
            // Create Byte array to hold ack packet
            byte[] ret = new byte[4];

            // Set first Opcode of packet to TFTP_ACK
            ret[0] = 0;
            ret[1] = (byte)Opcodes.Ack;

            // Insert block number into packet array
            ret[2] = (byte)((blockNr >> 8) & 0xff);
            ret[3] = (byte)(blockNr & 0xff);
            return ret;
        }

        #endregion


#if NOUSE
        private bool ReceivePacketFromConnect()
        {
            byte[] buffer = new byte[4096];
            int num_bytes;

            //---------------------------------------------------------------
            // Receive Packet from Buffer
            //---------------------------------------------------------------

            // IP and Port of sender.
            // IP and Port of sender.
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = (EndPoint)sender;

            try
            {
                num_bytes = broadcast.ReceiveFrom(buffer, ref senderRemote);
            }
            catch (System.Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }

            // Check for errors
            if (num_bytes >= 10000)
            {

                // Handle Error
                Debug.WriteLine("Error receiving data");
                return (l_connected = false);

            }
            else
            {

                // Handle Error
                Debug.WriteLine("Received Packet");

            }

            //---------------------------------------------------------------
            // Verify Packet
            //---------------------------------------------------------------
            if ((num_bytes == 2) && (buffer[0] == 0x04))
            {
                Operation = RecvOperation.IDLE;

                // Decode ACK type
                switch (buffer[1])
                {
                    case 0x01: // Programming Sucessful

                        Debug.WriteLine("Connection Sucess.");

                        lock (m_lock)
                        {
                            m_timeout.Change(Timeout.Infinite, Timeout.Infinite); //Stop timeout here
                            m_connected = true;
                        }
                        RaiseEvent<ResultArgs>(OnConnectCompleted, null, new ResultArgs(Result.ACCEPTED));
                        break;
                    case 0x00: // Address Rejected due to mismatched MAC address

                        Debug.WriteLine("Connection Fail.");

                        lock (m_lock)
                        {
                            m_timeout.Change(Timeout.Infinite, Timeout.Infinite); //Stop timeout here
                        }

                        RaiseEvent<ResultArgs>(OnConnectCompleted, null, new ResultArgs(Result.REJECTED));
                        break;
                    case 0xFF: // Unknown Error

                        Debug.WriteLine("Unknown Error");

                        lock (m_lock)
                        {
                            m_timeout.Change(Timeout.Infinite, Timeout.Infinite); //Stop timeout here
                        }

                        RaiseEvent<ResultArgs>(OnConnectCompleted, null, new ResultArgs(Result.UNKNOWN));
                        break;
                }

            }
            else
            {
                Debug.WriteLine("Invalid Assign Packet Received");
                return (l_connected = false);
            }
            return l_connected;
        }
#endif
        private void RaiseEvent<T>(EventHandler<T> eventHandler, object sender, T e)
            where T : EventArgs
        {
            if (eventHandler != null)
            {
                eventHandler(sender, e);
            }
            return;
/*
#if WindowsCE
            if (eventHandler != null)
            {
                eventHandler(sender, e);
            }
#else
            if (eventHandler != null)
            {
                foreach (Delegate d in eventHandler.GetInvocationList())
                {
                    System.ComponentModel.ISynchronizeInvoke s = d.Target as System.ComponentModel.ISynchronizeInvoke;

                    if (s != null && s.InvokeRequired)
                        s.Invoke(d, new object[] { null, e });
                    else
                        d.DynamicInvoke(new object[] { null, e });
                }
            }
#endif
*/
        }
    }
    /// <summary>
    /// DeviceFinder Argument
    /// </summary>
    public class DeviceFinderArgs : EventArgs
    {
        private DeviceInfomation _data;
        /// <summary>
        /// Device Finder 
        /// </summary>
        /// <param name="data"></param>
        public DeviceFinderArgs(DeviceInfomation data)
        {
            _data = data;
        }
        /// <summary>
        /// Device finder information
        /// </summary>
        public DeviceInfomation Found
        {
            get { return _data; }
            set { _data = value; }
        }
    }
    /// <summary>
    /// Result Argument
    /// </summary>
    public class ResultArgs : EventArgs
    {
        private AssignResult m_Result;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Result"></param>
        public ResultArgs(AssignResult Result)
        {
            m_Result = Result;
        }
        /// <summary>
        /// Result
        /// </summary>
        public AssignResult Result
        {
            get { return m_Result; }
            set { m_Result = value; }
        }
    }
    /// <summary>
    /// UpdateResultArgs
    /// </summary>
    public class UpdateResultArgs : EventArgs
    {
        private UpdateResult m_Result;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Result"></param>
        public UpdateResultArgs(UpdateResult Result)
        {
            m_Result = Result;
        }
        /// <summary>
        /// Update Result
        /// </summary>
        public UpdateResult Result
        {
            get { return m_Result; }
            protected set { m_Result = value; }
        }
    }
    /// <summary>
    /// UpdatePercentArgs
    /// </summary>
    public class UpdatePercentArgs : EventArgs
    {
        private int m_percent;
        /// <summary>
        /// Constrcutor
        /// </summary>
        /// <param name="percent"></param>
        public UpdatePercentArgs(int percent)
        {
            m_percent = percent;
        }
        /// <summary>
        /// Total update Percent
        /// </summary>
        public int Percent
        {
            get { return m_percent; }
            protected set { m_percent = value; }
        }
    }

    /// <summary>
    /// A TFTP Exception
    /// </summary>
    class TFTPException : Exception
    {
        /// <summary>
        /// Error Message
        /// </summary>
        public string ErrorMessage = "";
        /// <summary>
        /// Error code
        /// </summary>
        public int ErrorCode = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="TFTPException"/> class.
        /// </summary>
        /// <param name="errCode">The err code.</param>
        /// <param name="errMsg">The err MSG.</param>
        public TFTPException(int errCode, string errMsg)
        {
            ErrorCode = errCode;
            ErrorMessage = errMsg;
        }

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns>
        /// A string representation of the current exception.
        /// </returns>
        /// <filterPriority>1</filterPriority>
        /// <permissionSet class="System.Security.permissionSet" version="1">
        /// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*"/>
        /// </permissionSet>
        public override string ToString()
        {
            return String.Format("TFTPException: ErrorCode: {0} Message: {1}", ErrorCode, ErrorMessage);
        }
    }
}
