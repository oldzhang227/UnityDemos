using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;



public class TestDownload : MonoBehaviour {

    public Button button;
    public Text text_button;
    public Text text_progress;

    public enum DownloadState
    {
        WaitStart,
        Download,
        WaitContine,
    }

    public DownloadState state = DownloadState.WaitStart;


    // Use this for initialization
    void Start () {

        button.onClick.AddListener(OnClick);
        SetState(DownloadState.WaitStart);
    }


    void SetState(DownloadState s)
    {
        state = s;
        UpdateText();
    }


    void OnClick()
    {
        switch (state)
        {
            case DownloadState.WaitStart:
                {
                    StartDownload();
                }
                break;
            case DownloadState.WaitContine:
                {
                    DownLoadManager.GetInstance().Continue();
                }
                break;
        }
       
    }


    void UpdateText()
    {
        switch(state)
        {
            case DownloadState.WaitStart:
                {
                    text_button.text = "点击下载";
                }
                break;
            case DownloadState.Download:
                {
                    text_button.text = "正在下载";
                }
                break;
            case DownloadState.WaitContine:
                {
                    text_button.text = "点击恢复";
                }
                break;
        }


    }

    void StartDownload()
    {
        string url = "https://wod-1258597549.cos.ap-shanghai.myqcloud.com/test";
        for (int i = 0; i < 9; ++i)
        {
            DownLoadTask task = new DownLoadTask();
            task.url = string.Format("{0}/{1}.zip",url, i);
            task.path = task.url.Replace(url, Application.persistentDataPath);
            DownLoadManager.GetInstance().DoTask(task);
        }
        StartCoroutine(WaitDownLoad());
        SetState(DownloadState.Download);
    }

    IEnumerator WaitDownLoad()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);
        while (true)
        {
            bool isPause = DownLoadManager.GetInstance().IsPause();
            bool isFailed = DownLoadManager.GetInstance().IsFaied();
            long downLoadSize = DownLoadManager.GetInstance().GetDownLoadSize();
            long speed = DownLoadManager.GetInstance().GetSpeed();
            if(isPause)
            {
                if(!isFailed)
                {
                    text_progress.text = "下载成功";
                    SetState(DownloadState.WaitStart);
                    UpdateText();
                    break;
                }
                else
                {
                    if(state != DownloadState.WaitContine)
                    {
                        text_progress.text = "下载中断，等待继续...";
                        SetState(DownloadState.WaitContine);
                    }
                }
            }
            else
            {
                text_progress.text = string.Format("下载中..{0}, 速度：{1}/s", FileUtils.ByteToString(downLoadSize), FileUtils.ByteToString(speed));
            }
            yield return waitForSeconds;
        }
     
        yield return null;
    }
}
