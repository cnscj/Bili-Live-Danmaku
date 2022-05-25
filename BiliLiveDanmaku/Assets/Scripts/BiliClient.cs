
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;
using LitJson;
using System.Text;

public class BiliLiveClient
{
    public Action<string> onDanmakuMsg;

    int _roomId;
    bool _isRunning;

    BiliLiveRoomInfo _roomInfo;
    BiliLiveHostInfo _hostInfo;

    WebSocket _websocket = new WebSocket();
    IntervalTimer _heartbeatTimer = new IntervalTimer(BiliLiveDef.HEART_BEAT_PACKET_SEND_INTERVAL);

    Stack<byte[]> _tempBuffs = new Stack<byte[]>();

    public BiliLiveClient(int roomId)
    {
        _roomId = roomId;
        _heartbeatTimer.onEvent = OnTimerEvent;
        _websocket.onMessage = OnWebsocketMessage;
        onDanmakuMsg = OnDanmakuMsg;
    }

    public bool IsRunning()
    {
        return _isRunning;
    }

    public async void Start()
    {
        if (_isRunning) return;
        _isRunning = true;

        await InitRoomInfo();
        await InitHostServer();

        await ConnectRoom();
    }

    public async void Close()
    {
        StopKeepConnect();
        await DisconnectRoom();
        _isRunning = false;
    }

    ////////////////////////

    private async Task InitRoomInfo()
    {
        try
        {
            var jsonStr = await HttpRequest.GetAsync(BiliLiveDef.ROOM_INIT_URL, new Dictionary<string, string> { ["room_id"] = _roomId.ToString() });
            var jsonData = JsonMapper.ToObject(jsonStr);

            var codeStr = jsonData["code"].ToString();
            if (codeStr == "0")
            {
                var room_info = jsonData["data"]["room_info"];

                _roomInfo.longRoomId = int.Parse(room_info["room_id"].ToString());
                _roomInfo.shortRoomId = int.Parse(room_info["short_id"].ToString());

                _roomInfo.roomTitle = room_info["title"].ToString();
                _roomInfo.roomOwnerUid = int.Parse(room_info["uid"].ToString());

                _roomInfo.finalRoomId = (_roomInfo.shortRoomId != 0) ? _roomInfo.shortRoomId : _roomInfo.longRoomId;

                Debug.LogFormat("room_id={0},short_id={1},title={2}", _roomInfo.longRoomId, _roomInfo.shortRoomId, _roomInfo.roomTitle);
            }
        }
        catch(Exception)
        {

        }

    }

    private async Task InitHostServer()
    {
        try
        {
            var jsonStr = await HttpRequest.GetAsync(BiliLiveDef.DANMAKU_SERVER_CONF_URL, new Dictionary<string, string> { ["id"] = _roomId.ToString(),["type"] = "0" });
            var jsonData = JsonMapper.ToObject(jsonStr);

            var codeStr = jsonData["code"].ToString();
            if (codeStr == "0")
            {
                var data = jsonData["data"];
                _hostInfo.token = data["token"].ToString();

                var host_list = data["host_list"];
                _hostInfo.hostList = new BiliLiveHostInfoHostData[host_list.Count];

                for(int i = 0;i < host_list.Count;i++)
                {
                    var host_data = host_list[i];
                    var hostData = new BiliLiveHostInfoHostData();
                    hostData.host = host_data["host"].ToString();
                    hostData.port = int.Parse(host_data["port"].ToString());
                    hostData.wsPort = int.Parse(host_data["ws_port"].ToString());
                    hostData.wssPort = int.Parse(host_data["wss_port"].ToString());

                    _hostInfo.hostList[i] = hostData;
                }

                Debug.LogFormat("token={0}", _hostInfo.token);
            }
        }
        catch (Exception)
        {

        }
    }

    private async Task ConnectRoom()
    {
        //建立WebSocket链接,这里应该对每个进行尝试
        foreach(var hostData in _hostInfo.hostList)
        {
            await ConnectWebscoket("wss", hostData.host, hostData.wssPort);
            await SendAuthPacket();
            KeepConnect();

            break;
        }
    }

    private void StopKeepConnect()
    {
        _heartbeatTimer.Stop();
    }

    private async Task DisconnectRoom()
    {
        await _websocket.Disconnect();
    }

    private void KeepConnect()
    {

        _heartbeatTimer.Start();
    }

    private async Task ConnectWebscoket(string proto,string host,int port)
    {
        string url = string.Format("{0}://{1}:{2}/sub", proto, host, port);
        await _websocket.Connect(url);
    }

    //发送认证包
    private async Task SendAuthPacket()
    {
        var authArgs = new Dictionary<string, object>();
        //XXX:应该不是房主自己,这里传太多参数会导致链接失败

        //authArgs["uid"] = 0;//_roomInfo.roomOwnerUid;
        //authArgs["protover"] = 3;
        //authArgs["platform"] = "web";
        //authArgs["type"] = 2;   
        authArgs["roomid"] = _roomInfo.finalRoomId;
        authArgs["key"] = _hostInfo.token;

        var data = MakePackData(authArgs, (uint)BiliLiveCode.WS_OP_USER_AUTHENTICATION);
        await _websocket.Send(data);
    }

    //心跳包,空的用来维持链接
    private async Task SendHeartbeatPacket()
    {
        var emptyArgs = new Dictionary<string, object>();

        var data = MakePackData(emptyArgs, (uint)BiliLiveCode.WS_OP_HEARTBEAT);
        await _websocket.Send(data);
    }

    private byte[] MakePackData(Dictionary<string, object> args, uint operation)
    {
        var jsonStr = JsonMapper.ToJson(args);
        var body = Encoding.UTF8.GetBytes(jsonStr);

        return PackageData(body, operation);
    }

    private void ParsePacketData(byte[] data)
    {
        UnpackageData(data, out var outHeader, out var outBody);

        //会出现pack_len>outBody.Length的问题
        if (outHeader.operation == (uint)BiliLiveCode.WS_OP_HEARTBEAT_REPLY)
        {
            //好像什么都没返回
        }
        else if (outHeader.operation == (uint)BiliLiveCode.WS_OP_MESSAGE)
        {
            if (outHeader.ver == (uint)BiliLiveCode.WS_BODY_PROTOCOL_VERSION_NORMAL) //JSON明文
            {
                var str = Encoding.UTF8.GetString(outBody, 0, (int)outHeader.pack_len - (int)outHeader.raw_header_size);
                onDanmakuMsg?.Invoke(str);
            }
            else if (outHeader.ver == (uint)BiliLiveCode.WS_BODY_PROTOCOL_VERSION_DEFLATE)
            {
                //需要剥离头部信息
                var newData = ZipUtility.Decompress_Deflate(outBody);   //TODO:解码后长度会变大,这里也会发生粘包现象
                ParsePacketData(newData);   //TODO:可能多个包粘一起
            }
        }
        else if (outHeader.operation == (uint)BiliLiveCode.WS_OP_CONNECT_SUCCESS)
        {
            try
            {
                var str = Encoding.UTF8.GetString(outBody, 0, (int)outHeader.pack_len - (int)outHeader.raw_header_size);
                var jsonData = JsonMapper.ToObject(str);
                var code = int.Parse(jsonData["code"].ToString());
                if (code != 0)
                {
                    Debug.LogError("Connect Error");
                }
            }
            catch (Exception)
            {
                Debug.LogError("Parse Error");
            }
        }
    }

    //弹幕类解析
    private void OnDanmakuMsg(string jsonStr)
    {
        try
        {
            var jsonData = JsonMapper.ToObject(jsonStr);
            var cmd = jsonData["cmd"].ToString();

            if (cmd == BiliLiveDanmakuCmd.DANMU_MSG)
            {
                var info = jsonData["info"];
                var uid = info[2][0].ToString();
                var nick = info[2][1].ToString();
                var content = info[1].ToString();

                Debug.LogFormat("{0}:{1}", nick, content);

            }
            else if (cmd == BiliLiveDanmakuCmd.SEND_GIFT)
            {

            }

        }
        catch (Exception)
        {
            Debug.LogError("Parse Error");
        }
    }

    //
    private BiliLiveHeader DecodePacketHeader(byte[] data)
    {
        var headerData = new byte[16];
        Array.Copy(data, 0, headerData, 0, headerData.Length);
        object blHeader = new BiliLiveHeader();
        ByteHelper.ByteArrayToStructureEndian(headerData, ref blHeader, 0);
        var header = (BiliLiveHeader)blHeader;

        return header;
    }
    //封装一个数据包
    private byte[] PackageData(byte[] body, uint operation)
    {
        var blHeader = new BiliLiveHeader();
        blHeader.raw_header_size = (ushort)System.Runtime.InteropServices.Marshal.SizeOf(blHeader);
        blHeader.pack_len = (uint)(blHeader.raw_header_size + body.Length);
        blHeader.ver = 1;
        blHeader.operation = operation;
        blHeader.seq_id = 1;

        var header = ByteHelper.StructureToByteArrayEndian(blHeader);

        return ByteHelper.CombineBytes(header,body);
    }

    //解析一个数据包
    private void UnpackageData(byte[] data, out BiliLiveHeader outHeader, out byte[] outBody)
    {
        var headerData = new byte[16];
        ByteHelper.SpliteBytes(data, headerData, null);

        object blHeader = new BiliLiveHeader();
        ByteHelper.ByteArrayToStructureEndian(headerData, ref blHeader, 0);
        var header = (BiliLiveHeader)blHeader;

        var body = new byte[header.pack_len - header.raw_header_size];
        ByteHelper.SpliteBytes(data, headerData, body);

        outHeader = header;
        outBody = body;
    }

    
    //
    private async void OnTimerEvent()
    {
        await SendHeartbeatPacket();
    }

    private void OnWebsocketMessage(byte[] data)
    {
        var outHeader = DecodePacketHeader(data);
        Debug.LogFormat("len={0},op={1},ver={2},seq={3}", outHeader.pack_len, outHeader.operation, outHeader.ver, outHeader.seq_id);

        //FIXME:整包过长问题
        if (_tempBuffs.Count > 0)
        {
            var finalData = new List<byte>();
            while (_tempBuffs.Count > 0)
            {
                var lastData = _tempBuffs.Pop();
                finalData.AddRange(lastData);
            }

            finalData.AddRange(data);
            data = finalData.ToArray();

            ParsePacketData(data);
        }
        else
        {
            if (outHeader.pack_len > WebSocket.RECEIVE_BUFF_SIZE)
            {
                _tempBuffs.Push(data);
            }
            else
            {
                ParsePacketData(data);
            }

        }
    }
}
