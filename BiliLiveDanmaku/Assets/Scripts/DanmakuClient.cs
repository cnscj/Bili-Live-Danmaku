
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XLibGame;

public class DanmakuClient : MonoBehaviour
{
    //# 22634198,     #乐
    //# 22625025,     #晚
    //# 22625027      #琳,
    //# 22632424      #拉,
    //# 22637261      #然,
    public enum RoomWho
    {
        Custom = 0,
        Ava = 22625025,
        Bella = 22632424,
        Carol = 22634198,
        Diana = 22637261,
        Eileen = 22625027,
    }
    public RoomWho whosRoom;
    public int roomId;

    public UnityEvent<string> msgEvent = new UnityEvent<string>();

    BiliLiveClient _client = new BiliLiveClient();
    void Start()
    {
        int finalRoom = roomId;
        if (whosRoom != RoomWho.Custom)
        {
            finalRoom = (int)whosRoom;
        }

        _client.listener.Clear();
        _client.listener.onRoomInfo = OnRoomInfo;
        _client.listener.onDataDanmuMsg = OnDataDanmuMsg;

        _client.Start(finalRoom);
    }

    private void OnDestroy()
    {
        _client.Close();
    }

    private void Send(string msg)
    {
        msgEvent.Invoke(msg);
    }

    private void OnRoomInfo(BiliLiveRoomInfo roomInfo)
    {
        EventDispatcher.GetInstance().Dispatch("ROOM_INFO_UPDATE", roomInfo);
    }

    private void OnDataDanmuMsg(BiliLiveDanmakuData.DanmuMsg danmuMsg)
    {
        var text = string.Format("{0}", danmuMsg.content);
        Send(text);

        Debug.Log(string.Format("{0}:{1}", danmuMsg.nick, danmuMsg.content));
    }
   
}
