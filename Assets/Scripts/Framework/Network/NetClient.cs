using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetClient
{
    private TcpClient m_Client;
    private NetworkStream m_TcpStream;
    private const int BufferSize = 1024 * 64;
    private byte[] m_Buffer = new byte[BufferSize];
    private MemoryStream m_MemStream;
    private BinaryReader m_BinaryReader;

    public NetClient()
    {
        m_MemStream = new MemoryStream();
        m_BinaryReader = new BinaryReader(m_MemStream);
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    public void OnConnectServer(string host, int port)
    {
        try
        {
            IPAddress[] addresses = Dns.GetHostAddresses(host);
            if (addresses.Length == 0)
            {
                Debug.LogError("host invalid");
                return;
            }
            if (addresses[0].AddressFamily != AddressFamily.InterNetworkV6)
                m_Client = new TcpClient(AddressFamily.InterNetworkV6);
            else
                m_Client = new TcpClient(AddressFamily.InterNetwork);
            m_Client.SendTimeout = 1000;
            m_Client.ReceiveTimeout = 1000;
            m_Client.NoDelay = true;
            m_Client.BeginConnect(host, port, OnConnect, null);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    /// <summary>
    /// 连接成功 接收数据
    /// </summary>
    private void OnConnect(IAsyncResult ar)
    {
        if (m_Client == null || !m_Client.Connected)
        {
            Debug.LogError("connect server error!");
            return;
        }
        Manager.Net.OnNetConnected();
        m_TcpStream = m_Client.GetStream();
        m_TcpStream.BeginRead(m_Buffer, 0, BufferSize, OnRead, null);
    }


    /// <summary>
    /// 读取数据
    /// </summary>
    private void OnRead(IAsyncResult ar)
    {
        try
        {
            if (m_Client == null || m_TcpStream == null)
                return;
            if (m_Buffer.Length < 1)
            {
                OnDisConnected();
                return;
            }
            ReceiveData();
            lock (m_TcpStream)
            {
                Array.Clear(m_Buffer, 0, m_Buffer.Length);
                m_TcpStream.BeginRead(m_Buffer, 0, BufferSize, OnRead, null);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            OnDisConnected();
        }
    }

    /// <summary>
    /// 解析接收到的数据
    /// </summary>
    private void ReceiveData()
    {
        // 移动到末尾写入数据
        m_MemStream.Seek(0, SeekOrigin.End);
        m_MemStream.Write(m_Buffer, 0, m_Buffer.Length);
        // 移动到开头
        m_MemStream.Seek(0, SeekOrigin.Begin);
        while (RemainingBytesLength() >= 8)
        {
            int msgId = m_BinaryReader.ReadInt32();
            int msgLen = m_BinaryReader.ReadInt32();
            if (RemainingBytesLength() >= msgLen)
            {
                byte[] date = m_BinaryReader.ReadBytes(msgLen);
                string message = System.Text.Encoding.UTF8.GetString(date);

                // 转到lua
                Manager.Net.Receive(msgId, message);
            }
            else
            {   // 返回读取的数据
                m_MemStream.Position = m_MemStream.Position - 8;
                break;
            }
        }
        // 剩余数据写入MemStream
        byte[] leftover = m_BinaryReader.ReadBytes(RemainingBytesLength());
        m_MemStream.SetLength(0);
        m_MemStream.Write(leftover, 0, leftover.Length);
    }

    /// <summary>
    /// 获取剩余字节长度
    /// </summary>
    private int RemainingBytesLength()
    {
        return (int)(m_MemStream.Length - m_MemStream.Position);
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    public void SendMessage(int msgID, string msg)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            ms.Position = 0;
            BinaryWriter bw = new BinaryWriter(ms);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);
            // 协议ID
            bw.Write(msgID);
            // 数据长度
            bw.Write((int)data.Length);
            // 数据内容
            bw.Write(data);
            bw.Flush();
            if (m_Client != null && m_Client.Connected)
            {
                byte[] sendData = ms.ToArray();
                m_TcpStream.BeginWrite(sendData, 0, sendData.Length, OnEndSend, null);
            }
            else
            {
                Debug.LogError("TCPClient:SendData:连接已断开");
            }
        }
    }

    private void OnEndSend(IAsyncResult ar)
    {
        try
        {
            m_TcpStream.EndWrite(ar);
        }
        catch (Exception e)
        {
            OnDisConnected();
            Debug.LogError("TCPClient:OnEndSend:" + e.Message);
        }
    }


    /// <summary>
    /// 断开连接
    /// </summary>
    private void OnDisConnected()
    {
        if (m_Client != null && m_Client.Connected)
        {
            m_Client.Close();
            m_Client = null;

            m_TcpStream.Close();
            m_TcpStream = null;
        }
        Manager.Net.OnDisConnected();
    }
}
