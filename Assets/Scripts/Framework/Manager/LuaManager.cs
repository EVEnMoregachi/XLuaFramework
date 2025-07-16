using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

public class LuaManager : MonoBehaviour
{
    // 所有的Lua文件名
    public List<string> LuaNames = new List<string>();
    /* 应为资源加载是异步的，不能加载Lua脚本资源后立即调用Lua脚本
     * 为了实现在需要使用Lua脚本的时候能够立刻调用
     * 所以预加载所有Lua脚本内容，使用时直接获取*/
    public Dictionary<string, byte[]> m_LuaScripts;

    public LuaEnv LuaEnv;

    public void Init()
    {
        LuaEnv = new LuaEnv();
        LuaEnv.AddLoader(Loader);
        m_LuaScripts = new Dictionary<string, byte[]>();
#if UNITY_EDITOR
        if (AppConst.GameMode == GameMode.EditorMode)
            EditorLoadLuaScript();
        else
#endif
            LoadLuaScript();

    }

    /// <summary>
    /// 加载Lua脚本
    /// </summary>
    public void StartLua(string name)
    {
        LuaEnv.DoString(string.Format("require '{0}'", name));
    }

    /// <summary>
    /// 自定义Loader 用于加载Lua脚本
    /// </summary>
    byte[] Loader(ref string name)
    {
        return GetLuaScript(name);
    }

    internal byte[] GetLuaScript(string name)
    {
        name = name.Replace(".", "/");
        string fileName = PathUtil.GetLuaPath(name);

        byte[] luaScript = null;
        if (!m_LuaScripts.TryGetValue(fileName, out luaScript))
            Debug.LogError("Lua script is not exist:" + fileName);
        return luaScript;
    }

    void LoadLuaScript()
    {
        foreach (string name in LuaNames)
        {
            Manager.Resource.LoadLua(name, (UnityEngine.Object obj) =>
            {   // 当Lua脚本异步加载完成后，将Lua脚本缓存起来
                AddLuaScript(name, (obj as TextAsset).bytes);
                if (m_LuaScripts.Count >= LuaNames.Count)
                {   // 已经读取的Lua脚本数量大于等于需要加载的Lua脚本数量，即所有Lua加载完成
                    Manager.Event.Fire(10000);
                    LuaNames.Clear();
                    LuaNames = null;
                }
            });
        }
    }

    private void AddLuaScript(string fileName, byte[] file)
    {
        m_LuaScripts[fileName] = file;
    }

#if UNITY_EDITOR
    void EditorLoadLuaScript()
    {
        string[] luaFile = Directory.GetFiles(PathUtil.LuaPath, "*.bytes", SearchOption.AllDirectories);
        for (int i = 0; i < luaFile.Length; i++)
        {
            string fileName = PathUtil.GetStandardPath(luaFile[i]);
            byte[] file = File.ReadAllBytes(fileName);
            AddLuaScript(PathUtil.GetUnityPath(fileName), file);
        }
        Manager.Event.Fire(10000);
    }
#endif

    private void Update()
    {
        LuaEnv?.Tick();
    }

    private void OnDestroy()
    {
        if (LuaEnv != null)
        {
            LuaEnv.Dispose();
            LuaEnv = null;
        }
    }
}
