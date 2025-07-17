using System.Collections.Generic;
using UnityEngine;

public class NetManager : MonoBehaviour
{
    NetClient m_NetClient;
    // 消息队列
    Queue<KeyValuePair<int, string>> m_MessageQueue = new Queue<KeyValuePair<int, string>>();
    // 接收消息发送给Lua
    XLua.LuaFunction ReceiveMessage;

    public void Init()
    {   // 如果使用Awake或者Start初始化，Lua还没初始好，会报错
        m_NetClient = new NetClient();
        ReceiveMessage = Manager.Lua.LuaEnv.Global.Get<XLua.LuaFunction>("ReceiveMessage");
    }

    // 发送消息
    public void SendMessage(int msgID, string msg)
    {
        m_NetClient.SendMessage(msgID, msg);
    }

    // 链接服务器
    public void OnConnectServer(string host, int port)
    {
        m_NetClient.OnConnectServer(host, port);
    }

    // 网络链接
    public void OnNetConnected()
    {
       
    }

    // 被服务器断开链接
    public void OnDisConnected()
    {

    }

    // 接收到数据
    public void Receive(int msgId, string msg)
    {
        m_MessageQueue.Enqueue(new KeyValuePair<int, string>(msgId, msg));
    }

    private void Update()
    {
        if (m_MessageQueue.Count > 0)
        {
            KeyValuePair<int, string> msg = m_MessageQueue.Dequeue();
            ReceiveMessage?.Call(msg.Key, msg.Value);
        }
    }
}
