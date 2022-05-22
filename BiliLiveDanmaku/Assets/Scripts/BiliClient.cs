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
    IntervalTimer _heartbeatTimer = new IntervalTimer(30 * 1000);
    public BiliLiveClient(int roomId)
    {
        _roomId = roomId;
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
            await ConnectWebscoket("wss", hostData.host, hostData.wssPort);
            await SendAuthPacket();
            KeepConnect();

            break;
        }
    }

    private async Task DisconnectRoom()
    {
        await _websocket.Disconnect();
    }

    private void KeepConnect()
    {
        _heartbeatTimer.Event(()=>
        {
            SendTiktokPacket();
        });
        //_tiktokTimer.Start();
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
        authArgs["uid"] = _roomInfo.roomOwnerUid;
        authArgs["roomid"] = _roomInfo.realRoomId;
        authArgs["protover"] = 3;
        authArgs["platform"] = "web";
        authArgs["type"] = 2;
        authArgs["key"] = _hostInfo.token;

        var data = MakePacket(authArgs);
        await _websocket.Send(data);
    }

    //心跳包,空的用来维持链接
    private async Task SendHeartbeatPacket()
    {
        var emptyArgs = new Dictionary<string, object>();

        var data = MakePacket(emptyArgs);
        await _websocket.Send(data);
    }


    //创建一个信息包二进制数据
    private byte[] MakePacket(Dictionary<string, object> args)
    {
        //TODO:缺少头部数据
        var jsonStr = JsonMapper.ToJson(args);
        var data = Encoding.UTF8.GetBytes(jsonStr);
        return data;
    }

}
