using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathUtil
{
    // 只读目录
    public static readonly string ReadPath = Application.streamingAssetsPath;
    // 可读写目录
    public static readonly string ReadWritePath = Application.persistentDataPath;
    // 如果每次打包都使用Application.dataPath，会产生GC，这里定义后只需要访问一次
    // 根目录
    public static readonly string AssetsPath = Application.dataPath;
    // AssetBundle 资源路径
    public static readonly string BuildResoursesPath = AssetsPath + "/BuildResources/";
    // Build Bundle 输出路径
    public static readonly string BundleOutPath = Application.streamingAssetsPath;
    // Lua脚本 目录
    public static readonly string LuaPath = "Assets/BuildResources/LuaScripts";

    

    // Bundle 资源目录
    public static string BundleResourcePath
    {
        get 
        {
            if (AppConst.GameMode == GameMode.UpdateMode)
                return ReadWritePath;
            return ReadPath; 
        
        }
    }

    /// <summary>
    /// 获取Unity资源的相对路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetUnityPath(string path)
    {
        if (string.IsNullOrEmpty(path)) 
            return string.Empty;
        return path.Substring(path.IndexOf("Assets"));// 截取Assets之后的路径
    }

    /// <summary>
    /// 获取标准路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetStandardPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;
        return path.Trim().Replace("\\", "/");
    }

    public static string GetLuaPath(string name)
    {
        return string.Format("Assets/BuildResources/LuaScripts/{0}.bytes", name);
    }

    public static string GetUIPath(string name)
    {
        return string.Format("Assets/BuildResources/UI/Prefabs/{0}.prefab", name);
    }

    public static string GetMusicPath(string name)
    {
        return string.Format("Assets/BuildResources/Audio/Music/{0}", name);
    }

    public static string GetSoundPath(string name)
    {
        return string.Format("Assets/BuildResources/Audio/Sound/{0}", name);
    }

    public static string GetEffectPath(string name)
    {
        return string.Format("Assets/BuildResources/Effect/Prefabs/{0}.prefab", name);
    } 

    public static string GetModelPath(string name)
    {
        return string.Format("Assets/BuildResources/Model/Prefabs/{0}.prefab", name);
    }

    public static string GetSpritePath(string name)
    {
        return string.Format("Assets/BuildResources/Sprite/{0}", name);
    }

    public static string GetScenePath(string name)
    {
        return string.Format("Assets/BuildResources/Scenes/{0}.unity", name);
    }
}
