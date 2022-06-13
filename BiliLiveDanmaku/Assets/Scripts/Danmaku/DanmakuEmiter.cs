using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLibGame;

public class DanmakuEmiter : MonoBehaviour
{
    public GameObject tmplPrefab;
    public Transform parentNode;

    public float emitInterval = 0;
    public float emitMaxCount = 20;

    GameObjectPool _tmplPool;
    Queue<object> _msgQueue = new Queue<object>();
    float _emitTick;

    private void Awake()
    {
        if (tmplPrefab != null)
        {
            _tmplPool = GameObjectPoolManager.GetInstance().NewPool(tmplPrefab.name, tmplPrefab);
            _tmplPool.maxCount = 80;
            _tmplPool.minCount = 10;
        }
    }
    public void Receive(object msg)
    {
        if (msg == null)
            return;

        _msgQueue.Enqueue(msg);
    }

    public void Clear()
    {
        _msgQueue.Clear();
    }

    public void EmiterObject(object args)
    {
        var emitGo = _tmplPool.GetOrCreate();
        var tmplComp = emitGo.GetComponent<DanmakuObject>();
        if (tmplComp == null) tmplComp = emitGo.AddComponent<DanmakuObject>();

        tmplComp.ownEmiter = this;
        tmplComp.Emit(args);

        if (parentNode != null)
            emitGo.transform.SetParent(parentNode, false);  //动态生成位置不对,不使世界空间
    }

    public void ReleaseObject(DanmakuObject tmpl)
    {
        if (tmpl == null)
            return;

        _tmplPool.Release(tmpl.gameObject);
    }

    void UpdateQueue()
    {
        if (_msgQueue.Count <= 0)
            return;

        if (_emitTick + emitInterval <= Time.realtimeSinceStartup)
            return;

        //各项发射参数
        var maxNum = Mathf.Min(_msgQueue.Count, emitMaxCount);
        if (emitMaxCount < 0)
            maxNum = _msgQueue.Count;

        for (int i = 0; i < maxNum; i++)
        {
            var args = _msgQueue.Dequeue();
            EmiterObject(args);
        }

        _emitTick = Time.realtimeSinceStartup;
    }

    void Update()
    {
        UpdateQueue();
    }
}
