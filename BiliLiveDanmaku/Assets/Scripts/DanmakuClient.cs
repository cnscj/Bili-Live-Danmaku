
using System;
using System.Collections.Generic;
using LitJson;
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

        _client.onRoomMsg = OnRoomMsg;
        _client.onDanmakuMsg = OnDanmakuMsg;
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

    private void OnRoomMsg(string jsonStr)
    {
        try
        {
            var jsonData = JsonMapper.ToObject(jsonStr);
            var codeStr = jsonData["code"].ToString();
            if (codeStr == "0")
            {
                var room_info = jsonData["data"]["room_info"];
                var room_dict = new Dictionary<string, object>()
                {
                    ["longRoomId"] = int.Parse(room_info["room_id"].ToString()),
                    ["shortRoomId"] = int.Parse(room_info["short_id"].ToString()),
                    ["roomTitle"] = room_info["title"].ToString(),
                    ["roomOwnerUid"] = int.Parse(room_info["uid"].ToString()),
                };
                EventDispatcher.GetInstance().Dispatch("ROOM_INFO_UPDATE", room_dict);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("Parse Error:\r\n{0}\r\n{1}", jsonStr, e.ToString()));
        }
    }
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

                var text = string.Format("{0}", content);
                Send(text);

                Debug.Log(string.Format("{0}:{1}", nick, content));

            }
            else if (cmd == BiliLiveDanmakuCmd.SEND_GIFT)
            {

            }

        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("Parse Error:\r\n{0}\r\n{1}", jsonStr, e.ToString()));
        }
    }
}
