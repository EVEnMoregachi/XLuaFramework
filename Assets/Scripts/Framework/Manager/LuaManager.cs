using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

public class LuaManager : MonoBehaviour
{
    // ���е�Lua�ļ���
    public List<string> LuaNames = new List<string>();
    /* ӦΪ��Դ�������첽�ģ����ܼ���Lua�ű���Դ����������Lua�ű�
     * Ϊ��ʵ������Ҫʹ��Lua�ű���ʱ���ܹ����̵���
     * ����Ԥ��������Lua�ű����ݣ�ʹ��ʱֱ�ӻ�ȡ*/
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
    /// ����Lua�ű�
    /// </summary>
    public void StartLua(string name)
    {
        LuaEnv.DoString(string.Format("require '{0}'", name));
    }

    /// <summary>
    /// �Զ���Loader ���ڼ���Lua�ű�
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
            {   // ��Lua�ű��첽������ɺ󣬽�Lua�ű���������
                AddLuaScript(name, (obj as TextAsset).bytes);
                if (m_LuaScripts.Count >= LuaNames.Count)
                {   // �Ѿ���ȡ��Lua�ű��������ڵ�����Ҫ���ص�Lua�ű�������������Lua�������
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
