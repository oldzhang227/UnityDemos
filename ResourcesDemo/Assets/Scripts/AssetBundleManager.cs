using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AssetBundle管理
/// </summary>
public class AssetBundleManager : Singleton<AssetBundleManager>
{ 
    private Dictionary<string, AssetBundleInfo> _assetBundleInfos;
    private List<AssetBundleInfo> _loadingAssetBundleInfos;

    protected override void Init()
    {
        base.Init();
        LoadAssetBundleInfo();
    }

    public override void Release()
    {
        base.Release();
        _assetBundleInfos.Clear();
        _assetBundleInfos = null;
    }

    public void Update()
    {
        CheckLoadingAssetBundle();
    }

    /// <summary>
    /// 加载AssetBundle依赖关系
    /// </summary>
    private void LoadAssetBundleInfo()
    {
        string manifestName = AssetBundleInfo.GetPlatformName();
        string filePath = AssetBundleInfo.GetAssetBundleFilePath(manifestName);
        AssetBundle assetBundle = AssetBundle.LoadFromFile(filePath);
        if(assetBundle != null)
        {
            AssetBundleManifest assetBundleManifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (assetBundleManifest != null)
            {
                _assetBundleInfos = new Dictionary<string, AssetBundleInfo>();
                string[] allAssetBundles = assetBundleManifest.GetAllAssetBundles();
                for(int i = 0, count = allAssetBundles.Length; i < count; ++i)
                {
                    AssetBundleInfo assetBundleInfo = new AssetBundleInfo();
                    assetBundleInfo.name = allAssetBundles[i];
                    _assetBundleInfos.Add(assetBundleInfo.name, assetBundleInfo);
                }
                foreach(KeyValuePair<string, AssetBundleInfo> p in _assetBundleInfos)
                {
                    string[] depends = assetBundleManifest.GetAllDependencies(p.Key);
                    for (int i = 0, count = depends.Length; i < count; ++i)
                    {
                        AssetBundleInfo assetBundleInfo = _assetBundleInfos[depends[i]];
                        p.Value.AddDependAssetBundle(assetBundleInfo);
                        assetBundleInfo.AddRefAssetBundle(p.Value);
                    }
                }
            }
           
            assetBundle.Unload(true);
            assetBundle = null;
        }
    }

    /// <summary>
    /// 同步加载AssetBundle及其依赖
    /// </summary>
    /// <param name="abName"></param>
    /// <returns></returns>
    public AssetBundleInfo LoadAssetBundle(string abName)
    {
        AssetBundleInfo assetBundleInfo = GetAssetBundleInfo(abName);
        if (assetBundleInfo != null)
        {
            List<AssetBundleInfo> depends = assetBundleInfo.GetDependAssetBundeInfos();
            if (depends != null)
            {
                for (int i = 0, count = depends.Count; i < count; ++i)
                {
                    depends[i].LoadAssetBundle();
                }
            }
            assetBundleInfo.LoadAssetBundle();
        }
        return assetBundleInfo;
    }

    /// <summary>
    /// 异步加载AssetBundle及其依赖
    /// </summary>
    /// <param name="abName"></param>
    /// <returns></returns>
    public AssetBundleInfo LoadAssetBundleAsync(string abName)
    {
        AssetBundleInfo assetBundleInfo = GetAssetBundleInfo(abName);
        if(assetBundleInfo != null)
        {
            List<AssetBundleInfo> depends = assetBundleInfo.GetDependAssetBundeInfos();
            if (depends != null)
            {
                for (int i = 0, count = depends.Count; i < count; ++i)
                {
                    depends[i].LoadAssetBundleAsync();
                    AddLoadingAssetBundle(depends[i]);
                }
            }
            assetBundleInfo.LoadAssetBundleAsync();
            AddLoadingAssetBundle(assetBundleInfo);
        }
        return assetBundleInfo;
    }

    /// <summary>
    /// 获取AssetBundle
    /// </summary>
    /// <param name="abName"></param>
    /// <returns></returns>
    public AssetBundleInfo GetAssetBundleInfo(string abName)
    {
        AssetBundleInfo assetBundleInfo = null;
        if (_assetBundleInfos.TryGetValue(abName, out assetBundleInfo))
        {
            return assetBundleInfo;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 卸载没有引用的AssetBundle
    /// </summary>
    /// <param name="unloadAllLoadedObjects"></param>
    public void UnloadUnusedAssetBundle(bool unloadAllLoadedObjects)
    {
        System.GC.Collect();
        foreach (KeyValuePair<string, AssetBundleInfo> p in _assetBundleInfos)
        {
            p.Value.UnloadUnused(unloadAllLoadedObjects);
        }
    }
    /// <summary>
    /// 卸载所有的AssetBundle
    /// </summary>
    /// <param name="unloadAllLoadedObjects"></param>
    public void UnloadAllAssetBundle(bool unloadAllLoadedObjects)
    {
        foreach(KeyValuePair<string, AssetBundleInfo> p in _assetBundleInfos)
        {
            p.Value.ForceUnload(unloadAllLoadedObjects);
        }
    }

    /// <summary>
    /// 检查正在加载的AssetBundle
    /// </summary>
    private void CheckLoadingAssetBundle()
    {
        if (_loadingAssetBundleInfos != null)
        {
            for (int i = _loadingAssetBundleInfos.Count - 1; i >= 0; --i)
            {
                if (!_loadingAssetBundleInfos[i].CheckLoading())
                {
                    _loadingAssetBundleInfos.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// 添加到加载列表
    /// </summary>
    /// <param name="assetBundleInfo"></param>
    private void AddLoadingAssetBundle(AssetBundleInfo assetBundleInfo)
    {
        if (_loadingAssetBundleInfos == null)
        {
            _loadingAssetBundleInfos = new List<AssetBundleInfo>();
        }
        if (assetBundleInfo.IsLoading() && !_loadingAssetBundleInfos.Contains(assetBundleInfo))
        {
            _loadingAssetBundleInfos.Add(assetBundleInfo);
        }
    }
}
