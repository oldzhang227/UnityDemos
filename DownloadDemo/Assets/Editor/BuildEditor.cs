using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class BuildEditor {



    public static void Test()
    {



        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, Application.dataPath + "/../test.apk", BuildTarget.Android, BuildOptions.None);


    }

}
