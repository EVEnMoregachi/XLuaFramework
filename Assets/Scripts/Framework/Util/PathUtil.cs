using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathUtil
{
    // 如果每次打包都使用Application.dataPath，会产生GC，这里定义后只需要访问一次
    // 根目录
    public static readonly string AssetsPath = Application.dataPath;
    // 需要打Bundle的目录
    public static readonly string BuildResoursesPath = AssetsPath + "/BuildResources/";
    // Bundle 输出目录
    public static readonly string BundleOutPath = Application.streamingAssetsPath;

    public static string BundleResourcePath
    {
        get { return Application.streamingAssetsPath; }
    }

    /// <summary>
    /// 获取Unity的相对路径
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
}
