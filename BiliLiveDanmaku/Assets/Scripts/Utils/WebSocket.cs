using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class WebSocket
{
    ClientWebSocket _ws;
    CancellationToken _ct;
    bool _isConnected;

    public Action onOpen;
    public Action onClose;
    public Action<string> onMessage;

    public async void Connect(string addr)
    {
        try
        {
            Disconnect();
            _ws = new ClientWebSocket();
            Uri url = new Uri(addr);
            await _ws.ConnectAsync(url, _ct);
            OnOpen();

            LoopReceive();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async void Send(string msg)
    {
        await _ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)), WebSocketMessageType.Text, true, _ct); //发送数据
    }

    public async void Send(byte[] data)
    {
        await _ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, _ct); //发送数据
    }

    public async void Disconnect()
    {
        if (_ws != null)
        {
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", _ct);
            OnClose();
            _ws = null;
        }
    }

    private async void LoopReceive()
    {
        while (_isConnected)
        {
            var result = new byte[1024];
            await _ws.ReceiveAsync(new ArraySegment<byte>(result), new CancellationToken());//接受数据
            var str = Encoding.UTF8.GetString(result, 0, result.Length);
            OnMessage(str);
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
    protected void OnMessage(string message)
    {
        Debug.Log(message);
        onMessage?.Invoke(message);
    }
}