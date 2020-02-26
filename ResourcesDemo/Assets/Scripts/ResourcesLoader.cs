using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 采用Resources.Load加载
/// </summary>
public class ResourcesLoader : AssetLoader
{
    public ResourcesLoader(string name, bool async) :base(name, async)
    {

    }

    public override void Start()
    {
        base.Start();
        if (!isLoading)
        {
            return;
        }
        if (isScene == true)
        {
            assetInfo.LoadScene(isSingle, isAsync);
        }
        else
        {
            assetInfo.LoadByResources(isAsync);
        }
        if(!isAsync)
        {
            Exit();
        }
    }
    public override void Excute()
    {
        base.Excute();
        if (isScene)
        {
            if (!assetInfo.CheckSceneLoading())
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
            if (!assetInfo.CheckResourceLoading())
            {
                Exit();
            }
            else
            {
                progress = assetInfo.LoadingByResourcesProgress();
            }  
        }      
    }

    public override void Exit()
    {
        base.Exit();
    }
}
