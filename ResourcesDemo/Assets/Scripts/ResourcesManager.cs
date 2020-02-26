using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 资源加载与管理
/// </summary>
public class ResourcesManager : Singleton<ResourcesManager>
{
    private List<AssetLoader> _loadingAssetLoaders;
    private List<AssetLoader> _waitLoadAssetLoaders;
    private List<int> _removeLoadingAssetLoaders;
    private int _maxLoadingAssetLoaderCount = 20;

    protected override void Init()
    {
        base.Init();
        _loadingAssetLoaders = new List<AssetLoader>();
        _waitLoadAssetLoaders = new List<AssetLoader>();
        _removeLoadingAssetLoaders = new List<int>();
    }

    public override void Release()
    {
        base.Release();
    }

    public void Update()
    {
        ObjectPoolManager.Instance.Update();
        AssetManager.Instance.Update();
        AssetBundleManager.Instance.Update();
        CheckLoadingAssetLoader();
    }


    /// <summary>
    /// 加载场景
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="single"></param>
    /// <param name="async"></param>
    /// <param name="useAssetBundle"></param>
    /// <returns></returns>
    public AssetLoader LoadScene(string sceneName, bool single, bool async, bool useAssetBundle)
    {
        AssetLoader assetLoader = null;
        if (useAssetBundle)
        {
            assetLoader = new AssetBundleLoader(sceneName, async);
        }
        else
        {
            assetLoader = new ResourcesLoader(sceneName, async);
        }
        assetLoader.isSingle = single;
        if(async)
        {
            _waitLoadAssetLoaders.Add(assetLoader);
        }
        else
        {
            assetLoader.Start();
        }
        return assetLoader;
    }


    /// <summary>
    /// 同步加载Asset
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="useAssetBundle"></param>
    /// <returns></returns>
    public AssetInfo LoadAsset(string assetName, bool useAssetBundle)
    {
        AssetLoader assetLoader = null;
        if (useAssetBundle)
        {
            assetLoader = new AssetBundleLoader(assetName, false);
        }
        else
        {
            assetLoader = new ResourcesLoader(assetName, false);
        }
        assetLoader.Start();
        return assetLoader.assetInfo;
    }

    /// <summary>
    /// 异步加载Asset
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="useAssetBundle"></param>
    /// <param name="finishCallBack"></param>
    /// <returns></returns>
    public AssetLoader LoadAssetAsync(string assetName, bool useAssetBundle, Action<AssetInfo> finishCallBack)
    {
        AssetLoader assetLoader = null;
        if (useAssetBundle)
        {
            assetLoader = new AssetBundleLoader(assetName, true);
        }
        else
        {
            assetLoader = new ResourcesLoader(assetName, true);
        }
        assetLoader.finishCallBack = finishCallBack;
        _waitLoadAssetLoaders.Add(assetLoader);
        return assetLoader;
    }

    /// <summary>
    /// 检查正在加载的任务
    /// </summary>
    private void CheckLoadingAssetLoader()
    {
        AssetLoader assetLoader = null;
        for(int i = 0, count = _loadingAssetLoaders.Count; i < count; ++i)
        {
            assetLoader = _loadingAssetLoaders[i];
            assetLoader.Excute();
            if (assetLoader.isDone == true)
            {
                _removeLoadingAssetLoaders.Add(i);
            }
        }
        for(int i = _removeLoadingAssetLoaders.Count - 1; i >= 0; --i)
        {
            _loadingAssetLoaders.RemoveAt(_removeLoadingAssetLoaders[i]);
        }
        _removeLoadingAssetLoaders.Clear();

        while(_waitLoadAssetLoaders.Count > 0)
        {
            if(_loadingAssetLoaders.Count >= _maxLoadingAssetLoaderCount)
            {
                break;
            }
            assetLoader = _waitLoadAssetLoaders[0];
            if(!assetLoader.isCanel)
            {
                assetLoader.Start();
                if(assetLoader.isLoading)
                {
                    _loadingAssetLoaders.Add(assetLoader);
                }
            }
            _waitLoadAssetLoaders.RemoveAt(0);
        }
    }
}
