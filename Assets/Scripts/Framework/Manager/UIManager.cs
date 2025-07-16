using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // ��ʱUI����
    //Dictionary<string, GameObject> m_UI = new Dictionary<string, GameObject>();
    // UI���飨��UI�㼶�����ֹ�Ͳ㼶UI�����ڸ߲㼶UI�ϲ㣩
    Dictionary<string, Transform> m_UIGroups = new Dictionary<string, Transform>();

    private Transform m_UIParent;
    private void Awake()
    {
        m_UIParent = this.transform.parent.Find("UI");
    }

    /// <summary>
    /// Lua ���UIGroups�ӿ�
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
        // ������ж���
        if (uiObj != null)
        {
            ui = uiObj as GameObject;
            ui.transform.SetParent(parent, false);
            UILogic uiLogic = ui.AddComponent<UILogic>();
            uiLogic.OnOpen();
            return;
        }
        // �����û�ж���
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
