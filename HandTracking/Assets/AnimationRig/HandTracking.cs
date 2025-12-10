using UnityEngine;

public class HandTracking : MonoBehaviour
{
    [Header("state")] 
    public string state;
    
    [Header("Control")]
    public bool stopAllLogic;

    public UDPReceive udpReceive;
    public GameObject[] handPoints;
    public GameObject handPrefab;

    public Transform[] joints;
    public Transform[] thumbsJoints;

    private readonly Vector3[] _lm = new Vector3[21];
    private Quaternion[] _initRot;
    private Quaternion[] _initThumbRot;

    private readonly Vector3[] _thumbsBaseDir = new Vector3[3];
    private readonly Vector3[] _fingerBaseDir = new Vector3[12];

    private void Start()
    {
        _initRot = new Quaternion[joints.Length];
        for (int i = 0; i < joints.Length; i++)
            _initRot[i] = joints[i].localRotation;
        
        _initThumbRot = new Quaternion[thumbsJoints.Length];
        for (int i = 0; i < thumbsJoints.Length; i++)
            _initThumbRot[i] = thumbsJoints[i].localRotation;

        _thumbsBaseDir[0] = thumbsJoints[0].parent
            .InverseTransformDirection((thumbsJoints[1].position - thumbsJoints[0].position).normalized);
        
        _thumbsBaseDir[1] = thumbsJoints[1].parent
            .InverseTransformDirection((thumbsJoints[2].position - thumbsJoints[1].position).normalized);
        
        _thumbsBaseDir[2] = thumbsJoints[2].parent
            .InverseTransformDirection((thumbsJoints[3].position - thumbsJoints[2].position).normalized);
        
        InitFingerBaseDir();
    }

    void Update()
    {
        if (stopAllLogic)
            return;
        
        ParseLandmarks();
        ApplyFingerRotations();
        ApplyThumbRotations();
    }
    
    #region Process Data

    private void ParseLandmarks()
    {
        string data = udpReceive?.Packet.Data;
        if (string.IsNullOrEmpty(data))
            return;
        
        string[] split = data.Split('|');
        if (split.Length < 2) 
            return;
        
        state = split[0];
        string pointsString = split[1];
        if (pointsString.StartsWith("[") && pointsString.EndsWith("]"))
        {
            pointsString = pointsString.Trim('[', ']');
        }

        string[] points = pointsString.Split(',');
        if (points.Length < handPoints.Length * 3)
            return;
        
        handPrefab.transform.localPosition = new Vector3(5 - float.Parse(points[0]) / 100, float.Parse(points[1 * 3 + 1]) / 100, float.Parse(points[2 * 3 + 2]) / 100);
        
        for (var i = 0; i < handPoints.Length; i++)
        {
            float x = float.Parse(points[i * 3]);
            float y = float.Parse(points[i * 3 + 1]);
            float z = float.Parse(points[i * 3 + 2]);
            
            _lm[i] = ConvertPoint(x, y, z);
        }
    }

    private Vector3 ConvertPoint(float x, float y, float z)
    {
        return new Vector3(5 - x / 100f, y / 100f, -z / 100f);
    }
    
    #endregion Process Data
    
    #region Process Thumb
    
    private void ApplyThumbRotations()
    {
        ApplyThumbRotation(thumbsJoints[0], _lm[1], _lm[2], _initThumbRot[0], _thumbsBaseDir[0]);
        ApplyThumbRotation(thumbsJoints[1], _lm[2], _lm[3], _initThumbRot[1], _thumbsBaseDir[1]);
        ApplyThumbRotation(thumbsJoints[2], _lm[3], _lm[4], _initThumbRot[2], _thumbsBaseDir[2]);
    }
    
    private void ApplyThumbRotation(Transform bone, Vector3 a, Vector3 b, Quaternion initRot, Vector3 baseDir)
    {
        Vector3 worldDir = (b - a).normalized;
        Vector3 localDir = bone.parent.InverseTransformDirection(worldDir);
        
        if (localDir == Vector3.zero)
            return;
        
        Quaternion delta = Quaternion.FromToRotation(baseDir, localDir);
        bone.localRotation = delta * initRot;
    }

    #endregion Process Thumb
    
    #region Process Finger
    
    private void InitFingerBaseDir()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            if (i >= joints.Length - 1)
            {
                // 마지막 index만 landmark의 값을 대입 -> 다음 관절이 없음
                _fingerBaseDir[11] = joints[11].parent
                    .InverseTransformDirection((handPoints[20].transform.position - joints[11].position).normalized);
                break;
            }
            
            _fingerBaseDir[i] = joints[i].parent
                .InverseTransformDirection((joints[i + 1].position - joints[i].position).normalized);
        }
    }
    
    private void ApplyFingerRotations()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            ApplyFingerRotation(joints[i], _lm[i + 5], _lm[i + 6], _initRot[0], _fingerBaseDir[i]);
        }
        
        // index finger
        ApplyFingerRotation(joints[0], _lm[5], _lm[6], _initRot[0],  _fingerBaseDir[0]);
        ApplyFingerRotation(joints[1], _lm[6], _lm[7], _initRot[0],  _fingerBaseDir[1]);
        ApplyFingerRotation(joints[2], _lm[7], _lm[8], _initRot[0],  _fingerBaseDir[2]);
    }

    private void ApplyFingerRotation(Transform bone, Vector3 lmA, Vector3 lmB, Quaternion initRot, Vector3 baseDir)
    {
        Vector3 worldDir = (lmA - lmB).normalized;
        Vector3 localDir = bone.parent.InverseTransformDirection(worldDir);

        if (localDir == Vector3.zero)
            return;

        Quaternion delta = Quaternion.FromToRotation(baseDir, localDir);
        bone.localRotation = delta * initRot;
    }
    
    #endregion Process Finger
}