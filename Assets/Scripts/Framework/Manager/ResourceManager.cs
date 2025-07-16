using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UObject = UnityEngine.Object;

public class ResourceManager : MonoBehaviour
{
    internal class BundleInfo
    {
        public string AssetsName;
        public string BundleName;
        public List<string> Dependences;
    }
    // 存放Bundle信息
    private Dictionary<string, BundleInfo> m_BundleInfos = new Dictionary<string, BundleInfo>();

    // 缓存AssetBundle资源
    private Dictionary<string, AssetBundle> m_AssetBundles = new Dictionary<string, AssetBundle>();

    /// <summary>
    /// 解析版本文件
    /// </summary>
    public void ParseVersionFile()
    {
        string url = Path.Combine(PathUtil.BundleResourcePath, AppConst.FileListName);
        string[] date = File.ReadAllLines(url);
        for (int i = 0; i < date.Length; i++)
        {
            BundleInfo bundleInfo = new BundleInfo();
            string[] info = date[i].Split('|');
            bundleInfo.AssetsName = info[0];
            bundleInfo.BundleName = info[1];
            // 知道list容量的时候尽量初始化容量，避免扩容
            bundleInfo.Dependences = new List<string>(info.Length - 2);
            for (int j = 2; j < info.Length; j++)
            {
                bundleInfo.Dependences.Add(info[j]);
            }
            m_BundleInfos.Add(bundleInfo.AssetsName, bundleInfo);
            // 找出所有的Lua脚本
            if (info[0].IndexOf("LuaScripts") > 0)
            {
                Manager.Lua.LuaNames.Add(info[0]);
            }
        }
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="assteName">资源名</param>
    /// <param name="action">完成回调</param>
    /// <returns></returns>
    private IEnumerator LoadBundleAsync(string assteName, Action<UObject> action = null)
    {
        string bundleName = m_BundleInfos[assteName].BundleName;
        string bundlePath = Path.Combine(PathUtil.BundleResourcePath, bundleName);
        List<string> dependences = m_BundleInfos[assteName].Dependences;

        // ab包只能加载一次，如果是音效资源需要多次调用，则把加载好的资源缓存起来取用
        AssetBundle bundle = GetBundle(bundleName);
        if (bundle == null)
        {
            if (dependences != null && dependences.Count > 0)
            {
                for (int i = 0; i < dependences.Count; i++)
                {   // 这里加载依赖不需要资源，只加载Bundle，不需要回调
                    yield return LoadBundleAsync(dependences[i]);
                }
            }
            // 把ab包加载到request.assetBundle
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
            yield return request;
            bundle = request.assetBundle;
            m_AssetBundles[bundleName] = bundle;
        }

        // 场景资源不需要Asset资源，只需要加载Bundle，不需要回调asset资源
        if (assteName.EndsWith(".unity"))
        {
            action?.Invoke(null);
            yield break;
        }
        AssetBundleRequest bundleRequest = bundle.LoadAssetAsync(assteName);
        yield return bundleRequest;

        action?.Invoke(bundleRequest?.asset);
    }

    /// <summary>
    /// 已经加载过的ab包，从缓存中获取
    /// </summary>
    AssetBundle GetBundle(string name)
    {
        AssetBundle bundle = null;
        if (m_AssetBundles.TryGetValue(name, out bundle))
        {
            return bundle;
        }
        return null;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器环境下加载资源
    /// 为了防止编辑器环境下频繁的Build Bundle，编辑器模式下直接读取Assets目录下的资源
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    void EditorLoadAsset(string assetName, Action<UObject> action = null)
    {
        UObject obj = UnityEditor.AssetDatabase.LoadAssetAtPath(assetName, typeof(UObject));
        if (obj == null)
            Debug.LogError("资源不存在：" + assetName);
        action?.Invoke(obj);
    }
#endif

    /// <summary>
    /// 异步加载资源
    /// </summary>
    private void LoadAsset(string assetName, Action<UObject> action)
    {
        // 仅在 Editor 模式下直接加载资源
#if UNITY_EDITOR
        if (AppConst.GameMode == GameMode.EditorMode)
        {
            EditorLoadAsset(assetName, action);
            return;
        }
        else
#endif
            StartCoroutine(LoadBundleAsync(assetName, action));
    }

    public void LoadUI(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetUIPath(assetName), action);
    }

    public void LoadMusic(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetMusicPath(assetName), action);
    }

    public void LoadSound (string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetSoundPath(assetName), action);
    }

    public void LoadEffect (string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetEffectPath(assetName), action);
    }

    public void LoadScene (string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetScenePath(assetName), action);
    }

    internal void LoadLua(string assetName, Action<UObject> action = null)
    {
        LoadAsset(assetName, action);
    }

    internal void LoadPrefab(string assetName, Action<UObject> action = null)
    {
        LoadAsset(assetName, action);
    }
}