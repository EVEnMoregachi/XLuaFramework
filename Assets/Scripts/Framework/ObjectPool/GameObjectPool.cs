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

    public void Release()
    {
        base.Release();
        foreach (var item in m_Objects)
        {
            if (System.DateTime.Now.Ticks - item.LastUseTime.Ticks >= m_ReleaseTime * 10000000)
            {
                Debug.Log("GameObjectPool 释放时间:" + System.DateTime.Now);
                Destroy(item.Objuect);
                // 这里删除了迭代器正在迭代的元素，所以递归调用一次
                m_Objects.Remove(item);
                Release();
                return;
            }
        }
    }
}
