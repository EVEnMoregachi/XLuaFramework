using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;
using XLua;

public class HotUpdate : MonoBehaviour
{
    // filelist��data���棬����д�� ��дĿ¼
    byte[] m_ReadPathFileListData;
    byte[] m_ServerFileListData;

    internal class DownFileInfo
    {
        public string url;
        public string fileName;
        public DownloadHandler fileData;
    }
    // �����ļ�����
    int m_DownloadCount;

    /// <summary>
    /// ���ص����ļ�
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    IEnumerator DownLoadFile(DownFileInfo info, Action<DownFileInfo> Complete)
    {
        // ��url������Դ
        UnityWebRequest webRequest = UnityWebRequest.Get(info.url);
        yield return webRequest.SendWebRequest();
        if (webRequest.isHttpError || webRequest.isNetworkError)
        {
            Debug.LogError("�����ļ�����" + info.url);
            yield break;

            // TODO:���� / ���Դ���
        }
        yield return new WaitForSeconds(0.2f);
        info.fileData = webRequest.downloadHandler;
        // �ص����
        Complete?.Invoke(info);
        // �ͷ���Դ
        webRequest.Dispose();
    }

    /// <summary>
    /// ���ض���ļ�
    /// </summary>
    /// <param name="info"></param>
    /// <param name="Complete"></param>
    /// <returns></returns>
    IEnumerator DownLoadFile(List<DownFileInfo> infos, Action<DownFileInfo> Complete, Action DownLoadAllComplete)
    {
        foreach(var info in infos)
        {
            yield return DownLoadFile(info, Complete);
        }
        DownLoadAllComplete?.Invoke();
    }

    /// <summary>
    /// ��ȡ�ļ���Ϣ(����filelist�ļ�)
    /// </summary>
    /// <returns></returns>
    private List<DownFileInfo> GetFileList(string fileDate, string path)
    {
        // windoswд��txt�ļ������һ���س���
        string content = fileDate.Trim().Replace("\r", "");
        string[] files = content.Split('\n');
        List<DownFileInfo> downFileInfos = new List<DownFileInfo>(files.Length);
        for (int i = 0; i < files.Length; i++)
        {
            string[] info = files[i].Split('|');
            DownFileInfo fileInfo = new DownFileInfo();
            fileInfo.fileName = info[1];
            fileInfo.url = Path.Combine(path, info[1]);
            downFileInfos.Add(fileInfo);
        }
        return downFileInfos;
    }


    GameObject loadingObj;
    LoadingUI loadingUI;
    private void Start()
    {
        GameObject go = Resources.Load<GameObject>("LoadingUI");
        loadingObj = Instantiate(go);
        loadingObj.transform.SetParent(this.transform, false);
        loadingUI = loadingObj.GetComponent<LoadingUI>();

        if (IsFirstInstall())
        {   // ���ֻ��Ŀ¼����Դ�������ͷ���Դ����дĿ¼
            ReleaseResources();
        }
        else
        {   // �������̣��ӷ�����������Դ����дĿ¼
            CheckUpdate();
        }
    }

    private bool IsFirstInstall()
    {
        // ֻ��Ŀ¼filelist
        bool isExisteReadPath = FileUtil.IsExists(Path.Combine(PathUtil.ReadPath, AppConst.FileListName));
        // ��дĿ¼filelist
        bool isExisteReadWritePath = FileUtil.IsExists(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName));

        return isExisteReadPath && !isExisteReadWritePath;
    }

    /// <summary>
    /// �ͷ���Դ
    /// </summary>
    private void ReleaseResources()
    {
        m_DownloadCount = 0;
        string url = Path.Combine(PathUtil.ReadPath, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        // ����ֻ��Ŀ¼filelist�ͷ���Դ����дĿ¼
        StartCoroutine(DownLoadFile(info, OnDownLoadReadPathFileListComplete));
    }

    /// <summary>
    /// ����filelist���ͷ���Դ
    /// </summary>
    private void OnDownLoadReadPathFileListComplete(DownFileInfo fileList)
    {
        // ����filelist��data����
        m_ReadPathFileListData = fileList.fileData.data;
        // ����ֻ��Ŀ¼��filelist
        List<DownFileInfo> fileInfos = GetFileList(fileList.fileData.text, PathUtil.ReadPath);
        // �ͷ�filelist�������ļ�
        StartCoroutine(DownLoadFile(fileInfos, OnReleaseFileComplete, OnRelsaseAllFileComplete));

        loadingUI.InitProgress(fileInfos.Count, "�����ͷ���Դ������������...");
    }

    /// <summary>
    /// д���ļ�����дĿ¼��ÿ��������һ���ļ�ʱ��
    /// </summary>
    private void OnReleaseFileComplete(DownFileInfo fileInfo)
    {
        Debug.Log("OnReleaseFileComplete:" + fileInfo.url);

        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);

        m_DownloadCount++;
        loadingUI.UpdateProgress(m_DownloadCount);
    }

    /// <summary>
    /// �����ļ��������ʱ���ڶ�дĿ¼д��filelist
    /// </summary>
    private void OnRelsaseAllFileComplete()
    {
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ReadPathFileListData);
        CheckUpdate();
    }

    /// <summary>
    /// ������
    /// </summary>
    private void CheckUpdate()
    {
        string url = Path.Combine(AppConst.ResourcesUrl, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo
        {
            url = url,
        };
        StartCoroutine(DownLoadFile(info, OnDownLoadSercerFileListComplete));
    }

    private void OnDownLoadSercerFileListComplete(DownFileInfo fileInfo)
    {
        m_DownloadCount = 0;
        m_ServerFileListData = fileInfo.fileData.data;
        List<DownFileInfo> fileInfos = GetFileList(fileInfo.fileData.text, AppConst.ResourcesUrl);
        List<DownFileInfo> downListFiles = new List<DownFileInfo>();

        for (int i = 0; i < fileInfos.Count; i++)
        {
            string localFile = Path.Combine(PathUtil.ReadWritePath, fileInfos[i].fileName);
            if (!FileUtil.IsExists(localFile))
            {
                downListFiles.Add(fileInfos[i]);
            }
        }

        if (downListFiles.Count > 0)
        {
            StartCoroutine(DownLoadFile(fileInfos, OnUpdateFileComplete, OnUpdateAllDileComplete));
            loadingUI.InitProgress(downListFiles.Count, "���ڸ�����Դ...");
        }
        else
        {
            EnterGame();
        }
    }

    private void OnUpdateFileComplete(DownFileInfo fileInfo)
    {
        Debug.Log("OnUpdateFileComplete:" + fileInfo.url);

        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
        m_DownloadCount++;
        loadingUI.UpdateProgress(m_DownloadCount);
    }

    private void OnUpdateAllDileComplete()
    {
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ServerFileListData);
        EnterGame();
        loadingUI.InitProgress(0, "��������...");
    }

    private void EnterGame()
    {
        Manager.Event.Fire((int)GameEvent.GameInit);
        Destroy(loadingObj);
    }

    private void OnComplete(UnityEngine.Object obj)
    {
        GameObject go = Instantiate(obj) as GameObject;
        go.transform.SetParent(this.transform);
        go.SetActive(true);
        go.transform.localPosition = Vector3.zero;
    }
}
