using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System;
using LitJson;

public class BiliLiveClient
{
    int _roomId;
    BiliLiveRoomInfo _roomInfo;
    BiliLiveHostInfo _hostInfo;

    bool _isRunning;
    WebSocket _websocket = new WebSocket();

    public BiliLiveClient(int roomId)
    {
        _roomId = roomId;
    }

    public bool IsRunning()
    {
        return _isRunning;
    }

    public void Start()
    {
        if (_isRunning) return;
        _isRunning = true;

        InitRoomInfo();
        InitHostServer();

        ConnectRoom();
    }

    public void Close()
    {
        DisconnectRoom();
        _isRunning = false;
    }

    ////////////////////////

    private void InitRoomInfo()
    {
        try
        {
            var jsonStr = HttpTool.Get(BiliLiveDefine.ROOM_INIT_URL, new Dictionary<string, string> { ["room_id"] = _roomId.ToString() });
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

    private void InitHostServer()
    {
        try
        {
            var jsonStr = HttpTool.Get(BiliLiveDefine.DANMAKU_SERVER_CONF_URL, new Dictionary<string, string> { ["id"] = _roomId.ToString(),["type"] = "0" });
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

    private void ConnectRoom()
    {
        //建立WebSocket链接,这里应该对每个进行尝试
        int retryCount = 0;
        foreach(var hostData in _hostInfo.hostList)
        {
            string url = string.Format("wss://{0}:{1}/sub", hostData.host, hostData.wssPort);
            _websocket.Connect(url);
            break;
        }
    }

    private void DisconnectRoom()
    {
        _websocket.Disconnect();
    }

    //心跳包
    private void MakePacket()
    {

    }
}
