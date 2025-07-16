using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    Transform m_PoolParent;
    // ������ֵ�
    Dictionary<string, PoolBase> m_Pools = new Dictionary<string, PoolBase>();

    private void Awake()
    {
        m_PoolParent = this.transform.parent.Find("Pool");

    }

    private void CreatePool<T>(string poolNname, float releaseTime) where T : PoolBase
    {
        if (!m_Pools.TryGetValue(poolNname, out PoolBase pool))
        {
            GameObject go = new GameObject(poolNname);
            go.transform.SetParent(m_PoolParent);
            pool = go.AddComponent<T>();
            pool.Init(releaseTime);
            m_Pools.Add(poolNname, pool);
        }
    }

    /// <summary>
    /// ������������
    /// </summary>
    /// <param name="poolNname">���������</param>
    /// <param name="releaseTime">��Դ�ͷ�����</param>
    public void CreateGameObjectPool(string poolNname, int releaseTime)
    {
        CreatePool<GameObjectPool>(poolNname, releaseTime);
    }

    /// <summary>
    /// ������Դ�����
    /// </summary>
    /// <param name="poolNname">���������</param>
    /// <param name="releaseTime">��Դ�ͷ�����</param>
    public void CreateAssetPool(string poolNname, int releaseTime)
    {
        CreatePool<AssetPool>(poolNname, releaseTime);
    }

    /// <summary>
    /// �Ӷ����ȡ������
    /// </summary>
    public Object Spawn(string poolName, string assetName)
    {
        if (m_Pools.TryGetValue(poolName, out PoolBase pool))
        {
            return pool.Spwan(assetName);
        }
        return null;
    }

    /// <summary>
    /// ���ն��󵽶����
    /// </summary>
    public bool UnSpawn(string poolName, string assetName, Object obj)
    {
        if (m_Pools.TryGetValue(poolName, out PoolBase pool))
        {
            pool.UnSpwan(assetName, obj);
            return true;
        }
        return false;
    }
}
