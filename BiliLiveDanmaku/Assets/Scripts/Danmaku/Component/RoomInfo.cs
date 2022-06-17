using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLibGame;

public class RoomInfo : MonoBehaviour
{
    public Text roomText;

    private void Awake()
    {
        roomText ??= GetComponent<Text>();
        EventDispatcher.GetInstance().AddListener("ROOM_INFO_UPDATE", OnRoomUpdate);
    }

    private void OnDestroy()
    {
        EventDispatcher.GetInstance().RemoveListener("ROOM_INFO_UPDATE", OnRoomUpdate);
    }

    private void OnRoomUpdate(EventContext context)
    {
        var dict = (BiliLiveRoomInfo)context.args[0];
        roomText.text = dict.roomTitle;
    }
}
