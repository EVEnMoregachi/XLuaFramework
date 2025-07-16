using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class LuaBahaviour : MonoBehaviour
{
    private LuaEnv m_LuaEnv = Manager.Lua.LuaEnv;
    protected LuaTable m_ScriptEnv;

    protected Action m_LuaInit;
    private Action m_LuaUpdate;
    private Action m_LuaOnDestroy;

    private void Awake()
    {
        // Ϊÿ���ű�����һ�������Ľű��򣬿�һ���̶��Ϸ�ֹ�ű���ȫ�ֱ�����������ͻ
        m_ScriptEnv = m_LuaEnv.NewTable();
        // ������Ԫ��� __index, ʹ���ܹ�����ȫ�ֱ���
        LuaTable meta = m_LuaEnv.NewTable();
        meta.Set("__index", m_LuaEnv.Global);
        m_ScriptEnv.SetMetaTable(meta);
        meta.Dispose();

        // ��Selfע�뵽Lua����
        m_ScriptEnv.Set("Self", this);
        // ��Ҫ��Awake֮ǰ��Lua�ű��е����ַ������루luaName�������˹���һ���ֶ���unity�༭���а����޷�������
        // ����ֻ������Awake��Start�������Զ����Init����
    }

    public virtual void Init(string luaName)
    {
        // �� Lua �ű����ص�ָ���� Lua ����������
        m_LuaEnv.DoString(Manager.Lua.GetLuaScript(luaName), luaName, m_ScriptEnv);

        m_ScriptEnv.Get("OnInit", out m_LuaInit);
        m_ScriptEnv.Get("Update", out m_LuaUpdate);
    }

    private void Update()
    {
        m_LuaUpdate?.Invoke();
    }

    protected virtual void Clear()
    {
        m_LuaOnDestroy = null;
        m_LuaInit = null;
        m_LuaUpdate = null;
        m_ScriptEnv?.Dispose();
        m_ScriptEnv = null;
    }

    private void OnDestroy()
    {
        m_LuaOnDestroy?.Invoke();
        Clear();
    }

    private void OnApplicationQuit()
    {
        Clear();
    }
}
