using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityLogic : LuaBahaviour
{
    Action m_LuaOnShow;
    Action m_LuaOnHide;

    public override void Init(string luaName)
    {
        base.Init(luaName);
        m_ScriptEnv.Get("OnShow", out m_LuaOnShow);
        m_ScriptEnv.Get("OnHide", out m_LuaOnHide);
        m_LuaInit?.Invoke();
    }

    public void OnShow()
    {
        m_LuaOnShow?.Invoke();
    }

    public void OnHide()
    {
        m_LuaOnHide?.Invoke();
    }

    protected override void Clear()
    {
        m_LuaOnShow = null;
        m_LuaOnHide = null;
        base.Clear();
    }
}
