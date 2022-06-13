using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanmakuObject : MonoBehaviour
{
    public enum EmitAction
    {
        None,
        Roll,
    }

    public float lifeTime = -1;
    public EmitAction emitAction;

    //Roll参数
    public Vector3 rollSpeed = Vector3.left;

    [HideInInspector]public DanmakuEmiter ownEmiter;
    private Coroutine _releaseCor;
    public void Emit(object args)
    {
        SendMessage("OnDanmaku", args); //向所有Behaviour发送事件,
        TryCountDown();
    }

    private void OnRelease()
    {
        if (ownEmiter != null)
        {
            ownEmiter.ReleaseObject(this);
        }
    }

    private void OnDestroy()
    {
        StopCountDown();
    }
    public void TryCountDown()
    {
        if (lifeTime > 0)
        {
            StartCountDown();
        }
    }

    private void StartCountDown()
    {
        StopCountDown();
        _releaseCor = StartCoroutine(CountDown());
    }
    private void StopCountDown()
    {
        if (_releaseCor != null) StopCoroutine(_releaseCor);
        _releaseCor = null;
    }

    IEnumerator CountDown()
    {
        yield return new WaitForSeconds(lifeTime);
        OnRelease();
    }
    /////
    private void Update()
    {
        switch(emitAction)
        {
            case EmitAction.Roll:
                UpdateRoll();
            break;
        }
    }

    private void UpdateRoll()
    {
        transform.Translate(Time.deltaTime * rollSpeed);
    }

}
