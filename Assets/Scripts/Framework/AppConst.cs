using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum GameMode
{ 
    EditorMode,             // 编辑器模式
    PackageMode,            // 只读目录释放资源模式
    UpdateMode,             // 服务器更新获取资源模式
}

public class AppConst
{
    public const string BundleEstension = ".ab";

    public const string FileListName = "filelist.txt";

    public static GameMode GameMode = GameMode.EditorMode;

    public static bool OpenLog = true;
    // 热更资源地址
    public const string ResourcesUrl = "http://192.168.1.13/AssetBundles";
}
