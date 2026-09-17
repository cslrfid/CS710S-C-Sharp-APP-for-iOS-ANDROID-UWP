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

using CSLibrary.Structures;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSLibrary.Tools
{
    public class CSLTCPSTREAM
    {
        public enum ConnectionStatus
        {
            Connecting,
            Connected,
            Disconnected,
            ConnectionClosed,
            Sending,
            Received,
            SendFail,
            ReceiveFail,
            Reconnecting,
            ReconnectFail,
        }

        private TcpClient _client;
        private Socket _socket; // 直接使用Socket獲得更好的性能
        private string _ipAddress;
        private int _port;
        private CancellationTokenSource _cancellationTokenSource = null;
        private Task _receivingTask = null;
        private Task _packetAnalysisTask = null;

        public event Action<ConnectionStatus> ConnectionStatusChanged;
        public event Action<byte[]> DataReceived;

        /// <summary>
        /// Initialize TCP/IP connection
        /// </summary>
        /// <returns>Error code (0 = success)</returns>
        public int Initialize()
        {
            return 0;
        }

        /// <summary>
        /// Connect to TCP/IP server asynchronously
        /// </summary>
        /// <param name="ipAddress">IP address to connect to</param>
        /// <param name="port">Port number to connect to</param>
        public async Task ConnectAsync(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            _client = new TcpClient();
            _client.SendBufferSize = 65535; // Set send buffer size
            _client.ReceiveBufferSize = 65535; // Set receive buffer size
            ConnectionStatusChanged?.Invoke(ConnectionStatus.Connecting);

            try
            {
                await _client.ConnectAsync(ipAddress, port);
                _socket = _client.Client;
                _socket.NoDelay = true;
                _socket.ReceiveBufferSize = 131072;
                _socket.SendBufferSize = 131072;

                ConnectionStatusChanged?.Invoke(ConnectionStatus.Connected);
                StartReceiving();
            }
            catch
            {
                ConnectionStatusChanged?.Invoke(ConnectionStatus.Disconnected);
            }
        }

        /// <summary>
        /// Send data over TCP/IP connection
        /// </summary>
        /// <param name="data">Data to send</param>
        public async Task SendAsync(byte[] data)
        {
            try
            {
                if (_socket != null && _socket.Connected)
                {
                    await _socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
                    ConnectionStatusChanged?.Invoke(ConnectionStatus.Sending);
                }
                else
                {
                    await ReconnectAsync();
                    await SendAsync(data); // Retry sending after reconnect
                }
            }
            catch
            {
                ConnectionStatusChanged?.Invoke(ConnectionStatus.SendFail);
                DisconnectAsync();
            }
        }


        /// <summary>
        /// Start receiving data from TCP/IP connection
        /// </summary>
        private void StartReceiving()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            //CSLibrary.Tools.HPFIFOQueue networkdata = new CSLibrary.Tools.HPFIFOQueue(4096);
            FIFOQueue networkdata = new FIFOQueue(10*1024*1024);
            byte[] buffer = new byte[1024];
            var segment = new ArraySegment<byte>(buffer);

            _receivingTask = Task.Factory.StartNew(async () =>
            {
                // 設置當前線程為最高優先級
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                Thread.CurrentThread.Name = "TCP_Receive_HighPriority";

                ConnectionStatusChanged?.Invoke(ConnectionStatus.Received);
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    int bytesRead;
                    try
                    {
                        if (_socket.Available == 0) // 使用更快的Socket檢查
                        {
                            Thread.Sleep(1);
                            continue;
                        }

                        bytesRead = await _socket.ReceiveAsync(segment, SocketFlags.None);
                        if (bytesRead > 0)
                        {
                            //CSLibrary.Debug.WriteLine(DateTime.Now.ToString() + ":In Append");
                            networkdata.Append(buffer, bytesRead);
                            //CSLibrary.Debug.WriteLine(DateTime.Now.ToString() + ":Out Append");

                            //while (_socket.Available > buffer.Length)
                            //    await _socket.ReceiveAsync(segment, SocketFlags.None);
                        }
                    }
                    catch (Exception ex)
                    {
                        ConnectionStatusChanged?.Invoke(ConnectionStatus.ReceiveFail);
                        DisconnectAsync();
                        break;
                    }
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();

            _packetAnalysisTask = Task.Run(async () =>
            {

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    //CSLibrary.Debug.WriteLine(DateTime.Now.ToString() + ":In ToHeader");
                    if (!networkdata.ToHeader(0xa7) || networkdata.Count < 8)
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                    //CSLibrary.Debug.WriteLine(DateTime.Now.ToString() + ":Out ToHeader");

                    //CSLibrary.Debug.WriteLine(DateTime.Now.ToString() + ":In Peek");
                    byte[] PeekData = networkdata.Peek(1, 3);
                    //CSLibrary.Debug.WriteLine(DateTime.Now.ToString() + ":Out Peek");

                    if (PeekData == null)
                        continue;

                    if (PeekData[0] != 0xb3) // 0xb3 is the second packet header mark
                    {
                        //CSLibrary.Debug.WriteLine(DateTime.Now.ToString() + ":In Seek");
                        networkdata.Seek(1);
                        //CSLibrary.Debug.WriteLine(DateTime.Now.ToString() + ":Out Seek");
                        continue; // Skip to next header
                    }

                    int packetLength = PeekData[1] + 8; // 8 is the header size
                    if (networkdata.Count < packetLength)
                        continue; // Not enough data for a complete packet

                    //CSLibrary.Debug.WriteLine(DateTime.Now.ToString() + ":In Read");
                    byte[] data = networkdata.Read(packetLength);
                    //CSLibrary.Debug.WriteLine(DateTime.Now.ToString() + ":Out Read");

                    Task.Run(() =>
                    {
                        try
                        {
                            // Trigger data received event in a separate thread to avoid blocking TCP reception
                            DataReceived?.Invoke(data);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Packet routine error!!!");
                        }
                    });
                }
            });
        }

        /// <summary>
        /// Reconnect to TCP/IP server with retry logic
        /// </summary>
        public async Task ReconnectAsync()
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                try
                {
                    ConnectionStatusChanged?.Invoke(ConnectionStatus.Reconnecting);
                    await ConnectAsync(_ipAddress, _port);
                    return; // Successfully reconnected
                }
                catch
                {
                    ConnectionStatusChanged?.Invoke(ConnectionStatus.ReconnectFail);
                    await Task.Delay(1000); // Wait before retrying
                }
            }
            ConnectionStatusChanged?.Invoke(ConnectionStatus.Disconnected);
        }

        /// <summary>
        /// Disconnect from TCP/IP server
        /// </summary>
        public async void DisconnectAsync()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }

            // Wait for receiving task to complete
            if (_receivingTask != null)
            {
                try
                {
                    await _receivingTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation token is used
                }
                catch (Exception)
                {
                    // Handle other exceptions if needed
                }
                _receivingTask = null;
            }

            if (_packetAnalysisTask != null)
            {
                try
                {
                    await _packetAnalysisTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation token is used
                }
                catch (Exception)
                {
                    // Handle other exceptions if needed
                }
                _packetAnalysisTask = null;
            }

            if (_client != null)
            {
                _client.Close();
                ConnectionStatusChanged?.Invoke(ConnectionStatus.ConnectionClosed);
            }
        }

        /// <summary>
        /// Check if TCP/IP connection is established
        /// </summary>
        public bool IsConnected => _socket?.Connected ?? false;

        /// <summary>
        /// Get current IP address
        /// </summary>
        public string IPAddress => _ipAddress;

        /// <summary>
        /// Get current port
        /// </summary>
        public int Port => _port;

    }
}