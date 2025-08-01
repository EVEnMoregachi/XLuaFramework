using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILogic : LuaBahaviour
{
    public string AssetName;
    Action m_LuaOnOpen;
    Action m_LuaOnClose;

    public override void Init(string luaName)
    {
        base.Init(luaName);
        m_ScriptEnv.Get("OnOpen", out m_LuaOnOpen);
        m_ScriptEnv.Get("OnClose", out m_LuaOnClose);
        m_LuaInit?.Invoke();
    }

    public void OnOpen()
    {
        m_LuaOnOpen?.Invoke();
    }

    public void OnClose()
    {
        m_LuaOnClose?.Invoke();
        Manager.Pool.UnSpawn("UI", AssetName, this.gameObject);
    }

    protected override void Clear()
    {
        m_LuaOnOpen = null;
        m_LuaOnClose = null;
        base.Clear();
    }
}
