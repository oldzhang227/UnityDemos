using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;


/// <summary>
/// 文件操作类
/// </summary>
public class FileUtils
{

    /// <summary>
    /// Byte转为字符串
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string ByteToString(long bytes)
    {
        if(bytes > 1024 * 1024 * 1024)
        {
            return string.Format("{0:0.00}G", bytes * 1.0f / (1024 * 1024 * 1024));
        }

        if (bytes > 1024 * 1024)
        {
            return string.Format("{0:0.00}M", bytes * 1.0f / (1024 * 1024));
        }

        if (bytes > 1024)
        {
            return string.Format("{0:0.00}K", bytes * 1.0f / (1024));
        }

        return string.Format("{0}B", bytes);   
    }


    /// <summary>
    /// 创建文件夹
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static bool CreateDir(string dir)
    {
        try
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
        return false;
    }

    /// <summary>
    /// 创建文件，如果文件存在则清空
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static bool CreateFile(string file)
    {
        try
        {
            if (!File.Exists(file))
            {
                string dir = Path.GetDirectoryName(file);
                CreateDir(dir);
                FileStream fs = File.Create(file);
                fs.Close();
            }
            else
            {
                FileStream fs = File.OpenWrite(file);
                fs.SetLength(0);
                fs.Close();
            }
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
        return false;
    }


    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static bool DeleteFile(string file)
    {
        try
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
        return false;
    }

    /// <summary>
    /// 把一个文件写入到另一个文件中
    /// </summary>
    /// <param name="destFile"></param>
    /// <param name="srcFile"></param>
    /// <returns></returns>
    public static bool PushFile(string destFile, string srcFile)
    {
        try
        {
            byte[] bytes = new byte[1024];
            FileStream fsDest = new FileStream(destFile, FileMode.Append, FileAccess.Write);
            FileStream fsSrc = new FileStream(srcFile, FileMode.Open, FileAccess.Read);
            int count = 0;
            while((count = fsSrc.Read(bytes, 0, bytes.Length)) > 0)
            {
                fsDest.Write(bytes, 0, count);
            }
            fsSrc.Close();
            fsDest.Close();
            return true;
        }
        catch(System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }

        return false;
    }


    /// <summary>
    /// 计算文件的MD5
    /// </summary>
    public static string GetFileMD5(string file)
    {
        try
        {
            if(!File.Exists(file))
            {
                return string.Empty;
            }
            FileStream stream = File.Open(file, FileMode.Open);
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(stream);
            byte[] b = md5.Hash;
            md5.Clear();
            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < b.Length; i++)
            {
                sb.Append(b[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
        return string.Empty;
    }
}
