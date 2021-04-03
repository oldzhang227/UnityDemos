using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ExportAnimEditor : EditorWindow
{
    private Animation _animation;
    private HashSet<string> _selectSet;
    private AnimFrame _animFrame;
    private Transform[] _bones;
    private Animation _sampleAnimation;
    enum AnimFrame
    {
        Frame_15,
        Frame_30,
        Frame_45,
        Frame_60
    }

    [MenuItem("Tools/ExportAnim")]
    public static void Open()
    {
        GetWindow<ExportAnimEditor>().Show();
    }

    private void OnGUI()
    {
        _animation = (Animation)EditorGUILayout.ObjectField("模型：", _animation, typeof(Animation), false);   
        if(_animation == null)
        {
            EditorGUILayout.HelpBox("请选择带有Animation组件的prefab", MessageType.Error);
            return;
        }
        GUILayout.Space(5);
        GUILayout.Label("动画列表：");
        int index = 0;
        foreach(AnimationState a in _animation)
        {
            EditorGUILayout.BeginHorizontal();
            {
                bool isSelect = _selectSet.Contains(a.name);
                bool newState = EditorGUILayout.Toggle(isSelect);
                if(isSelect != newState)
                {
                    if(!newState)
                    {
                        _selectSet.Remove(a.name);
                    }
                    else
                    {
                        _selectSet.Add(a.name);
                    }
                }
                EditorGUILayout.ObjectField(a.clip, typeof(AnimationClip), false);
            }
            EditorGUILayout.EndHorizontal();
            index++;
        }
        GUILayout.Space(5);
        if (index == 0)
        {
            EditorGUILayout.HelpBox("动画列表为空！", MessageType.Error);
            return;
        }
        GUILayout.Space(5);
        _animFrame = (AnimFrame)EditorGUILayout.EnumPopup("动画帧率:", _animFrame);

        GUILayout.Space(5);
        if (GUILayout.Button("导出动画数据"))
        {
            string dir = EditorUtility.SaveFolderPanel("导出动画数据", "", "");
            if(!string.IsNullOrEmpty(dir))
            {
                dir = dir.Replace("\\", "/");
                if(!dir.StartsWith(Application.dataPath))
                {
                    return;
                }
                dir = dir.Replace(Application.dataPath, "Assets");
                ExportAnim(dir);
            }
        }

    }

    private void OnEnable()
    {
        _animation = null;
        _selectSet = new HashSet<string>();
        _animFrame = AnimFrame.Frame_30;
    }

    private void ExportAnim(string dir)
    {
        GameObject gameObject = GameObject.Instantiate(_animation.gameObject);
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.localScale = Vector3.one;
        _sampleAnimation = gameObject.GetComponent<Animation>();
        _bones = gameObject.GetComponentInChildren<SkinnedMeshRenderer>().bones;
        foreach (string name in _selectSet)
        {
            ExportAnimData(_sampleAnimation[name], ((int)_animFrame + 1) * 15, dir);
        }
        GameObject.DestroyImmediate(gameObject);
    }

    private void ExportAnimData(AnimationState animationState, int frame, string dir)
    {
        _sampleAnimation.Stop();
        List<AnimData.FrameData> frameList = new List<AnimData.FrameData>();
        float time = 0.0f;
        bool isEnd = false;
        do
        {
            if (time >= animationState.length)
            {
                time = animationState.length;
                isEnd = true;
            }
            AnimData.FrameData frameData = GetFrameData(animationState, time);
            frameList.Add(frameData);
            time += 1.0f / frame;
            if (isEnd) break;
        } while (true);
        AnimData animData = ScriptableObject.CreateInstance<AnimData>();
        animData.frameDatas = frameList.ToArray();
        animData.name = animationState.name;
        animData.animLen = animationState.length;
        animData.frame = frame;
        string path = dir + "/" + animationState.name + ".asset";
        AssetDatabase.CreateAsset(animData, path);
    }



    private AnimData.FrameData GetFrameData(AnimationState animationState, float time)
    {
        animationState.enabled = true;
        animationState.weight = 1.0f;
        animationState.time = time;
        _sampleAnimation.Sample();
        AnimData.FrameData frameData = new AnimData.FrameData();
        frameData.time = time;
        List<Matrix4x4> matrix4X4s = new List<Matrix4x4>();
        foreach(Transform bone in _bones)
        {
            matrix4X4s.Add(bone.localToWorldMatrix);
        }
        frameData.matrix4X4s = matrix4X4s.ToArray();

        return frameData;
    }

}
