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


#if TCP
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSLibrary
{
    using Constants;
    //using CSLRFIDCommunicationAPI;
    using System.Collections.Generic;
    using System.Linq;
    using static CSLibrary.HighLevelInterface;
    using static RFIDDEVICE;

    public partial class HighLevelInterface
    {
        private CSLibrary.Tools.CSLTCPSTREAM _tcpipClient;
        //CSLRFIDCommunicationPacketAPI networkdata = new CSLRFIDCommunicationPacketAPI();

        MODEL _deviceType = MODEL.CS710S;

        /// <summary>
        /// return error code
        /// </summary>
        /// <returns></returns>
        int TCP_Init()
        {
            _tcpipClient = new CSLibrary.Tools.CSLTCPSTREAM();
            _tcpipClient.DataReceived += OnTcpDataReceived;
            return 0;
        }

        private void OnTcpDataReceived(byte[] data)
        {
            CSLibrary.Debug.WriteBytes("TCP receive data ", data);
            CharacteristicOnValueUpdated(data);
        }

        public async Task ConnectAsync(string ipAddress, int port)
        {
            if (_tcpipClient == null)
                TCP_Init();

            _sp._ConnectionMode = CONNECTIONMODE.TCP;
            await _tcpipClient.ConnectAsync(ipAddress, port);
            
            _readerState = READERSTATE.IDLE;
            BTTimer = new Timer(TimerFunc, this, 0, 1000);
            HardwareInit();
        }

        public async Task TCP_Send(byte[] data)
        {
            if (_tcpipClient != null)
            {
                await _tcpipClient.SendAsync(data);
            }
        }

        public async Task ReconnectAsync()
        {
            if (_tcpipClient != null)
            {
                await _tcpipClient.ReconnectAsync();
            }
        }

        public void DisconnectAsync()
        {
            if (_tcpipClient != null)
            {
                _tcpipClient.DisconnectAsync();
            }
            _readerState = READERSTATE.DISCONNECT;
        }

        int BLE_Init()
        {
            return TCP_Init();
        }

        public async Task BLE_Send(byte[] data)
        {
            await TCP_Send(data);
        }

        // Expose TCP/IP client properties for backward compatibility
        public bool IsConnected => _tcpipClient?.IsConnected ?? false;
        
        public event Action<CSLibrary.Tools.CSLTCPSTREAM.ConnectionStatus> ConnectionStatusChanged
        {
            add { if (_tcpipClient != null) _tcpipClient.ConnectionStatusChanged += value; }
            remove { if (_tcpipClient != null) _tcpipClient.ConnectionStatusChanged -= value; }
        }

    }
}
#endif