
public class BiliLiveDefine
{

    public static readonly string ROOM_INIT_URL = "https://api.live.bilibili.com/xlive/web-room/v1/index/getInfoByRoom";
    public static readonly string DANMAKU_SERVER_CONF_URL = "https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo";

    public static readonly int HEART_BEAT_PACKET_SEND_INTERVAL = 30 * 1000;    //���������ͼ��
}

public enum WSMessage
{
    WS_OP_HEARTBEAT = 2, //����
    WS_OP_HEARTBEAT_REPLY = 3, //������Ӧ 
    WS_OP_MESSAGE = 5, //��Ļ,��Ϣ��
    WS_OP_USER_AUTHENTICATION = 7,//�û����뷿��
    WS_OP_CONNECT_SUCCESS = 8, //������Ӧ
    WS_PACKAGE_HEADER_TOTAL_LENGTH = 16,//ͷ���ֽڴ�С
    WS_PACKAGE_OFFSET = 0,
    WS_HEADER_OFFSET = 4,
    WS_VERSION_OFFSET = 6,
    WS_OPERATION_OFFSET = 8,
    WS_SEQUENCE_OFFSET = 12,
    WS_BODY_PROTOCOL_VERSION_NORMAL = 0,//��ͨ��Ϣ
    WS_BODY_PROTOCOL_VERSION_BROTLI = 3,//brotliѹ����Ϣ
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

//���ݰ�ͷ������
//https://github.com/lovelyyoshino/Bilibili-Live-API/blob/master/API.WebSocket.md
//�����ҪתΪһ��16λbyte����
public struct BiliLiveHeader
{
    public int pack_len;            //���ݰ�����(����body)
    public int raw_header_size;     //���ݰ�ͷ�����ȣ��̶�Ϊ 16��
    public int ver;                 //Э��汾�������ģ�
    public int operation;           //�������ͣ������ģ�
    public int seq_id;              //���ݰ�ͷ�����ȣ��̶�Ϊ 1��
}
