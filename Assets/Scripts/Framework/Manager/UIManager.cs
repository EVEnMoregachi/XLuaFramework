using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // 临时UI缓存
    //Dictionary<string, GameObject> m_UI = new Dictionary<string, GameObject>();
    // UI分组（给UI层级分组防止低层级UI覆盖在高层级UI上层）
    Dictionary<string, Transform> m_UIGroups = new Dictionary<string, Transform>();

    private Transform m_UIParent;
    private void Awake()
    {
        m_UIParent = this.transform.parent.Find("UI");
    }

    /// <summary>
    /// Lua 添加UIGroups接口
    /// </summary>
    public void SetUIGroup(List<string> group)
    {
        for (int i = 0; i < group.Count; i++)
        {
            GameObject go = new GameObject("Group-" + group[i]);
            go.transform.SetParent(m_UIParent, false);
            m_UIGroups[group[i]] = go.transform;
        }
    }

    Transform GetUIGroup(string name)
    {
        if (m_UIGroups.TryGetValue(name, out Transform transform))
        {
            return transform;
        }
        Debug.Log("UIgroup is not exist");
        return null;
    }


    public void OpenUI(string uiName, string group, string luaName)
    {
        GameObject ui = null;
        Transform parent = GetUIGroup(group);
        string uiPath = PathUtil.GetUIPath(uiName);
        Object uiObj = Manager.Pool.Spawn("UI", uiPath);
        // 对象池有对象
        if (uiObj != null)
        {
            ui = uiObj as GameObject;
            ui.transform.SetParent(parent, false);
            UILogic uiLogic = ui.AddComponent<UILogic>();
            uiLogic.OnOpen();
            return;
        }
        // 对象池没有对象
        Manager.Resource.LoadUI(uiName, (UnityEngine.Object obj) =>
        {
            ui = Instantiate(obj) as GameObject;
            ui.transform.SetParent(parent, false);
            UILogic uiLogic = ui.AddComponent<UILogic>();
            uiLogic.AssetName = uiPath;
            uiLogic.Init(luaName);
            uiLogic.OnOpen();
        });
    }
}
