using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLibGame;

public class RoomInfo : MonoBehaviour
{
    public Text roomText;
    void Start()
    {
        roomText ??= GetComponent<Text>();
    }

    private void OnRoomUpdate(EventContext context)
    {
        var dict = (Dictionary<string,object>)context.args[0];
        roomText.text = dict["roomTitle"].ToString();
    }

    private void Awake()
    {
        EventDispatcher.GetInstance().AddListener("ROOM_INFO_UPDATE", OnRoomUpdate);
    }

    private void OnDestroy()
    {
        EventDispatcher.GetInstance().RemoveListener("ROOM_INFO_UPDATE", OnRoomUpdate);
    }

}
