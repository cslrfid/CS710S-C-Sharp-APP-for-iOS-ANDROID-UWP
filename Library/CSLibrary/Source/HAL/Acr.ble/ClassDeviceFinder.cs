using System;
using System.Collections.Generic;

using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace CSLibrary
{
    public partial class DeviceFinder
    {
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
        /// Netfinder information return from device
        /// </summary>
        public class DeviceInfomation
        {
            public uint ID;

            public string deviceName;

            public object nativeDeviceInformation;

            /*
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
                    /// <summary>
                    /// Reserved for future use, Server mode ip
                    /// </summary>
                    public byte[] serverip = new byte[4];
                    /// <summary>
                    /// enable or disable DHCP
                    /// </summary>
                    public bool DHCPEnabled;
                    /// <summary>
                    /// Reserved for future use, Server mode port
                    /// </summary>
                    public ushort serverport;
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
            */
        }

        static private Windows.Devices.Enumeration.DeviceWatcher deviceWatcher;
	    static List<Windows.Devices.Enumeration.DeviceInformation> _deviceDB = new List<Windows.Devices.Enumeration.DeviceInformation>();

        static public event EventHandler<DeviceFinderArgs> OnSearchCompleted;

        static public void SearchDevice()
        {
            // Additional properties we would like about the device.
            // Property strings are documented here https://msdn.microsoft.com/en-us/library/windows/desktop/ff521659(v=vs.85).aspx
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable", "System.Devices.Aep.AepId", "System.Devices.Aep.Category" };

            // BT_Code: Example showing paired and non-paired in a single query.
            string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

            deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        aqsAllBluetoothLEDevices,
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start the watcher.
            deviceWatcher.Start();
        }


        static public void Stop()
        {
            /// <summary>
            /// Stops watching for all nearby Bluetooth devices.
            /// </summary>
            if (deviceWatcher != null)
            {
                // Unregister the event handlers.
                deviceWatcher.Added -= DeviceWatcher_Added;

                // Stop the watcher.
                deviceWatcher.Stop();
                deviceWatcher = null;
            }
        }

	    static public void ClearDeviceList()
	    {
		    _deviceDB.Clear ();
	    }

        static public DeviceInformation GetDeviceInformation(int id)
        {
            if (id < _deviceDB.Count)
                return _deviceDB[id];

            return null;
        }

        static public DeviceInformation GetDeviceInformation (string readername)
	    {
		    foreach (DeviceInformation item in _deviceDB)
		    {
			    if (item.Id == readername)
				    return item;
		    }

		    return null;		
	    }

	    static public List<DeviceInformation> GetAllDeviceInformation ()
	    {
		    return _deviceDB;
	    }

        static private async void DeviceWatcher_Added(DeviceWatcher sender, Windows.Devices.Enumeration.DeviceInformation deviceInfo)
        {
            Debug.WriteLine(String.Format("Added {0}{1}", deviceInfo.Id, deviceInfo.Name));

            // Protect against race condition if the task runs after the app stopped the deviceWatcher.
            if (sender == deviceWatcher)
            {
                CSLibrary.DeviceFinder.DeviceInfomation di = new CSLibrary.DeviceFinder.DeviceInfomation();
                di.deviceName = deviceInfo.Name;
                di.ID = (uint)_deviceDB.Count;
                di.nativeDeviceInformation = (object)deviceInfo;

                _deviceDB.Add(deviceInfo); 

                RaiseEvent<DeviceFinderArgs>(OnSearchCompleted, new DeviceFinderArgs(di));
            }
        }

        static private async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
        }

        static private async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
        }

        static private async void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {
        }

        static private async void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
        }

        static private void RaiseEvent<T>(EventHandler<T> eventHandler, T e)
            where T : EventArgs
        {
            if (eventHandler != null)
            {
                eventHandler(null, e);
            }
            return;
        }
    }

}