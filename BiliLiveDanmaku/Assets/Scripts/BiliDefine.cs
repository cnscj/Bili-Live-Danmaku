
public class BiliLiveDefine
{
    public static readonly string ROOM_INIT_URL = "https://api.live.bilibili.com/xlive/web-room/v1/index/getInfoByRoom";
    public static readonly string DANMAKU_SERVER_CONF_URL = "https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo";
}

public struct BiliLiveRoomInfo
{
    public int realRoomId;
    public int shortRoomId;
    public string roomTitle;
    public int roomOwnerUid;
}

public struct BiliLiveHostInfoHostData
{
    public string host;
    public int port;
    public int wsPort;
    public int wssPort;
}


public struct BiliLiveHostInfo
{
    public string token;
    public BiliLiveHostInfoHostData[] hostList;
}
