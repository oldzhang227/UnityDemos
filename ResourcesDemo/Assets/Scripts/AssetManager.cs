using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Asset管理
/// </summary>
public class AssetManager : Singleton<AssetManager>
{
    private Dictionary<string, AssetInfo> _assetInfos = null;
    // 卸载异步句柄
    private AsyncOperation _unloadAsyncOperation = null;
    // 是否等待卸载
    private bool _waitUnload = false;

    protected override void Init()
    {
        base.Init();
        _assetInfos = new Dictionary<string, AssetInfo>();
    }

    public override void Release()
    {
        base.Release();
        foreach(KeyValuePair<string, AssetInfo> p in _assetInfos)
        {
            p.Value.ClearAsset();
        }
        _assetInfos.Clear();
        _assetInfos = null;
    }


    public void Update()
    {
        if (_unloadAsyncOperation != null && _unloadAsyncOperation.isDone)
        {
            _unloadAsyncOperation = null;
            AssetBundleManager.Instance.UnloadUnusedAssetBundle(true);
        }

        if(_waitUnload && _unloadAsyncOperation == null)
        {
            _waitUnload = false;
            UnloadunusedAsset();
        }
    }


    /// <summary>
    /// 获取Asset
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public AssetInfo GetAssetInfo(string assetName)
    {
        AssetInfo assetInfo = null;
        if (!_assetInfos.TryGetValue(assetName, out assetInfo))
        { 
            assetInfo = new AssetInfo();
            assetInfo.name = assetName;
            assetInfo.isScene = assetName.EndsWith(".unity");
            _assetInfos.Add(assetName, assetInfo);
        }
        return assetInfo;
    }

    /// <summary>
    /// 异步过程，仅仅标记，在Update中执行
    /// </summary>
    public void UnloadunusedAssetAsync()
    {
        _waitUnload = true;
    }

    /// <summary>
    /// 是否正在卸载Asset
    /// </summary>
    /// <returns></returns>
    public bool IsUnloadingunusedAsset()
    {
        if(_waitUnload || _unloadAsyncOperation != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 卸载未使用的Asset
    /// </summary>
    private void UnloadunusedAsset()
    {
        System.GC.Collect();
        foreach (KeyValuePair<string, AssetInfo> p in _assetInfos)
        {
            p.Value.UnloadUnused();
        }
        System.GC.Collect();
        _unloadAsyncOperation = Resources.UnloadUnusedAssets();
    }
}
