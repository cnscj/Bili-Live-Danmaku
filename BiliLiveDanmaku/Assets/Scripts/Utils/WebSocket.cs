using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WebSocket
{
    ClientWebSocket _ws;
    CancellationToken _ct;
    bool _isConnected;

    public Action onOpen;
    public Action onClose;
    public Action<byte[]> onMessage;

    public async Task Connect(string addr)
    {
        await Disconnect();
        await CreateConnect(addr);

        LoopReceive();
    }

    public async void Send(string msg)
    {
        await _ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)), WebSocketMessageType.Text, true, _ct); //发送数据
    }

    public async Task Send(byte[] data)
    {
        await _ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, _ct); //发送数据
    }

    public async Task Disconnect()
    {
        if (_ws != null)
        {
            OnClose();
            if (_ws.State != WebSocketState.Closed)
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnect", _ct);
            }
            _ws = null;
        }
    }

    private async Task CreateConnect(string addr)
    {
        _ws = new ClientWebSocket();
        Uri url = new Uri(addr);
        await _ws.ConnectAsync(url, _ct);
        OnOpen();
    }

    private async void LoopReceive()
    {
        while (_isConnected)
        {
            var result = new byte[2048];
            await _ws.ReceiveAsync(new ArraySegment<byte>(result), new CancellationToken());//接受数据

            OnMessage(result);
        }
    }

    //
    protected void OnOpen()
    {
        _isConnected = true;
        onOpen?.Invoke();
    }

    protected void OnClose()
    {
        _isConnected = false;
        onClose?.Invoke();
    }
    protected void OnMessage(byte[] data)
    {
        onMessage?.Invoke(data);
    }
}
