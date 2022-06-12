
using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using UnityEngine.Events;

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
    Queue<string> _textQueue = new Queue<string>();


    void Start()
    {
        int finalRoom = roomId;
        if (whosRoom != RoomWho.Custom)
        {
            finalRoom = (int)whosRoom;
        }

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

    //从子线程转回主线程处理
    private void Update()
    {
        while (_textQueue.Count > 0)
        {
            var text = _textQueue.Dequeue();
            Send(text);
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
                _textQueue.Enqueue(text);

                Debug.Log(string.Format("{0}:{1}", nick, content));

            }
            else if (cmd == BiliLiveDanmakuCmd.SEND_GIFT)
            {

            }

        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("Parse Error:{0},{1}", jsonStr,e.ToString()));
        }
    }
}
