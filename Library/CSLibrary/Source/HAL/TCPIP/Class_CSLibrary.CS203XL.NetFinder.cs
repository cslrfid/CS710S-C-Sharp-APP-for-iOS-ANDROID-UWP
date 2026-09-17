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
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Net;

using System.Text;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;

namespace CSLibrary.NetFinder.CS203XL
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
        /// Idle
        /// </summary>
        IDLE,
        /// <summary>
        /// 
        /// </summary>
        CLOSED,
    }
    #endregion

    /// <summary>
    /// Netfinder Mode
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// Application mode (Network device)
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Time Event
    /// </summary>
    public struct TimeEvent
    {
        /// <summary>
        /// Event name
        /// </summary>
        public string name;
        /// <summary>
        /// Days
        /// </summary>
        public uint days;
        /// <summary>
        /// Hours
        /// </summary>
        public uint hours;
        /// <summary>
        /// Minutes
        /// </summary>
        public uint minutes;
        /// <summary>
        /// Seconds
        /// </summary>
        public uint seconds;
    }

    /// <summary>
    /// MAC Structure
    /// </summary>
    public class MAC
    {
        /// <summary>
        /// Mac Address in 6 bytes format
        /// </summary>
        public byte[] Address = new byte[6];
        /// <summary>
        /// MAC Address in String format, i.e. 00:19:BB:44:7C:AA
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (Address == null || Address.Length == 0) ? "Invalid IPAddress Formate" :
                string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", Address[0], Address[1], Address[2], Address[3], Address[4], Address[5]);
        }

/*        /// <summary>
        /// Check equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is MAC)
            {
                return Win32.memcmp(((MAC)obj).Address, Address, 6) == 0;
            }
            else
            {
                Byte[] array = (Byte[])obj;
                if (array != null)
                {
                    return Win32.memcmp((Byte[])obj, Address, 6) == 0;
                }
            }
            return false;
        }
*/

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <param name="mac"></param>
        /// <returns></returns>
        public static implicit operator String(MAC mac)
        {
            return mac.ToString();
        }

        /// <summary>
        /// Parse String to Byte array format
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] Parse(String address)
        {
            if (String.IsNullOrEmpty(address))
                return null;
            String[] split = address.Split(':');
            if (split == null || split.Length != 6)
                return null;
            return new Byte[]{
                byte.Parse(split[0], System.Globalization.NumberStyles.HexNumber),
                byte.Parse(split[1], System.Globalization.NumberStyles.HexNumber),
                byte.Parse(split[2], System.Globalization.NumberStyles.HexNumber),
                byte.Parse(split[3], System.Globalization.NumberStyles.HexNumber),
                byte.Parse(split[4], System.Globalization.NumberStyles.HexNumber),
                byte.Parse(split[5], System.Globalization.NumberStyles.HexNumber)};
        }
    }

    /// <summary>
    /// IP Structure
    /// </summary>
    public class IP
    {
        /// <summary>
        /// IP Address in 4 bytes format
        /// </summary>
        public byte[] Address = new byte[4];
        /// <summary>
        /// IP Address in String format, i.e. 127.0.0.1
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (Address == null || Address.Length == 0) ? "Invalid IPAddress Formate" :
                string.Format("{0}.{1}.{2}.{3}", Address[0], Address[1], Address[2], Address[3]);
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static implicit operator String(IP ip)
        {
            return ip.ToString();
        }
        /// <summary>
        /// Convert to string
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static implicit operator long(IP ip)
        {
            return (long)BitConverter.ToInt32(ip.Address, 0);
        }
    }

    /// <summary>
    /// Netfinder information return from device
    /// </summary>
    public class DeviceInfomation
    {
        /// <summary>
        /// Reserved for future use
        /// </summary>
        public Mode Mode = Mode.Unknown;
        /// <summary>
        /// Total time on network
        /// </summary>
        public TimeEvent TimeElapsedNetwork = new TimeEvent();
        /// <summary>
        /// Total Power on time
        /// </summary>
        public TimeEvent TimeElapsedPowerOn = new TimeEvent();
        /// <summary>
        /// MAC address
        /// </summary>
        public MAC MACAddress = new MAC();//[6];
        /// <summary>
        /// IP address
        /// </summary>
        public IP IPAddress = new IP();
        /// <summary>
        /// Subnet Mask
        /// </summary>
        public IP SubnetMask = new IP();
        /// <summary>
        /// Gateway
        /// </summary>
        public IP Gateway = new IP();
        /// <summary>
        /// Trusted hist IP
        /// </summary>
        public IP TrustedServer = new IP();
        /// <summary>
        /// Inducated trusted server enable or not.
        /// </summary>
        public Boolean TrustedServerEnabled = false;
        /// <summary>
        /// UDP Port
        /// </summary>
        public ushort Port; // Get port from UDP header
        /*/// <summary>
        /// Reserved for future use, Server mode ip
        /// </summary>
        public byte[] serverip = new byte[4];*/
        /// <summary>
        /// enable or disable DHCP
        /// </summary>
        public bool DHCPEnabled;
        /*/// <summary>
        /// Reserved for future use, Server mode port
        /// </summary>
        public ushort serverport;*/
        /// <summary>
        /// DHCP retry
        /// </summary>
        public byte DHCPRetry;
        /// <summary>
        /// Device name, user can change it.
        /// </summary>
        public string DeviceName;
        /// <summary>
        /// Mode discription
        /// </summary>
        public string Description;
        /// <summary>
        /// Connect Mode
        /// </summary>        
        public byte ConnectMode;
        /// <summary>
        /// Gateway check reset mode
        /// </summary>
        public int GatewayCheckResetMode;
    }

    class DeviceCollection : List<DeviceInfomation>
    {
        public DeviceInfomation GetDeviceInfo(int index)
        {
            return index >= 0 && index < Count ? this[index] : null;
        }

        public DeviceInfomation GetDeviceInfo(MAC MACAddress)
        {
            int index = this.FindIndex(delegate (DeviceInfomation item) { return (item.MACAddress.Equals(MACAddress)); });
            if (index < 0)
                return null;
            return this[index];
        }
    }


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

        private IPAddress m_targetSearch = new IPAddress(0);


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
        /// Constructor
        /// </summary>
        public NetFinder()
        {
            try
            {
                m_broadcast = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                m_broadcast.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                //m_broadcast.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.BlockSource, 1);
                //m_broadcast.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);

                //Start Listening here
                Operation = RecvOperation.IDLE;

                Thread run = new Thread(new ThreadStart(RecvThread));
                run.IsBackground = true;
                run.Start();
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
        /// Start to re-search device on ethernet continuously until Stop function called.
        /// </summary>
        public void ResearchDevice()
        {
            if (Operation == RecvOperation.SEARCH)
                lock (m_lock) m_research = true;
        }

        private DeviceInfomation GetTargetEntry(Byte[] TargetMAC)
        {
            // Check the mac address of each cell in the table
            return m_devices.Find(delegate(DeviceInfomation info) { return info.MACAddress.Equals(TargetMAC); });
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
                    Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Clear all device list
        /// </summary>
        public void ClearDeviceList()
        {
            m_devices.Clear();
        }

        private void RecvThread()
        {
            while (true)
            {
                switch (Operation)
                {
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

        private void SendAllBroadcast(byte[] buff)
        {
            IPEndPoint endPoint;

            try
            {
                if (m_targetSearch.Address != IPAddress.Broadcast.Address)
                {
                    endPoint = new IPEndPoint(m_targetSearch, 3050);

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

                                    endPoint = new IPEndPoint(new IPAddress(broadcastIPBytes), 3050);
                                    m_broadcast.SendTo(buff, endPoint);
                                }
                            }
                        }
                    }
                }
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
            //IPEndPoint endPoint = new IPEndPoint(m_targetSearch, 3050);
            //IPEndPoint endPoint;

            Random rand = new Random();
            byte[] buff = new byte[] { 0, 0, 0, 0 };

            m_rand = rand.Next(1, 0x7fff); // rand returns a number between 0 and 0x7FFF

            buff[2] = (byte)(m_rand >> 8);
            buff[3] = (byte)(m_rand & 0x00FF);

            SendAllBroadcast(buff);
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
                // This is a very rare case in which the PC randomly chooses port 3050 
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

        /// <summary>
        /// Get Read Tag GPO Indication Status
        /// </summary>
        public int GetTagIndicationMode(string IP)
        {
            byte[] CMDBuf = new byte[10];
            UdpClient UdpCMD = new UdpClient();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(IP), 3051);
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
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(IP), 3051);
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
        public void GetPoeInfo(string IP, out byte Type, out UInt16 RequestedPower, out UInt16 AllocatedPower)
        {
            byte[] CMDBuf = new byte[10];
            UdpClient UdpCMD = new UdpClient();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(IP), 3051);
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

            //return Result.OK;
        }

        /// <summary>
        /// Set POE Request Power 0.1w pre step
        /// </summary>
        public void SetPoePower(string IP, UInt16 power)
        {
            byte[] CMDBuf = new byte[10];
            UdpClient UdpCMD = new UdpClient();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(IP), 3051);
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

            //return Result.OK;
        }

        private void RaiseEvent<T>(EventHandler<T> eventHandler, object sender, T e)
            where T : EventArgs
        {
            if (eventHandler != null)
            {
                eventHandler(sender, e);
            }
            return;
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
}
