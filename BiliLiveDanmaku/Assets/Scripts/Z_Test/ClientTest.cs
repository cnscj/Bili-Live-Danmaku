
using UnityEngine;

public class ClientTest : MonoBehaviour
{
    //# 22634198,     #乐
    //# 22625025,     #晚
    //# 22625027      #琳,
    //# 22632424      #拉,
    //# 22637261      #然,
    BiliLiveClient client = new BiliLiveClient(22632424);
    void Start()
    {
        client.Start();
    }

    private void OnDestroy()
    {
        client.Close();
    }
}
