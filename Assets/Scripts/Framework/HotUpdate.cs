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
    // filelist的data缓存，用于写入 读写目录
    byte[] m_ReadPathFileListData;
    byte[] m_ServerFileListData;

    internal class DownFileInfo
    {
        public string url;
        public string fileName;
        public DownloadHandler fileData;
    }
    // 下载文件数量
    int m_DownloadCount;

    /// <summary>
    /// 下载单个文件
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    IEnumerator DownLoadFile(DownFileInfo info, Action<DownFileInfo> Complete)
    {
        // 从url请求资源
        UnityWebRequest webRequest = UnityWebRequest.Get(info.url);
        yield return webRequest.SendWebRequest();
        if (webRequest.isHttpError || webRequest.isNetworkError)
        {
            Debug.LogError("下载文件出错：" + info.url);
            yield break;

            // TODO:重试 / 重试次数
        }
        yield return new WaitForSeconds(0.2f);
        info.fileData = webRequest.downloadHandler;
        // 回调完成
        Complete?.Invoke(info);
        // 释放资源
        webRequest.Dispose();
    }

    /// <summary>
    /// 下载多个文件
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
    /// 获取文件信息(解析filelist文件)
    /// </summary>
    /// <returns></returns>
    private List<DownFileInfo> GetFileList(string fileDate, string path)
    {
        // windosw写入txt文件会存在一个回车符
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
        {   // 如果只读目录有资源包，则释放资源到读写目录
            ReleaseResources();
        }
        else
        {   // 更新流程，从服务器下载资源到读写目录
            CheckUpdate();
        }
    }

    private bool IsFirstInstall()
    {
        // 只读目录filelist
        bool isExisteReadPath = FileUtil.IsExists(Path.Combine(PathUtil.ReadPath, AppConst.FileListName));
        // 读写目录filelist
        bool isExisteReadWritePath = FileUtil.IsExists(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName));

        return isExisteReadPath && !isExisteReadWritePath;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    private void ReleaseResources()
    {
        m_DownloadCount = 0;
        string url = Path.Combine(PathUtil.ReadPath, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        // 根据只读目录filelist释放资源到读写目录
        StartCoroutine(DownLoadFile(info, OnDownLoadReadPathFileListComplete));
    }

    /// <summary>
    /// 解析filelist并释放资源
    /// </summary>
    private void OnDownLoadReadPathFileListComplete(DownFileInfo fileList)
    {
        // 缓存filelist的data数据
        m_ReadPathFileListData = fileList.fileData.data;
        // 解析只读目录的filelist
        List<DownFileInfo> fileInfos = GetFileList(fileList.fileData.text, PathUtil.ReadPath);
        // 释放filelist中所有文件
        StartCoroutine(DownLoadFile(fileInfos, OnReleaseFileComplete, OnRelsaseAllFileComplete));

        loadingUI.InitProgress(fileInfos.Count, "正在释放资源，不消耗流量...");
    }

    /// <summary>
    /// 写入文件到读写目录（每当下载完一个文件时）
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
    /// 所有文件下载完成时，在读写目录写入filelist
    /// </summary>
    private void OnRelsaseAllFileComplete()
    {
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ReadPathFileListData);
        CheckUpdate();
    }

    /// <summary>
    /// 检查更新
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
            loadingUI.InitProgress(downListFiles.Count, "正在更新资源...");
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
        loadingUI.InitProgress(0, "正在载入...");
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
