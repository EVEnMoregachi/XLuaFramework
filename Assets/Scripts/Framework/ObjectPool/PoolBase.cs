using System.Collections.Generic;
using UnityEngine;

public class PoolBase : MonoBehaviour
{
    // 自动释放时间
    protected float m_ReleaseTime;

    // 上次释放资源的时间(单位：100纳秒/0.1毫秒) 1s = 10 000 000 Tick
    protected long m_LastReleaseTime = 0;

    protected List<PoolObject> m_Objects;

    public void Start()
    {
        m_LastReleaseTime = System.DateTime.Now.Ticks;
    }
    
    /// <summary>
    /// 初始化, 对象池自动释放时间
    /// </summary>
    /// <param name="time">自动释放时间（秒）</param>
    public void Init(float time)
    {
        m_ReleaseTime = time;
        m_Objects = new List<PoolObject>();
    }

    /// <summary>
    /// 取出对象
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual Object Spwan(string name)
    {
        foreach (PoolObject obj in m_Objects)
        {
            if (obj.Name == name)
            {
                m_Objects.Remove(obj);
                return obj.Object;
            }
        }
        return null;
    }

    /// <summary>
    /// 回收对象
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    public virtual void UnSpwan(string name, Object obj)
    {
        m_Objects.Add(new PoolObject(name, obj));
    }

    /// <summary>
    /// 释放对象
    /// </summary>
    public virtual void Release()
    {

    }

    private void Update()
    {
        if (System.DateTime.Now.Ticks - m_LastReleaseTime >= m_ReleaseTime * 10000000)
        {
            m_LastReleaseTime = System.DateTime.Now.Ticks;
            Release();
        }
    }


    protected void RemoveObjects(List<PoolObject> removeList)
    {
        foreach (var item in removeList)
        {
            m_Objects.Remove(item);
        }
    }
}
