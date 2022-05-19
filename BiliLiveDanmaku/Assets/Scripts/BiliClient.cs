using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System;
using LitJson;

public class BiliLiveClient
{
    static readonly string ROOM_INIT_URL = "https://api.live.bilibili.com/xlive/web-room/v1/index/getInfoByRoom";
    static readonly string DANMAKU_SERVER_CONF_URL = "https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo";

    int _roomId;

    int _realRoomId;
    int _shortRoomId;
    string _roomTitle;
    int _roomOwnerUid;


    string _hostListHost;
    int _hostListPort;
    int _hostListWsPort;

    string _token;

    bool _isRunning;

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
            var jsonStr = HttpTool.Get(ROOM_INIT_URL, new Dictionary<string, string> { ["room_id"] = _roomId.ToString() });
            var jsonData = JsonMapper.ToObject(jsonStr);

            var codeStr = jsonData["code"].ToString();
            if (codeStr == "0")
            {
                var room_info = jsonData["data"]["room_info"];

                _realRoomId = int.Parse(room_info["room_id"].ToString());
                _shortRoomId = int.Parse(room_info["short_id"].ToString());
                _roomTitle = room_info["title"].ToString();
                _roomOwnerUid = int.Parse(room_info["uid"].ToString());

                Debug.LogFormat("room={0},title={1},uid={2}", _realRoomId, _roomTitle, _roomOwnerUid);
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
            var jsonStr = HttpTool.Get(DANMAKU_SERVER_CONF_URL, new Dictionary<string, string> { ["id"] = _roomId.ToString(),["type"] = "0" });
            var jsonData = JsonMapper.ToObject(jsonStr);

            var codeStr = jsonData["code"].ToString();
            if (codeStr == "0")
            {
                var data = jsonData["data"];
                _token = data["token"].ToString();

                var host_list = data["host_list"];  //XXX:多个host,理论上应该逐个尝试
                var host_list_one = host_list[1];

                _hostListHost = host_list_one["host"].ToString();
                _hostListPort = int.Parse(host_list_one["port"].ToString());
                _hostListWsPort = int.Parse(host_list_one["ws_port"].ToString());

                Debug.LogFormat("host={0},port={1},ws_port={2}", _hostListHost, _hostListPort, _hostListWsPort);
                Debug.LogFormat("token={0}", _token);
            }
        }
        catch (Exception)
        {

        }
    }

    private void ConnectRoom()
    {
        //TODO:建立WebSocket链接
    }

    private void DisconnectRoom()
    {
            
    }

    //心跳包
    private void MakePacket()
    {

    }
}
