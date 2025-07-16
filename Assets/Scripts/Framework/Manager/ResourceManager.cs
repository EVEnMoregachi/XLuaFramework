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

    internal class BundleDate
    {
        public AssetBundle Bundle;
        // ���ü���
        public int Count;
        public BundleDate(AssetBundle bundle)
        {
            Bundle = bundle;
            Count = 1;
        }
    }
    // ���Bundle��Ϣ
    private Dictionary<string, BundleInfo> m_BundleInfos = new Dictionary<string, BundleInfo>();

    // ����AssetBundle��Դ
    private Dictionary<string, BundleDate> m_AssetBundles = new Dictionary<string, BundleDate>();

    /// <summary>
    /// �����汾�ļ�
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
            // ֪��list������ʱ������ʼ����������������
            bundleInfo.Dependences = new List<string>(info.Length - 2);
            for (int j = 2; j < info.Length; j++)
            {
                bundleInfo.Dependences.Add(info[j]);
            }
            m_BundleInfos.Add(bundleInfo.AssetsName, bundleInfo);
            // �ҳ����е�Lua�ű�
            if (info[0].IndexOf("LuaScripts") > 0)
            {
                Manager.Lua.LuaNames.Add(info[0]);
            }
        }
    }

    /// <summary>
    /// �첽������Դ
    /// </summary>
    /// <param name="assteName">��Դ��</param>
    /// <param name="action">��ɻص�</param>
    /// <returns></returns>
    private IEnumerator LoadBundleAsync(string assteName, Action<UObject> action = null)
    {
        string bundleName = m_BundleInfos[assteName].BundleName;
        string bundlePath = Path.Combine(PathUtil.BundleResourcePath, bundleName);
        List<string> dependences = m_BundleInfos[assteName].Dependences;

        // ab��ֻ�ܼ���һ�Σ��������Ч��Դ��Ҫ��ε��ã���ʹ�û����е�
        BundleDate bundle = GetBundle(bundleName);
        // ������û��
        if (bundle == null)
        {
            // ȥ���������
            UObject obj = Manager.Pool.Spawn("AssetBundle", bundleName);
            if (obj != null)
            {
                AssetBundle ab = obj as AssetBundle;
                bundle = new BundleDate(ab);
            }
            else
            {// �������Ҳû�У�������Դ
                // ��ab�����ص�request.assetBundle
                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
                yield return request;
                bundle = new BundleDate(request.assetBundle);
            }
            m_AssetBundles[bundleName] = bundle;
        }

        // ������Դ����
        if (dependences != null && dependences.Count > 0)
        {
            for (int i = 0; i < dependences.Count; i++)
            {   // ���������������Ҫ��Դ��ֻ����Bundle������Ҫ�ص�
                yield return LoadBundleAsync(dependences[i]);
            }
        }

        // ������Դ����ҪAsset��Դ��ֻ��Ҫ����Bundle������Ҫ�ص�asset��Դ
        if (assteName.EndsWith(".unity"))
        {
            action?.Invoke(null);
            yield break;
        }

        if (action == null)
        {   // ������Դ����Ҫ������Դ��ֱ��Break
            yield break;
        }
        AssetBundleRequest bundleRequest = bundle.Bundle.LoadAssetAsync(assteName);
        yield return bundleRequest;

        action?.Invoke(bundleRequest?.asset);
    }

    /// <summary>
    /// �Ѿ����ع���ab�����ӻ����л�ȡ
    /// </summary>
    BundleDate GetBundle(string name)
    {
        BundleDate bundle = null;
        if (m_AssetBundles.TryGetValue(name, out bundle))
        {
            bundle.Count++;
            return bundle;
        }
        return null;
    }

    /// <summary>
    /// ����һ��Bundle�����ü���
    /// </summary>
    /// <param name="name"></param>
    private void MinusOneBundleCount(string bundleName)
    {
        if (m_AssetBundles.TryGetValue(bundleName, out BundleDate bundle))
        {
            if (bundle.Count > 0)
            {
                bundle.Count--;
                Debug.Log("bundle���ü���:" + bundleName + " count:" + bundle.Count);
            }
            if (bundle.Count <= 0)
            {
                Debug.Log("����Bundle����أ�" + bundleName);
                Manager.Pool.UnSpawn("AssetBundle", bundleName, bundle.Bundle);
                m_AssetBundles.Remove(bundleName);
            }
        }
    }


    /// <summary>
    /// ����Bundle�����������ü���
    /// </summary>
    public void MinuBundleCount(string assetName)
    {
        string bundleName = m_BundleInfos[assetName].BundleName;
        MinusOneBundleCount(bundleName);

        // ������Դ
        List<string> dependences = m_BundleInfos[assetName].Dependences;
        if (dependences != null)
        {
            foreach (string dependence in dependences)
            {
                string name = m_BundleInfos[dependence].BundleName;
                MinusOneBundleCount(name);
            }
        }
    }


#if UNITY_EDITOR
    /// <summary>
    /// �༭�������¼�����Դ
    /// Ϊ�˷�ֹ�༭��������Ƶ����Build Bundle���༭��ģʽ��ֱ�Ӷ�ȡAssetsĿ¼�µ���Դ
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    void EditorLoadAsset(string assetName, Action<UObject> action = null)
    {
        UObject obj = UnityEditor.AssetDatabase.LoadAssetAtPath(assetName, typeof(UObject));
        if (obj == null)
            Debug.LogError("��Դ�����ڣ�" + assetName);
        action?.Invoke(obj);
    }
#endif

    /// <summary>
    /// �첽������Դ
    /// </summary>
    private void LoadAsset(string assetName, Action<UObject> action)
    {
        // ���� Editor ģʽ��ֱ�Ӽ�����Դ
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

    /// <summary>
    /// ж����Դ
    /// </summary>
    /// <param name="name"></param>
    internal void UnLoadBundle(UObject obj)
    {
        AssetBundle ab = obj as AssetBundle;
        ab.Unload(true);
    }
}