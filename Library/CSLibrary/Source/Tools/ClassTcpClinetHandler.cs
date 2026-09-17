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

/* Github copilot

寫一個 C# 7.3 class, 功能是 TCP/IP 

1. API ConnectAsync : 可在 connect 時輸入 IP address 和 port number
2. API ReconnectAsync : 如send 或 receive 不到, 可自動 reconnect 之前 connect 的 address/port, 
   每次reconnect 可試10次, 如reconnect fail 則disconnect, 另外 call一個 connection status EventHandler 出來
2. API TCP_Send : send binary data, 如send 不到, 使用 reconnect, 成功之後 resend, 不成功則 Disconnect, 停止收data
3. API TCP_Recv : 獨立一個 task 一直收 binary data 而且有 CancellationToken cancellationToken, 有  data 時主動call "CharacteristicOnValueUpdated" routine 
4. DisconnectAsync : 可停止收 data cancellationToken, 及關掉所有 connection
5. 有完整的 connection status EventHandler 可由外部知道完整 connection 情況
public enum ConnectionStatus {
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
*/

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


public class TcpClientHandler
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

    private TcpClient _tcpClient;
    private NetworkStream _networkStream;
    private string _ipAddress;
    private int _port;
    private int _reconnectAttempts = 10;
    private CancellationTokenSource _cancellationTokenSource;

    public event EventHandler<ConnectionStatus> ConnectionStatusChanged;
    public event EventHandler<byte[]> CharacteristicOnValueUpdated;

    public async Task ConnectAsync(string ipAddress, int port)
    {
        _ipAddress = ipAddress;
        _port = port;
        _tcpClient = new TcpClient();

        try
        {
            OnConnectionStatusChanged(ConnectionStatus.Connecting);
            await _tcpClient.ConnectAsync(ipAddress, port);
            _networkStream = _tcpClient.GetStream();
            OnConnectionStatusChanged(ConnectionStatus.Connected);
        }
        catch
        {
            OnConnectionStatusChanged(ConnectionStatus.Disconnected);
        }
    }

    public async Task ReconnectAsync()
    {
        for (int i = 0; i < _reconnectAttempts; i++)
        {
            try
            {
                OnConnectionStatusChanged(ConnectionStatus.Reconnecting);
                await ConnectAsync(_ipAddress, _port);
                return;
            }
            catch
            {
                await Task.Delay(1000); // Wait before retrying
            }
        }
        OnConnectionStatusChanged(ConnectionStatus.ReconnectFail);
        await DisconnectAsync();
    }

    public async Task TCP_Send(byte[] data)
    {
        if (_networkStream == null || !_tcpClient.Connected)
        {
            OnConnectionStatusChanged(ConnectionStatus.SendFail);
            await ReconnectAsync();
        }

        try
        {
            OnConnectionStatusChanged(ConnectionStatus.Sending);
            await _networkStream.WriteAsync(data, 0, data.Length);
        }
        catch
        {
            OnConnectionStatusChanged(ConnectionStatus.SendFail);
            await ReconnectAsync();
            if (_tcpClient.Connected)
            {
                await TCP_Send(data);
            }
            else
            {
                await DisconnectAsync();
            }
        }
    }

    public async Task TCP_Recv(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        byte[] buffer = new byte[1024];

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);
                if (bytesRead > 0)
                {
                    OnConnectionStatusChanged(ConnectionStatus.Received);
                    byte[] data = new byte[bytesRead];
                    Array.Copy(buffer, data, bytesRead);
                    CharacteristicOnValueUpdated?.Invoke(this, data);
                }
            }
            catch
            {
                OnConnectionStatusChanged(ConnectionStatus.ReceiveFail);
                await ReconnectAsync();
            }
        }
    }

    public async Task DisconnectAsync()
    {
        _cancellationTokenSource?.Cancel();
        _networkStream?.Close();
        _tcpClient?.Close();
        OnConnectionStatusChanged(ConnectionStatus.ConnectionClosed);
    }

    private void OnConnectionStatusChanged(ConnectionStatus status)
    {
        ConnectionStatusChanged?.Invoke(this, status);
    }
}
