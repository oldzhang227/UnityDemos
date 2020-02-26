using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Asset加载器基类
/// </summary>
public abstract class AssetLoader
{
    public string assetName { get; protected set; }
    public float progress { get; protected set; }
    public bool isDone { get; protected set; }
    public bool isAsync { get; protected set; }
    public bool isCanel { get; protected set; }
    public bool isLoading { get; protected set; }
    public bool isScene { get; protected set; }
    public AssetInfo assetInfo { get; protected set; }

    public bool isSingle;

    public Action<AssetInfo> finishCallBack;

    public Action<float> progressCallBack;


    public AssetLoader(string name, bool async, bool single = false)
    {
        assetName = name;
        isAsync = async;
        progress = 0.0f;
        finishCallBack = null;
        progressCallBack = null;
        isDone = false;
        isCanel = false;
        isLoading = false;
        assetInfo = null;
        isSingle = single;
        isScene = false;
    }

    ~AssetLoader()
    {
        ReleaseAsset();
    }


    public virtual void Start()
    {
        if(isDone || isCanel)
        {
            return;
        }
        isLoading = true;
        assetInfo = AssetManager.Instance.GetAssetInfo(assetName);
        isScene = assetInfo.isScene;
        RetainAsset();
        if (assetInfo != null && assetInfo.Exist())
        {
            Exit();
        }
    }

    public virtual void Excute()
    {
        if(!isCanel)
        {
            if (progressCallBack != null)
            {
                progressCallBack(progress);
            }
        }

    }
    public virtual void Exit()
    {
        isDone = true;
        isLoading = false;
        progress = 1.0f;
        
        if (!isCanel)
        {
            if (finishCallBack != null)
            {
                finishCallBack(assetInfo);
            }
        }
    }

    public void Cancel()
    {
        isCanel = true;
    }

    public void Reset()
    {
        assetName = string.Empty;
        progress = 0.0f;
        finishCallBack = null;
        progressCallBack = null;
        isDone = false;
        isCanel = false;
        isLoading = false;
        ReleaseAsset();
    }


    private void RetainAsset()
    {
        if (assetInfo != null)
        {
            assetInfo.Retain();
        }
        
    
    }

    private void ReleaseAsset()
    {
        if (assetInfo != null)
        {
            assetInfo.Release();
            assetInfo = null;
        }
    }
}
