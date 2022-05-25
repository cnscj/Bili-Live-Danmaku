using System;
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
            var buff = new byte[RECEIVE_BUFF_SIZE];
            var result = new ArraySegment<byte>(buff);
            await _ws.ReceiveAsync(result, new CancellationToken());//接受数据

            OnMessage(result.Array);
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
