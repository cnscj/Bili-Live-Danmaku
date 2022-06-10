
using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using XLibGame;

public class DanmakuClient : MonoBehaviour
{
    //# 22634198,     #乐
    //# 22625025,     #晚
    //# 22625027      #琳,
    //# 22632424      #拉,
    //# 22637261      #然,

    public int roomId;
    public GameObject textPrefab;
    public Canvas canvas;


    GameObjectPool _textPool;
    BiliLiveClient _client = new BiliLiveClient();
    Queue<string> _textQueue = new Queue<string>();

    private void Awake()
    {

        _textPool = GameObjectPoolManager.GetInstance().NewPool("TextPool", textPrefab, 80);
    }

    void Start()
    {
        _client.onDanmakuMsg = OnDanmakuMsg;
        _client.Start(roomId);
    }

    private void OnDestroy()
    {
        _client.Close();
    }

    private void EmitText(string text)
    {
        var textGo = _textPool.GetOrCreate(5f);
        var danmakuText = textGo.GetComponent<DanmakuText>();

        danmakuText.SetText(text);
        danmakuText.transform.SetParent(canvas.transform);

    }

    private void Update()
    {
     
        while (_textQueue.Count > 0)
        {
            var text = _textQueue.Dequeue();
            EmitText(text);

            Debug.Log(text);
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

                var text = string.Format("{0}:{1}", nick, content);
                _textQueue.Enqueue(text);


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
}
