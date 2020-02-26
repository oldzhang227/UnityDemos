using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildEditor
{
    public static string RESOURCES_DIR = "Assets/Resources";


 
    [MenuItem("Tool/BuildAsssetBundle")]
    public static void BuildAssetBundle()
    {
        List<AssetBundleBuild> abBuildList = new List<AssetBundleBuild>();
        abBuildList.AddRange(CollectDirList(RESOURCES_DIR));
        abBuildList.AddRange(CollectSceneList());
        string outputPath = Application.streamingAssetsPath + "/assetbundles/" + GetPlatformName();
        if(!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        BuildPipeline.BuildAssetBundles(outputPath, abBuildList.ToArray(), BuildAssetBundleOptions.DeterministicAssetBundle 
            | BuildAssetBundleOptions.ChunkBasedCompression 
            | BuildAssetBundleOptions.DisableWriteTypeTree, 
            EditorUserBuildSettings.activeBuildTarget);

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 获取相对Resources的路径
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    public static string GetResourcesPath(string assetPath)
    {
        int index = assetPath.IndexOf(RESOURCES_DIR);
        if (index != -1)
        {
            return assetPath.Substring(index + RESOURCES_DIR.Length + 1);
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 按照目录收集ab列表
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static AssetBundleBuild[] CollectDirList(string dir)
    {
        List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();
        string[] dirs = Directory.GetDirectories(dir, "*", SearchOption.AllDirectories);
        for (int i = 0; i < dirs.Length; ++i)
        {
            string abName = dirs[i].Replace("\\", "/");
            abName = GetResourcesPath(abName);
            List<string> assetNames = new List<string>();
            string[] assets = Directory.GetFiles(dirs[i]);
            for (int j = 0; j < assets.Length; ++j)
            {
                string assetName = assets[j].Replace("\\", "/");
                if (assetName.EndsWith(".meta") || assetName.EndsWith(".unity"))
                {
                    continue;
                }
                int assetIndex = assetName.IndexOf("Assets/");
                if (assetIndex != -1)
                {
                    assetName = assetName.Substring(assetIndex);
                }
                else
                {
                    continue;
                }
                assetNames.Add(assetName);
            }
            if (assetNames.Count > 0)
            {
                AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
                assetBundleBuild.assetNames = assetNames.ToArray();
                assetBundleBuild.assetBundleName = abName + ".ab";
                buildList.Add(assetBundleBuild);
            }
        }
        return buildList.ToArray();
    }


    /// <summary>
    /// 收集场景列表
    /// </summary>
    /// <returns></returns>
    public static AssetBundleBuild[] CollectSceneList()
    {
        List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();
        foreach (EditorBuildSettingsScene scene in  EditorBuildSettings.scenes)
        {
            if(!scene.path.StartsWith(RESOURCES_DIR) || !scene.enabled)
            {
                continue;
            }
            string abName = GetResourcesPath(scene.path);
            AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
            assetBundleBuild.assetBundleName = abName.Replace(".unity", ".ab");
            assetBundleBuild.assetNames = new string[] { scene.path };
            buildList.Add(assetBundleBuild);
        }
        return buildList.ToArray();
    }



    /// <summary>
    /// 获取当前的平台名
    /// </summary>
    /// <returns></returns>
    public static string GetPlatformName()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return "windows";
#elif UNITY_ANDROID
        return "android";
#elif UNITY_IOS
        return "ios";
#endif
    }
}
