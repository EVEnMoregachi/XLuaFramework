using System.Collections.Generic;
using UnityEngine;

public class AssetPool : PoolBase
{
    public override Object Spwan(string name)
    {
        return base.Spwan(name);
    }

    public override void UnSpwan(string name, Object obj)
    {
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
                Debug.Log("AssetPool 释放时间:" + System.DateTime.Now + "UnLoad ab:" + item.Name);
                Manager.Resource.UnLoadBundle(item.Object);

                removeList.Add(item);
                return;
            }
        }
        RemoveObjects(removeList);
    }
}
