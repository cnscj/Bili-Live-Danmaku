using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ClientTest : MonoBehaviour
{
    // Start is called before the first frame update
    BiliLiveClient client = new BiliLiveClient(22625025);
    void Start()
    {
        client.Start();

        //var client = new WebSocket();
        //client.onOpen = () =>
        //{
        //    client.Send("Hellow");
        //};
        //client.Connect("ws://localhost:8080/");

        //Debug.Log(System.Runtime.InteropServices.Marshal.SizeOf(new BiliLiveHeader()));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        client.Close();
    }
}
