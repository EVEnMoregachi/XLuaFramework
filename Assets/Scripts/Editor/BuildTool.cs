using System;
using System.Collections.Generic;
using System.IO;
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
        // 按文件打包(GetFiles)
        string[] files = Directory.GetFiles(PathUtil.BuildResoursesPath, "*", SearchOption.AllDirectories);

        for (int i = 0; i < files.Length; i++)
        {   // 排除meta文件
            if (files[i].EndsWith(".meta")) continue;

            string filename = PathUtil.GetStandardPath(files[i]);
            Debug.Log("file:" + filename);
            AssetBundleBuild assetBundle = new AssetBundleBuild();
            string assetName = PathUtil.GetUnityPath(filename);
            assetBundle.assetNames = new string[] { assetName };
            string bundleName = filename.Replace(PathUtil.BuildResoursesPath, "").ToLower();
            assetBundle.assetBundleName = bundleName + ".ab";
            assetBundleBuilds.Add(assetBundle);
        }

        if (Directory.Exists(PathUtil.BundleOutPath))
            Directory.Delete(PathUtil.BundleOutPath, true);
        Directory.CreateDirectory(PathUtil.BundleOutPath);

        BuildPipeline.BuildAssetBundles(PathUtil.BundleOutPath, assetBundleBuilds.ToArray(), BuildAssetBundleOptions.None, target);
    }
}

