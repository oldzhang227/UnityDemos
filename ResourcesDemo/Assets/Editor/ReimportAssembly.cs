using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

public class ReimportAssembly
{
    [MenuItem("Assets/Reimport Assemblies", false, 100)]
    public static void ReimportAssemblies()
    {
        string extentDir = EditorApplication.applicationContentsPath + "/UnityExtensions/Unity";
        string[] dlls = Directory.GetFiles(extentDir, "*.dll", SearchOption.AllDirectories);
        for(int i = 0; i < dlls.Length; ++i)
        {
            ReimportDll(dlls[i]);
        }
    }
    static void ReimportDll(string path)
    {
        if (File.Exists(path))
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.DontDownloadFromCacheServer);
        else
            Debug.LogError(string.Format("DLL not found {0}", path));
    }
}

