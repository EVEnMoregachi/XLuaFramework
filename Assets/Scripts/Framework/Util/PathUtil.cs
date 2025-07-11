using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathUtil
{
    // ���ÿ�δ����ʹ��Application.dataPath�������GC�����ﶨ���ֻ��Ҫ����һ��
    // ��Ŀ¼
    public static readonly string AssetsPath = Application.dataPath;
    // ��Ҫ��Bundle��Ŀ¼
    public static readonly string BuildResoursesPath = AssetsPath + "/BuildResources/";
    // Bundle ���Ŀ¼
    public static readonly string BundleOutPath = Application.streamingAssetsPath;

    public static string BundleResourcePath
    {
        get { return Application.streamingAssetsPath; }
    }

    /// <summary>
    /// ��ȡUnity�����·��
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetUnityPath(string path)
    {
        if (string.IsNullOrEmpty(path)) 
            return string.Empty;
        return path.Substring(path.IndexOf("Assets"));// ��ȡAssets֮���·��
    }

    /// <summary>
    /// ��ȡ��׼·��
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
