using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;

public class FileUtil
{
    /// <summary>
    /// 检测文件是否存在
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsExists(string path)
    {
        FileInfo file = new FileInfo(path);
        return file.Exists;
    }

    /// <summary>
    /// 写入文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="data"></param>
    public static void WriteFile(string path, byte[] data)
    {
        // 标准化路径
        path = PathUtil.GetStandardPath(path);
        // 文件夹的路径
        string dir = path.Substring(0, path.LastIndexOf('/'));
        // 文件夹是否存在
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        // 文件是否存在
        FileInfo file = new FileInfo(path);
        if (file.Exists)
        {
            file.Delete();
        }
        // 写入文件
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
                fs.Close();
            }
        }
        catch(IOException e)
        {
            Debug.LogError(e.Message);
        }
    }
}
