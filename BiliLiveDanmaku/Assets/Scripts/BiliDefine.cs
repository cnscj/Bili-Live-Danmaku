
public class BiliLiveDefine
{

    public static readonly string ROOM_INIT_URL = "https://api.live.bilibili.com/xlive/web-room/v1/index/getInfoByRoom";
    public static readonly string DANMAKU_SERVER_CONF_URL = "https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo";

    public static readonly int HEART_BEAT_PACKET_SEND_INTERVAL = 30 * 1000;    //心跳包发送间隔
}

public enum WSMessage
{
    WS_OP_HEARTBEAT = 2, //心跳
    WS_OP_HEARTBEAT_REPLY = 3, //心跳回应 
    WS_OP_MESSAGE = 5, //弹幕,消息等
    WS_OP_USER_AUTHENTICATION = 7,//用户进入房间
    WS_OP_CONNECT_SUCCESS = 8, //进房回应
    WS_PACKAGE_HEADER_TOTAL_LENGTH = 16,//头部字节大小
    WS_PACKAGE_OFFSET = 0,
    WS_HEADER_OFFSET = 4,
    WS_VERSION_OFFSET = 6,
    WS_OPERATION_OFFSET = 8,
    WS_SEQUENCE_OFFSET = 12,
    WS_BODY_PROTOCOL_VERSION_NORMAL = 0,//普通消息
    WS_BODY_PROTOCOL_VERSION_BROTLI = 3,//brotli压缩信息
    WS_HEADER_DEFAULT_VERSION = 1,
    WS_HEADER_DEFAULT_OPERATION = 1,
    WS_HEADER_DEFAULT_SEQUENCE = 1,
    WS_AUTH_OK = 0,
    WS_AUTH_TOKEN_ERROR = -101
}

public enum BLProtocol
{
    JSON = 0,
    RQ = 1,
    ZIP_BUFFER = 2,
    ZIP_brotli = 3,
}

public enum BLOperation
{
    HeartBeat = 2,
    HeartBeat_Resp = 3,
    NOtification = 5,
    EnterRoom = 7,
    EnterRoom_Resp = 8
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

//数据包头部数据
//https://github.com/lovelyyoshino/Bilibili-Live-API/blob/master/API.WebSocket.md
//最后需要转为一个16位byte数据
public struct BiliLiveHeader
{
    public int pack_len;            //数据包长度(包含body)
    public int raw_header_size;     //数据包头部长度（固定为 16）
    public int ver;                 //协议版本（见下文）
    public int operation;           //操作类型（见下文）
    public int seq_id;              //数据包头部长度（固定为 1）
}
