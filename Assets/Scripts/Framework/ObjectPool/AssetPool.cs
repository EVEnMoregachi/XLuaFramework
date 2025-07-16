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
        foreach (var item in m_Objects)
        {
            if (System.DateTime.Now.Ticks - item.LastUseTime.Ticks >= m_ReleaseTime * 10000000)
            {
                Debug.Log("AssetPool 释放时间:" + System.DateTime.Now);
                Manager.Resource.UnLoadBundle(item.Name);
                // 这里删除了迭代器正在迭代的元素，所以递归调用一次
                m_Objects.Remove(item);
                Release();
                return;
            }
        }
    }
}
