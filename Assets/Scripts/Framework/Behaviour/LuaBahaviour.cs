using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class LuaBahaviour : MonoBehaviour
{
    private LuaEnv m_LuaEnv = Manager.Lua.LuaEnv;
    protected LuaTable m_ScriptEnv;

    //private Action m_LuaAwake;
    //private Action m_LuaStart;
    private Action m_LuaInit;
    private Action m_LuaUpdate;
    private Action m_LuaOnDestroy;

    private void Awake()
    {
        // 为每个脚本设置一个独立的脚本域，可一定程度上防止脚本间全局变量、函数冲突
        m_ScriptEnv = m_LuaEnv.NewTable();
        // 设置其元表的 __index, 使其能够访问全局变量
        LuaTable meta = m_LuaEnv.NewTable();
        meta.Set("__index", m_LuaEnv.Global);
        m_ScriptEnv.SetMetaTable(meta);
        meta.Dispose();

        m_ScriptEnv.Set("Self", this);

        // 想要在Awake之前将Lua脚本中的名字符串传入（luaName），除了公开一个字段在unity编辑器中绑定外无法做到，
        // 所以只能弃用Awake和Start，改用自定义的Init方法

        //m_LuaEnv.DoString(Manager.Lua.GetLuaScript(luaName), luaName, m_ScriptEnv);
        //m_ScriptEnv.Get("Awake", out m_LuaAwake);
        //m_ScriptEnv.Get("Start", out m_LuaStart);
        //m_ScriptEnv.Get("Update", out m_LuaUpdate);

        //m_LuaAwake?.Invoke();
    }

    public virtual void Init(string luaName)
    {
        m_LuaEnv.DoString(Manager.Lua.GetLuaScript(luaName), luaName, m_ScriptEnv);

        //m_ScriptEnv.Get("Awake", out m_LuaAwake);
        //m_ScriptEnv.Get("Start", out m_LuaStart);
        m_ScriptEnv.Get("OnInit", out m_LuaInit);
        m_ScriptEnv.Get("Update", out m_LuaUpdate);

        m_LuaInit?.Invoke();
    }

    //private void Start()
    //{
    //    m_LuaStart?.Invoke();
    //}

    private void Update()
    {
        m_LuaUpdate?.Invoke();
    }

    protected virtual void Clear()
    {
        m_LuaOnDestroy = null;
        //m_LuaAwake = null;
        //m_LuaStart = null;
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
