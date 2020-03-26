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
        //string url = "http://woda.jijiagames.com/cdn/wod_android_1.0.0.227915_200321_1804_pr2v6_update/";
        string url = "https://down.qq.com/wod/cdn/wod_android_1.0.0.227915_200321_1804_pr2v6_update/";

        Debug.Log(Application.persistentDataPath);
        List<string> files = new List<string>() {
            "0e3b671686680b573737c89a6f2c251f.zip",
            "0f4f049806f0e5f6f4ba2583ae7efb07.zip",
            "1c81d60584f69db4c294699421c84549.zip",
            "1c67094f3671f16a636c77636e979a98.zip",
            "1ccfb16671d241eaacc145f55b923252.zip",
            "2a26c158c6c4533dcfd04285a77f8369.zip",
            "2d472e915235e8174e1a1f3fc74b0656.zip",
            "2f631b41f6f31db87da7d941fb0027fe.zip",
            "6a6783933b4a8d5de63a3bdc53878289.zip" };
 
        for (int i = 0; i < files.Count; ++i)
        {
            DownLoadTask task = new DownLoadTask();
            task.url = url + files[i];
            task.path = task.url.Replace(url, Application.persistentDataPath + "/");
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
