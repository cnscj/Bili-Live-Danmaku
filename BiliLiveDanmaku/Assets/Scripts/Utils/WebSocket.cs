using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class WebSocket
{
    public Action onOpen;
    public Action onClose;
    public Action<byte[]> onMessage;

    public const int RECEIVE_BUFF_SIZE = 2048;

    ClientWebSocket _ws;
    CancellationToken _ct;
    bool _isConnected;
    Queue<byte[]> _dataQueue = new Queue<byte[]>();

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
        List<byte> retBuff = new List<byte>();
        while (_isConnected)
        {
            retBuff.Clear();
            bool isEndOfMessage;
            do
            {
                var buffer = new ArraySegment<byte>(new byte[RECEIVE_BUFF_SIZE]);
                var result = await _ws.ReceiveAsync(buffer, new CancellationToken());//接收数据

                retBuff.AddRange(new ArraySegment<byte>(buffer.Array,0,result.Count));
                isEndOfMessage = result.EndOfMessage;

            } while (!isEndOfMessage);
            _dataQueue.Enqueue(retBuff.ToArray());

            //通知有新的消息
            Notify();
        }
    }

    private void Notify()
    {
        //Task.Run(() =>
        //{
            var data = _dataQueue.Dequeue();
            OnMessage(data);
        //});
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
