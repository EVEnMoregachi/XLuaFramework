using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BuildTool : Editor
{
    [MenuItem("Tools/Build Windows Bundle")]
    static void BuildWindowsBundle()
    {
        Build(BuildTarget.StandaloneWindows);
    }

    [MenuItem("Tools/Build Android Bundle")]
    static void BuildAndroidBundle()
    {
        Build(BuildTarget.Android);
    }
    [MenuItem("Tools/Build iOS Bundle")]
    static void BuildBundle()
    {
        Build(BuildTarget.iOS);
    }

    static void Build(BuildTarget target)
    {
        List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();

        // 文件信息列表
        List<string> bundleInfos = new List<string>();

        // 按文件打包(GetFiles)
        string[] files = Directory.GetFiles(PathUtil.BuildResoursesPath, "*", SearchOption.AllDirectories);

        for (int i = 0; i < files.Length; i++)
        {   // 排除meta文件
            if (files[i].EndsWith(".meta")) continue;

            string filename = PathUtil.GetStandardPath(files[i]);
            Debug.Log("file:" + filename);
            // 创建AB包
            AssetBundleBuild assetBundle = new AssetBundleBuild();
            string assetName = PathUtil.GetUnityPath(filename);
            assetBundle.assetNames = new string[] { assetName };
            string bundleName = filename.Replace(PathUtil.BuildResoursesPath, "").ToLower();
            assetBundle.assetBundleName = bundleName + AppConst.BundleEstension;
            assetBundleBuilds.Add(assetBundle);

            // 添加文件和依赖信息
            List<string> dependenceInfo = GetDependence(assetName);
            string bundleInfo = assetName + "|" + bundleName + AppConst.BundleEstension;
            if (dependenceInfo.Count > 0)
                bundleInfo = bundleInfo + "|" + string.Join("|", dependenceInfo);

            bundleInfos.Add(bundleInfo);
        }

        if (Directory.Exists(PathUtil.BundleOutPath))
            Directory.Delete(PathUtil.BundleOutPath, true);
        Directory.CreateDirectory(PathUtil.BundleOutPath);

        File.WriteAllLines(PathUtil.BundleOutPath + "/" + AppConst.FileListName, bundleInfos);

        BuildPipeline.BuildAssetBundles(PathUtil.BundleOutPath, assetBundleBuilds.ToArray(), BuildAssetBundleOptions.None, target);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 获取依赖文件列表并且排除掉无用资源
    /// </summary>
    /// <param name="curFile"></param>
    /// <returns></returns>
    static List<string> GetDependence(string curFile)
    {
        string[] files = AssetDatabase.GetDependencies(curFile, true);

        List<string> dependence = files.Where(file =>
            !file.EndsWith(".cs") &&                           // 排除脚本
            !file.Equals(curFile) &&                           // 排除自己
            !file.Contains("/Editor") &&                       // 排除编辑器资源
            !file.Contains("/Gizmos") &&                       // 排除Gizmos图标
            !file.EndsWith(".psd") &&                          // 排除TMP图标等psd资源
            !file.EndsWith(".ttf") &&                          // 排除字体源文件（除非你确实用到）
            !file.StartsWith("Packages/com.unity.textmeshpro") && // TMP包内资源
            !file.StartsWith("Assets/TextMesh Pro/")           // TMP运行时资源（你只保留用到的字体就行）
        ).ToList();

        return dependence;
    }
}
