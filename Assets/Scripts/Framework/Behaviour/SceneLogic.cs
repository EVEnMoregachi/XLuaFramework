using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLogic : LuaBahaviour
{
    public string SceneName;

    Action m_LuaAction;// º§ªÓ
    Action m_LuaInActice; // Õ£”√
    Action m_LuaOnEnter;
    Action m_LuaOnQuit;

    public override void Init(string luaName)
    {
        base.Init(luaName);
        m_ScriptEnv.Get("OnActice", out m_LuaAction);
        m_ScriptEnv.Get("OnInActive", out m_LuaInActice);
        m_ScriptEnv.Get("OnEnter", out m_LuaOnEnter);
        m_ScriptEnv.Get("OnQuit", out m_LuaOnQuit);
    }

    public void OnActive()
    {
        m_LuaAction?.Invoke();
    }

    public void OnInActive()
    {
        m_LuaInActice?.Invoke();
    }
    public void OnEnter()
    {
        m_LuaOnEnter?.Invoke();
    }
    public void OnQuit()
    {
        m_LuaOnQuit?.Invoke();
    }

    protected override void Clear()
    {
        m_LuaAction = null;
        m_LuaInActice = null;
        m_LuaOnEnter = null;
        m_LuaOnQuit = null;
        base.Clear();
    }
}
