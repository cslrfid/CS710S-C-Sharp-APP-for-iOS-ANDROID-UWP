using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Windows.Security.Cryptography;
using Windows.Storage.Streams;

using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace CSLibrary
{
    public partial class HighLevelInterface
    {
        #region Error Codes
        readonly int E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED = unchecked((int)0x80650003);
        readonly int E_BLUETOOTH_ATT_INVALID_PDU = unchecked((int)0x80650004);
        readonly int E_ACCESSDENIED = unchecked((int)0x80070005);
        readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)
        #endregion

        // for bluetooth Connection
        private BluetoothLEDevice bluetoothLeDevice = null;         // BLE Device handler 
        IReadOnlyList<GattDeviceService> services = null;           // Services list of device 
        IReadOnlyList<GattCharacteristic> characteristics = null;   // Characteristics list of service

        private GattCharacteristic notificationCharacteristic;      // Characteristic for notification (data read)
        private GattCharacteristic writeCharacteristic;             // Characteristic for write (data send)
        private GattPresentationFormat presentationFormat;

        /// <summary>
        /// return error code
        /// </summary>
        /// <returns></returns>
        int BLE_Init()
        {
            return 0;
        }

        public async Task<bool> ConnectAsync(string id)
        {
            try
            {
                // BT_Code: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
                bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(id);

                if (bluetoothLeDevice == null)
                {
                    Debug.WriteLine("Failed to connect to device.");
                    return false;
                }
            }
            catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
            {
                Debug.WriteLine("Bluetooth radio is not on.");
                return false;
            }

            GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

            if (result.Status == GattCommunicationStatus.Success)
            {
                services = result.Services;

                Debug.WriteLine(String.Format("Found {0} services", services.Count));
            }
            else
            {
                Debug.WriteLine("Device unreachable");
            }

            if (services.Count < 2)
                return false;

            foreach (GattDeviceService service in services)
            {
                if (service.Uuid == Guid.Parse("00009800-0000-1000-8000-00805f9b34fb"))
                {
                    characteristics = null;

                    try
                    {
                        // Ensure we have access to the device.
                        var accessStatus = await service.RequestAccessAsync();

                        if (accessStatus == Windows.Devices.Enumeration.DeviceAccessStatus.Allowed)
                        {
                            // BT_Code: Get all the child characteristics of a service. Use the cache mode to specify uncached characterstics only 
                            // and the new Async functions to get the characteristics of unpaired devices as well. 
                            var result1 = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                            if (result1.Status == GattCommunicationStatus.Success)
                            {
                                characteristics = result1.Characteristics;
                            }
                            else
                            {
                                // On error, act as if there are no characteristics.
                                return false;
                            }
                        }
                        else
                        {
                            // Not granted access
                            // On error, act as if there are no characteristics.
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        // On error, act as if there are no characteristics.
                        //characteristics = new List<GattCharacteristic>();
                        return false;
                    }
                }
            }

            // Find notification characteristic
            notificationCharacteristic = null;
            foreach (GattCharacteristic characteristic in characteristics)
            {
                if (characteristic.Uuid == Guid.Parse("00009901-0000-1000-8000-00805f9b34fb"))
                {
                    notificationCharacteristic = characteristic;
                    break;
                }
            }
            if (notificationCharacteristic == null)
                return false;

            // Turn notification on
            GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
            var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
            if (notificationCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
            }
            else if (notificationCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
            }

            // Set notification call back
            try
            {
                // BT_Code: Must write the CCCD in order for server to send indications.
                // We receive them in the ValueChanged event handler.
                status = await notificationCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);

                if (status == GattCommunicationStatus.Success)
                {
                    notificationCharacteristic.ValueChanged += BLE_Recv;
                }
                else
                {
                    return false;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // This usually happens when a device reports that it support indicate, but it actually doesn't.
                //rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                return false;
            }


            // Find write characteristic
            writeCharacteristic = null;
            foreach (GattCharacteristic characteristic in characteristics)
            {
                if (characteristic.Uuid == Guid.Parse("00009900-0000-1000-8000-00805f9b34fb"))
                {
                    writeCharacteristic = characteristic;
                    break;
                }
            }
            if (writeCharacteristic == null)
                return false;

            presentationFormat = null;
            if (writeCharacteristic.PresentationFormats.Count > 0)
            {
                if (writeCharacteristic.PresentationFormats.Count.Equals(1))
                {
                    // Get the presentation format since there's only one way of presenting it
                    presentationFormat = writeCharacteristic.PresentationFormats[0];
                }
                else
                {
                    // It's difficult to figure out how to split up a characteristic and encode its different parts properly.
                    // In this case, we'll just encode the whole thing to a string to make it easy to print out.
                    return false;
                }
            }

            // Jump to CS108 Connect
            _handleSiliconLabIC.GetVersion();
            HardwareInit();

            return true;
        }

        public async Task<bool> DisconnectAsync()
        {
            //if (Status != READERSTATE.IDLE)
            //    return false;

            BARCODEPowerOff();

            // Need to clear the CCCD from the remote device so we stop receiving notifications
            
            //var result = await notificationCharacteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
            var result = await notificationCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
            if (result != GattCommunicationStatus.Success)
                return false;

            notificationCharacteristic.ValueChanged -= BLE_Recv;

            foreach (var ser in services)
                ser?.Dispose();

            bluetoothLeDevice?.Dispose();
            bluetoothLeDevice = null;

            return true;
        }

        /// <summary>
        /// return error code
        /// </summary>
        /// <returns></returns>
        private async Task<bool> BLE_Send (byte[] data)
        {
            if (data.Length <= 0)
                return true;

            try
            {
                var writer = new DataWriter();

                writer.ByteOrder = ByteOrder.LittleEndian;
                writer.WriteBytes(data);

                // BT_Code: Writes the value from the buffer to the characteristic.
                var result = await  writeCharacteristic.WriteValueWithResultAsync(writer.DetachBuffer(), GattWriteOption.WriteWithResponse);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    Debug.WriteLine("Successfully wrote value to device");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Write failed: {result.Status}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        private async void BLE_Recv(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            // BT_Code: An Indicate or Notify reported that the value has changed.
            byte[] data;

            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out data);

            CharacteristicOnValueUpdated(data);
        }
    }
}
