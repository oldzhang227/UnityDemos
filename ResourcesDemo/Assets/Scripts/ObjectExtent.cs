using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// GameObject拓展方法
/// </summary>
public static class ObjectExtent
{

    public static void DestroyUsePool(this GameObject gameObject, bool usePool = true)
    {
        if(usePool)
        {
            ObjectPoolManager.Instance.InPool(gameObject);
        }
        else
        {
            ObjectPoolManager.Instance.Destroy(gameObject);
        }
    }


    public static void StoreUsePool(this GameObject gameObject, string assetName)
    {
        ObjectPoolManager.Instance.StoreObject(assetName, gameObject);
    }
}



