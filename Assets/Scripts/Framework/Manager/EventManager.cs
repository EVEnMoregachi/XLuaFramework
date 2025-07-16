using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void EventHandler(object args);

    Dictionary<int, EventHandler> m_Evnets = new Dictionary<int, EventHandler>();

    /// <summary>
    /// 订阅事件
    /// </summary>
    public void Subscribe(int id, EventHandler handler)
    {
        if (m_Evnets.ContainsKey(id))
            m_Evnets[id] += handler;
        else
            m_Evnets.Add(id, handler);
    }

    /// <summary>
    /// 取消订阅
    /// </summary>
    public void UnSubscribe(int id, EventHandler handler)
    {
        if (m_Evnets[id] != null)
            m_Evnets[id] -= handler;

        if (m_Evnets[id] == null)
            m_Evnets.Remove(id);
    }

    /// <summary>
    /// 执行事件
    /// </summary>
    public void Fire(int id, object args = null)
    {
        EventHandler handler;
        if (m_Evnets.TryGetValue(id, out handler))
        {
            handler?.Invoke(args);
        }
    }
}
