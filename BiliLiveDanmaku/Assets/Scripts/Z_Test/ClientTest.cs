using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var client = new BiliLiveClient(22625025);
        client.Start();

        //var client = new WebSocket();
        //client.onOpen = () =>
        //{
        //    client.Send("Hellow");
        //};
        //client.Connect("ws://localhost:8080/");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
