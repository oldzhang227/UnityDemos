using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimTest : MonoBehaviour
{
    public string animName = "walk";
    public string animDir = "Assets/Resources/Anims";
    private AnimData _animData;
    private float _time;
    private int _frame;
    private Mesh _mesh;
    private Matrix4x4[] _bindPoses;
    private List<Vector3> _srcPoints;
    private List<Vector3> _newPoints;
    // Start is called before the first frame update
    void Awake()
    {
        _mesh = GetComponentInChildren<MeshFilter>().mesh;
        _srcPoints = new List<Vector3>();
        _mesh.GetVertices(_srcPoints);
        _bindPoses = _mesh.bindposes;
        _newPoints = new List<Vector3>(_srcPoints);
        Play(animName);
    }


    void Play(string name)
    {
        if(_animData == null || _animData.name != name)
        {
            string path = animDir + "/" + name;
            string resName = path.Substring("Assets/Resources/".Length);
            _animData = Resources.Load<AnimData>(resName);
            _time = 0.0f;
            _frame = -1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_animData == null) return;
        if (_frame < 0)
        {
            ApplyFrame(0);
            return;
        }
        _time += Time.deltaTime;
        _time %= _animData.animLen;
        int f = (int)(_time / (1.0f / _animData.frame));

        if (f != _frame)
        {
            ApplyFrame(f);
        }
    }

    void ApplyFrame(int f)
    {
        _frame = f;

        AnimData.FrameData frameData = _animData.frameDatas[_frame];
        for (int i = 0; i < _srcPoints.Count; ++i)
        {
            Vector3 point = _srcPoints[i];
            BoneWeight weight = _mesh.boneWeights[i];
            Matrix4x4 tempMat0 = frameData.matrix4X4s[weight.boneIndex0] * _bindPoses[weight.boneIndex0];
            Matrix4x4 tempMat1 = frameData.matrix4X4s[weight.boneIndex1] * _bindPoses[weight.boneIndex1];
            Matrix4x4 tempMat2 = frameData.matrix4X4s[weight.boneIndex2] * _bindPoses[weight.boneIndex2];
            Matrix4x4 tempMat3 = frameData.matrix4X4s[weight.boneIndex3] * _bindPoses[weight.boneIndex3];

            Vector3 temp = tempMat0.MultiplyPoint(point) * weight.weight0 +
                                   tempMat1.MultiplyPoint(point) * weight.weight1 +
                                   tempMat2.MultiplyPoint(point) * weight.weight2 +
                                   tempMat3.MultiplyPoint(point) * weight.weight3;

            _newPoints[i] = temp;
        }

        _mesh.SetVertices(_newPoints);
    }
}
