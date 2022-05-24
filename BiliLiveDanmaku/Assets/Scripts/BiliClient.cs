using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System;
using LitJson;
using System.Text;

public class BiliLiveClient
{
    int _roomId;
    BiliLiveRoomInfo _roomInfo;
    BiliLiveHostInfo _hostInfo;

    bool _isRunning;
    WebSocket _websocket = new WebSocket();
    IntervalTimer _heartbeatTimer = new IntervalTimer(BiliLiveDefine.HEART_BEAT_PACKET_SEND_INTERVAL);
    public BiliLiveClient(int roomId)
    {
        _roomId = roomId;
        _heartbeatTimer.onEvent = OnTimerEvent;
        _websocket.onMessage = OnWebsocketMessage;
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
            var jsonStr = await HttpRequest.GetAsync(BiliLiveDefine.ROOM_INIT_URL, new Dictionary<string, string> { ["room_id"] = _roomId.ToString() });
            var jsonData = JsonMapper.ToObject(jsonStr);

            var codeStr = jsonData["code"].ToString();
            if (codeStr == "0")
            {
                var room_info = jsonData["data"]["room_info"];

                _roomInfo.realRoomId = int.Parse(room_info["room_id"].ToString());
                _roomInfo.shortRoomId = int.Parse(room_info["short_id"].ToString());
                _roomInfo.roomTitle = room_info["title"].ToString();
                _roomInfo.roomOwnerUid = int.Parse(room_info["uid"].ToString());

                Debug.LogFormat("room={0},title={1},uid={2}", _roomInfo.realRoomId, _roomInfo.roomTitle, _roomInfo.roomOwnerUid);
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
            var jsonStr = await HttpRequest.GetAsync(BiliLiveDefine.DANMAKU_SERVER_CONF_URL, new Dictionary<string, string> { ["id"] = _roomId.ToString(),["type"] = "0" });
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
            await ConnectWebscoket("ws", hostData.host, hostData.wsPort);
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
        authArgs["roomid"] = _roomInfo.realRoomId;
        //authArgs["protover"] = 3;
        //authArgs["platform"] = "web";
        //authArgs["type"] = 2;
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

    private void ParsePackData(byte[] data)
    {
        UnpackageData(data, out var outHeader, out var outBody);
        var str = Encoding.UTF8.GetString(outBody, 0, (int)outHeader.pack_len - (int)outHeader.raw_header_size);

        if (outHeader.operation == (uint)BiliLiveCode.WS_OP_HEARTBEAT_REPLY)
        {
            //好像什么都没返回
        }
        else if (outHeader.operation == (uint)BiliLiveCode.WS_OP_MESSAGE)
        {
            if (outHeader.ver == (uint)BiliLiveCode.WS_BODY_PROTOCOL_VERSION_NORMAL) //JSON明文
            {
                ParseDanmakuMsg(str);
            }
            else if (outHeader.ver == (uint)BiliLiveCode.WS_BODY_PROTOCOL_VERSION_DEFLATE)
            {
                //需要剥离头部信息
                var newData = ZipUtility.Decompress_Deflate(outBody);
                ParsePackData(newData);
            }
        }
        else if (outHeader.operation == (uint)BiliLiveCode.WS_OP_CONNECT_SUCCESS)
        {
            //JSON数据
            //{"code":0}
        }
    }

    private void ParseDanmakuMsg(string jsonStr)
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
            else if(cmd == BiliLiveDanmakuCmd.SEND_GIFT)
            {
                


            }
            //Debug.Log(jsonStr);
        }
        catch(Exception _)
        {

        }
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
    private void UnpackageData(byte[] data, out BiliLiveHeader outHader, out byte[] outBody)
    {
        var header = new byte[16];
        var body = new byte[2048];

        ByteHelper.SpliteBytes(data, header, body);

        object blHeader = new BiliLiveHeader();
        ByteHelper.ByteArrayToStructureEndian(header, ref blHeader, 0);

        outHader = (BiliLiveHeader)blHeader;
        outBody = body;
    }

    
    //
    private async void OnTimerEvent()
    {
        await SendHeartbeatPacket();
    }

    private void OnWebsocketMessage(byte[] data)
    {
        //TODO:粘包问题,如果body的长度不合理,需要根据header头部的len拼接数据
        //TODO:有时候2个包当成1个包发送

        UnpackageData(data, out var outHeader, out var _);
        //Debug.LogFormat("{0},{1},{2}", outHeader.pack_len, outHeader.operation, outHeader.ver);

        ParsePackData(data);
    }
}
