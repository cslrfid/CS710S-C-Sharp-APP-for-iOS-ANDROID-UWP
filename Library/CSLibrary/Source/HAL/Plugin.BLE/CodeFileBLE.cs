/*
Copyright (c) 2018 Convergence Systems Limited

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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace CSLibrary
{
    using static RFIDDEVICE;
    using Constants;
    using System.Linq;

    public partial class HighLevelInterface
    {
        // for bluetooth Connection
        // for bluetooth Connectiond
        IAdapter _adapter;
        IDevice _device;
        IService _service;
        IService _serviceDeviceInfo;
        ICharacteristic _characteristicWrite;
        ICharacteristic _characteristicUpdate;
        ICharacteristic _characteristicDeviceInfoRead;
        MODEL _deviceType = MODEL.UNKNOWN;

        /// <summary>
        /// return error code
        /// </summary>
        /// <returns></returns>
        int BLE_Init()
        {
            return 0;
        }

        public async Task<bool> ConnectAsync(IAdapter adapter, IDevice device, MODEL deviceType)
        {
            if (_readerState != READERSTATE.DISCONNECT)
                return false; // reader can not reconnect

            this._deviceType = deviceType;

            try
            {
                switch (_deviceType)
                {
                    case MODEL.CS108:
                        _service = await device.GetServiceAsync(Guid.Parse("00009800-0000-1000-8000-00805f9b34fb"));
                        break;

                    case MODEL.CS710S:
                        await device.RequestMtuAsync(255); // for BLE 5.0
                        _service = await device.GetServiceAsync(Guid.Parse("00009802-0000-1000-8000-00805f9b34fb"));
                        break;
                }

                if (_service == null)
                    return false;

                _serviceDeviceInfo = await device.GetServiceAsync(Guid.Parse("0000180a-0000-1000-8000-00805f9b34fb"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Program execption error, please check!!! error message : " + ex.Message);
            }

            _readerState = READERSTATE.IDLE;

            _adapter = adapter;
            _device = device;

            _adapter.DeviceConnectionLost -= OnDeviceConnectionLost;
            _adapter.DeviceConnectionLost += OnDeviceConnectionLost;

            try
            {
                _characteristicWrite = await _service.GetCharacteristicAsync(Guid.Parse("00009900-0000-1000-8000-00805f9b34fb"));
                _characteristicUpdate = await _service.GetCharacteristicAsync(Guid.Parse("00009901-0000-1000-8000-00805f9b34fb"));
                _characteristicWrite = await _service.GetCharacteristicAsync(Guid.Parse("00009900-0000-1000-8000-00805f9b34fb"));

                if (_serviceDeviceInfo != null)
                {
                    _characteristicDeviceInfoRead = await _serviceDeviceInfo.GetCharacteristicAsync(Guid.Parse("00002a23-0000-1000-8000-00805f9b34fb"));

                    await _characteristicDeviceInfoRead.ReadAsync();
                    
                    if (_characteristicDeviceInfoRead?.Value.Count() == 8)
                    {
                        _MacAdd = _characteristicDeviceInfoRead?.Value[7].ToString("X2") +
                            _characteristicDeviceInfoRead?.Value[6].ToString("X2") +
                            _characteristicDeviceInfoRead?.Value[5].ToString("X2") +
                            _characteristicDeviceInfoRead?.Value[2].ToString("X2") +
                            _characteristicDeviceInfoRead?.Value[1].ToString("X2") +
                            _characteristicDeviceInfoRead?.Value[0].ToString("X2");
                            ;

                        //CSLibrary.Debug.WriteLine("BLE Mac Addres : " + _MacAdd);
                    }
                }
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("Can not set characters");
            }

            _characteristicUpdate.ValueUpdated -= BLE_Recv;
            _characteristicUpdate.ValueUpdated += BLE_Recv;
            //            _characteristicWrite.ValueUpdated += CharacteristicOnWriteUpdated;

            //CSLibraryv4: wait for new ble library compatibility
            BTTimer = new Timer(TimerFunc, this, 0, 1000);

            await _characteristicUpdate.StartUpdatesAsync();
            //            await _characteristicWrite.StartUpdatesAsync();

            _readerState = READERSTATE.IDLE;
            BTTimer = new Timer(TimerFunc, this, 0, 1000);

            HardwareInit();

            return true;
        }

        public async Task<bool> DisconnectAsync()
        {
            try
            {
                if (Status != READERSTATE.IDLE)
                    return false;

                if (_readerState != READERSTATE.DISCONNECT)
                {
                    BARCODEPowerOff();
                    //CSLibraryv4: clear connection without waiting for BLE readiness
                    //WhenBLEFinish(ClearConnection);
                    await ClearConnection();
                }
                else
                {
                    await ClearConnection();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Disconnect error " + ex.Message.ToString());
            }

            return true;
        }

        /// <summary>
        /// return error code
        /// </summary>
        /// <returns></returns>
        private async Task<int> BLE_Send (byte[] data)
        {
            return await _characteristicWrite.WriteAsync(data);
        }

        private async void BLE_Recv(object sender, CharacteristicUpdatedEventArgs characteristicUpdatedEventArgs)
        {
            try
            {
                byte[] data = characteristicUpdatedEventArgs.Characteristic.Value;
                if (data == null)
                    return;
 
                CSLibrary.Debug.WriteBytes("BT data received", data);
                CharacteristicOnValueUpdated(data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Program execption error, please check!!! error message : " + ex.Message);
            }
        }

        private void CharacteristicOnWriteUpdated(object sender, CharacteristicUpdatedEventArgs characteristicUpdatedEventArgs)
        {
            CSLibrary.Debug.WriteBytes("BT: Write data success updated", characteristicUpdatedEventArgs.Characteristic.Value);
        }

        private void OnStateChanged(object sender, BluetoothStateChangedArgs e)
        {
        }

        private void OnDeviceConnectionLost(object sender, DeviceErrorEventArgs e)
        {
            if (e.Device.Id == _device.Id)
            {
                //DisconnectAsync();
                ConnectLostAsync();
            }
        }

        public async void ConnectLostAsync()
        {
            _readerState = READERSTATE.READYFORDISCONNECT;

            _characteristicUpdate.ValueUpdated -= BLE_Recv;
            _adapter.DeviceConnectionLost -= OnDeviceConnectionLost;

            _characteristicUpdate = null;
            _characteristicWrite = null;
            _service = null;

            try
            {

                if (_device.State == DeviceState.Connected)
                {
                    await _adapter.DisconnectDeviceAsync(_device);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Program execption error, please check!!! error message : " + ex.Message);
            }
            _device = null;

            _readerState = READERSTATE.DISCONNECT;

            FireReaderStateChangedEvent(new Events.OnReaderStateChangedEventArgs(null, Constants.ReaderCallbackType.CONNECTION_LOST));
        }

        async Task ClearConnection()
        {
            _readerState = READERSTATE.READYFORDISCONNECT;
            // Stop Timer;
            await _characteristicUpdate.StopUpdatesAsync();

            _characteristicUpdate.ValueUpdated -= BLE_Recv;
            _adapter.DeviceConnectionLost -= OnDeviceConnectionLost;

            _characteristicUpdate = null;
            _characteristicWrite = null;
            _service = null;

            try
            {
                if (_device.State == DeviceState.Connected)
                {
                    await _adapter.DisconnectDeviceAsync(_device);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Program execption error, please check!!! error message : " + ex.Message);
            }
            _device = null;

            _readerState = READERSTATE.DISCONNECT;
        }

    }
}
