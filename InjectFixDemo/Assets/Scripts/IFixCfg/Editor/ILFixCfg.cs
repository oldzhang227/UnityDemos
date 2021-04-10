using System.Collections.Generic;
using IFix;
using System;
using System.Reflection;
using System.Linq;
using UnityEditor;
using System.Text;
using UnityEngine;
using System.IO;

[Configure]
public class ILFixCfg
{
    private static Assembly _mainAssembly = Assembly.Load("Assembly-CSharp");

    [IFix]
    static IEnumerable<Type> hotfix
    {
        get
        {
            var types = (from type in _mainAssembly.GetTypes()
                         where
                         (
                            type.Namespace != null &&
                            type.Namespace.StartsWith("Game") &&
                            !type.IsGenericType
                         )
                         select type
                         );

            return new List<Type>(types);
        }
    }

    [IFix.Filter]
    static bool Filter(System.Reflection.MethodInfo methodInfo)
    {
        return methodInfo.DeclaringType.FullName.Contains("Editor");
    }


    [MenuItem("InjectFix/Patched")]
    static void InjectfixEnable()
    {
        BuildTargetGroup buildTargetGroup = GetCurBuildTarget();
        string symbolsStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        string[] symbols = symbolsStr.Split(';');
        HashSet<string> symbolSet = new HashSet<string>();
        for(int i = 0; i < symbols.Length; ++i)
        {
            if(!symbolSet.Contains(symbols[i]))
            {
                symbolSet.Add(symbols[i]);
            }
        }
        if(!symbolSet.Contains("INJECTFIX_PATCH_ENABLE"))
        {
            symbolSet.Add("INJECTFIX_PATCH_ENABLE");
        }
        else
        {
            symbolSet.Remove("INJECTFIX_PATCH_ENABLE");
        }
        StringBuilder sb = new StringBuilder();
        foreach(string s in symbolSet)
        {
            sb.Append(s + ";");
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, sb.ToString());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static BuildTargetGroup GetCurBuildTarget()
    {
        BuildTarget buildTarget =  EditorUserBuildSettings.activeBuildTarget;
        switch(buildTarget)
        {
            case BuildTarget.Android:
                {
                    return BuildTargetGroup.Android;
                }
            case BuildTarget.iOS:
                {
                    return BuildTargetGroup.iOS;
                }
            default:
                {
                    return BuildTargetGroup.Standalone;
                }
        }
    }

    [MenuItem("InjectFix/Patched", true)]
    static bool InjectfixEnableChecked()
    {
#if INJECTFIX_PATCH_ENABLE
        Menu.SetChecked("InjectFix/Patched", true);
#else
        Menu.SetChecked("InjectFix/Patched", false);
#endif
        return true;
    }


    [MenuItem("InjectFix/Generate Warp Code")]
    static void GeneWarpCode()
    {
        var types = hotfix;

        HashSet<Type> typeSet = new HashSet<Type>();
        foreach (var type in types)
        {
            if(type.IsNestedPrivate)
            {
                continue;
            }
            if (typeof(Delegate).IsAssignableFrom(type))
            {
                typeSet.Add(type);
                continue;
            }
            if (type.IsInterface)
            {
                typeSet.Add(type);
            }
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                if(method.IsGenericMethod)
                {
                    continue;
                }
                var parameters = method.GetParameters();
                foreach (var parameter in parameters)
                {
                    if (typeof(Delegate).IsAssignableFrom(parameter.ParameterType) && !parameter.ParameterType.IsByRef)
                    {
                        typeSet.Add(parameter.ParameterType);
                    }
                }
            }
        }

        string warpFormat = @"
using System;
using System.Collections.Generic;


namespace IFix.Bridge
{
    [IFix.CustomBridge]
    public static class ILFixWarpBridge
    {
        static List<Type> bridge = new List<Type>()
        {
${TypeList}
        };
    }
}";

        StringBuilder sb = new StringBuilder();
        foreach(var type in typeSet)
        {
            sb.AppendFormat("\t\t\ttypeof({0}),\n", TypeToString(type));
        }
        string filePath = Application.dataPath + "/Scripts/IFixCfg/ILFixWarpBridge.cs";
        File.WriteAllText(filePath, warpFormat.Replace( "${TypeList}", sb.ToString()));
    }

    /// <summary>
    /// 主要处理泛型delegate以及类内部定义的delegate的tostring后的还原
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    static string TypeToString(Type type)
    {
        string typeStr = type.ToString();
        if (type.IsGenericType)
        {
            StringBuilder sb = new StringBuilder();
            Type defineType = type.GetGenericTypeDefinition();
            string defineStr = defineType.ToString();
            int index = defineStr.IndexOf("`");
            if (index != -1)
            {
                defineStr = defineStr.Substring(0, index);
            }
            sb.Append(defineStr);

            Type[] argTypes = type.GetGenericArguments();
            sb.Append("<");
            for (int i = 0; i < argTypes.Length; ++i)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }
                sb.Append(TypeToString(argTypes[i]));
            }
            sb.Append(">");
            typeStr = sb.ToString();
        }
        
        if(type.IsNested)
        {
            typeStr = typeStr.Replace("+", ".");
        }
        return typeStr;
    }
}
