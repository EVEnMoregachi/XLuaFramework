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
    // 预加载Lua脚本内容
    public Dictionary<string, byte[]> m_LuaScripts;

    public LuaEnv LuaEnv;

    Action InitFinished;

    public void Init(Action init)
    {
        InitFinished += init;
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

    private byte[] GetLuaScript(string name)
    {
        name = name.Replace(".", "/");
        string fileName = PathUtil.GetLuaPath(name);

        byte[] luaScript = null;
        if (!m_LuaScripts.TryGetValue(fileName, out luaScript))
            Debug.LogError("Lua script id not exist:" + fileName);
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
                    InitFinished?.Invoke();
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
