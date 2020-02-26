using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 下载任务
/// </summary>
public class DownLoadTask
{
    public string url;
    public string path;
    public string md5;
    public long downLoadSize;
    public long totalSize;
    public bool isDone;
    public bool isSuccess;
    public byte[] buffer;
    public bool isCancel;
    public Action<DownLoadTask> callBack;


    public DownLoadTask()
    {
        Reset();
    }

    public void Reset()
    {
        url = string.Empty;
        path = string.Empty;
        md5 = string.Empty;
        downLoadSize = 0;
        totalSize = 0;
        isDone = false;
        isSuccess = false;
        buffer = null;
        isCancel = false;
        callBack = null;
    }

    public float Progress()
    {
        if(totalSize > 0)
        {
            return downLoadSize * 1.0f / totalSize;
        }
        else
        {
            return 0.0f;
        }
    }

    public bool IsDownLoadDone()
    {
        if(downLoadSize == totalSize)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Done(bool success)
    {
        isDone = true;
        isSuccess = success;
    }
}
