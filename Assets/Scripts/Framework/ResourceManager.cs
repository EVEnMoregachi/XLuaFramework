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
    // ���Bundle��Ϣ
    private Dictionary<string, BundleInfo> m_BundleInfos = new Dictionary<string, BundleInfo>();

    /// <summary>
    /// �����汾�ļ�
    /// </summary>
    private void ParseVersionFile()
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
        if (dependences != null && dependences.Count > 0)
        {
            for (int i = 0; i < dependences.Count; i++)
            {   // ���������������Ҫ��Դ��ֻ����Bundle������Ҫ�ص�
                yield return LoadBundleAsync(dependences[i]);
            }
        }
        // ��ab�����ص�request.assetBundle
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return request;

        AssetBundleRequest bundleRequest = request.assetBundle.LoadAssetAsync(assteName);
        yield return bundleRequest;

        action?.Invoke(bundleRequest?.asset);
    }

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

    private void LoadAsset(string assetName, Action<UObject> action)
    {
        if (AppConst.GameMode == GameMode.EditorMode)
            EditorLoadAsset(assetName, action);
        else
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


    private void Start()
    {
        ParseVersionFile();

        LoadUI("Login/LoginUI", OnComplete);
    }

    private void OnComplete(UObject obj)
    {
        GameObject go = Instantiate(obj) as GameObject;
        go.transform.SetParent(this.transform);
        go.SetActive(true);
        go.transform.localPosition = Vector3.zero;
    }
}