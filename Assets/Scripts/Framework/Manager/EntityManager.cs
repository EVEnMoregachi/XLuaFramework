using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    // 临时Entity缓存
    Dictionary<string, GameObject> m_Entities = new Dictionary<string, GameObject>();
    // Entity分组
    Dictionary<string, Transform> m_Groups = new Dictionary<string, Transform>();

    private Transform m_EntityParent;
    private void Awake()
    {
        m_EntityParent = this.transform.parent.Find("Entity");
    }

    /// <summary>
    /// Lua 添加UIGroups接口
    /// </summary>
    public void SetEntityGroup(List<string> group)
    {
        for (int i = 0; i < group.Count; i++)
        {
            GameObject go = new GameObject("Group-" + group[i]);
            go.transform.SetParent(m_EntityParent, false);
            m_Groups[group[i]] =  go.transform;
        }
    }

    Transform GetGroup(string name)
    {
        if (m_Groups.TryGetValue(name, out Transform transform))
        {
            return transform;
        }
        Debug.Log("UIgroup is not exist");
        return null;
    }


    public void ShowEntity(string name, string group, string luaName)
    {
        GameObject entity = null;
        if (m_Entities.TryGetValue(name, out entity))
        {
            EntityLogic logic = entity.AddComponent<EntityLogic>();
            logic.OnShow();
            return;
        }

        Manager.Resource.LoadPrefab(name, (UnityEngine.Object obj) =>
        {
            entity = Instantiate(obj, GetGroup(group)) as GameObject;
            m_Entities.Add(name, entity);

            EntityLogic logic = entity.AddComponent<EntityLogic>();
            logic.Init(luaName);
            logic.OnShow();
        });
    }
}
