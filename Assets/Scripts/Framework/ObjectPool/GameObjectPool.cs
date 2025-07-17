using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : PoolBase
{
    public override Object Spwan(string name)
    {
        Object obj = base.Spwan(name);
        if (obj == null)
            return null;

        GameObject go = obj as GameObject;
        go.SetActive(true);
        return obj;
    }

    public override void UnSpwan(string name, Object obj)
    {
        GameObject go = obj as GameObject;
        go.SetActive(false);
        go.transform.SetParent(this.transform, false);
        base.UnSpwan(name, obj);
    }

    public override void Release()
    {
        base.Release();
        List<PoolObject> removeList = new List<PoolObject>();
        foreach (var item in m_Objects)
        {
            if (System.DateTime.Now.Ticks - item.LastUseTime.Ticks >= m_ReleaseTime * 10000000)
            {
                Debug.Log("GameObjectPool 释放时间:" + System.DateTime.Now);
                Destroy(item.Object);

                // 减少Bundle和依赖的引用计数
                Manager.Resource.MinuBundleCount(item.Name);

                removeList.Add(item);
                return;
            }
        }
        RemoveObjects(removeList);
    }

    
}
