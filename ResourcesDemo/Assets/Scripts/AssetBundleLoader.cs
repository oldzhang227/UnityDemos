using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 采用AssetBundle加载
/// </summary>
public class AssetBundleLoader : AssetLoader
{
    private AssetBundleInfo _assetBundleInfo;
    private bool _loadingAsset;
    public AssetBundleLoader(string name, bool async) : base(name, async)
    {
        _loadingAsset = false;
        _assetBundleInfo = null;
    }

    public override void Start()
    {
        base.Start();
        if (!isLoading)
        {
            return;
        }
        string abName = assetInfo.GetAssetBundleName();
        if (isAsync)
        {
            _assetBundleInfo = AssetBundleManager.Instance.LoadAssetBundleAsync(abName);
            if (_assetBundleInfo != null)
            {
                _assetBundleInfo.AddRefAssetLoader(this);
            }
        }
        else
        {
            _assetBundleInfo = AssetBundleManager.Instance.LoadAssetBundle(abName);
            if(_assetBundleInfo != null)
            {
                if(isScene == true)
                {
                    assetInfo.LoadScene(isSingle, false);
                }
                else
                {
                    assetInfo.LoadByAssetBundle(_assetBundleInfo.assetBundle, false);
                }
                Exit();
            }
            
        }
    }
    public override void Excute()
    {
        base.Excute();
        if (_assetBundleInfo == null)
        {
            Exit();
            return;
        }
        if (_assetBundleInfo.IsLoadingByDepends())
        {
            return;
        }
        if (!_loadingAsset)
        {
            _loadingAsset = true;
            if (isScene == true)
            {
                assetInfo.LoadScene(isSingle, true);
            }
            else
            {
                assetInfo.LoadByAssetBundle(_assetBundleInfo.assetBundle, true);
            }
        }
        else
        {
            if(isScene)
            {
                if(!assetInfo.CheckSceneLoading())
                {
                    Exit();
                }
                else
                {
                    progress = assetInfo.LoadingSceneProgress();
                }
               
            }
            else
            {
                if(!assetInfo.CheckAssetBundleLoading())
                {
                    Exit();
                }
                else
                {
                    progress = assetInfo.LoadingByAssetBundleProgress();
                }           
            }
        }
    }
    public override void Exit()
    {
        base.Exit();
        assetInfo.AddAssetBundleRef(_assetBundleInfo);
        if(isAsync && _assetBundleInfo != null)
        {
            _assetBundleInfo.RemoveRefAssetLoader(this);
        }
        _assetBundleInfo = null;
        _loadingAsset = false;
    }

}