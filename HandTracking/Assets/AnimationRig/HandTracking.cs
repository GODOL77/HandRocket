using System;
using UnityEngine;

public class HandTracking : MonoBehaviour
{
    /// <summary>
    /// 1. hand Prefab은 180도 회전 상태여야 정상적으로 동작함
    /// 2. 플레이어가 정면을 바라보는 시점 그대로 재현
    /// 3. handRotationOffset은 손목의 뒤틀림을 보정한 값으로 X = -90을 적용 -> 손바닥 법선 벡터를 기준으로 회전할 때 발생하는 축 보정
    /// </summary>
    
    [Header("state")] 
    public string state;
    
    [Header("Control")]
    public bool stopAllLogic;
    public bool useSmoothing;
    public Vector3 handRotationOffset;
    
    [Range(1f, 30f)] 
    public float smoothSpeed = 30f;

    [Header("References")]
    public UDPReceive udpReceive;
    public GameObject handPrefab;

    [Header("Rigging")]
    public Transform[] joints;
    public Transform[] thumbsJoints;

    [Header("Data")]
    private readonly Vector3[] _lm = new Vector3[21];
    
    [Header("Init Rot")]
    private Quaternion[] _initRot;
    private Quaternion[] _initThumbRot;

    [Header("Bone Directions")]
    private Vector3[] _thumbsBaseDir = new Vector3[3];
    private Vector3[] _fingerBaseDir;

    private bool _isInitialized;
    
    // mediapipe landmark index를 각 본에 매핑
    // 손가락 segment 간 벡터 방향을 계산하기 위함
    // 각 행은 하나의 관절에 해당하며, {시작 랜드마크 인덱스, 끝 랜드마크 인덱스}로 구성됨
    [Header("Landmark Mapping")]
    private readonly int[,] _fingerMap =
    {
        {5,6}, {6,7}, {7,8},        // index
        {9,10}, {10,11}, {11,12},   // middle
        {13,14}, {14,15}, {15,16},  // ring
        {17,18}, {18,19}, {19,20}   // pinky
    };
    
    private readonly int[,] _thumbMap =
    {
        { 1, 2 },   // cmc
        { 2, 3 },   // mcp
        { 3, 4 }    // ip
    };

    private void Start()
    {
        // 일반 손가락 관절의 초기 회전값 저장
        _initRot = new Quaternion[joints.Length];
        for (int i = 0; i < joints.Length; i++)
            _initRot[i] = joints[i].localRotation;
        
        // 엄지 관절의 초기 회전값 저장
        _initThumbRot = new Quaternion[thumbsJoints.Length];
        for (int i = 0; i < thumbsJoints.Length; i++)
            _initThumbRot[i] = thumbsJoints[i].localRotation;
        
        // 모델 본초기 방향 벡터 계산 및 저장 
        CalculateModelDirections();
        
        _isInitialized = true;
    }

    void Update()
    {
        if (stopAllLogic)
            return;
        
        ParseLandmarks();
        
        if (_lm[0] == Vector3.zero)
            return;

        if (_isInitialized)
        {
            Vector3 wrist = _lm[0];
            Vector3 indexBase = _lm[5];
            Vector3 pinkyBase = _lm[17];
            
            Vector3 v1 = indexBase - wrist;
            Vector3 v2 =  pinkyBase - wrist;
            // 외적을 통해 v1과 v2에 수직인 벡터 계산
            Vector3 currentPalmNormal = Vector3.Cross(v1, v2).normalized;
            
            // 손목 회전 적용
            ApplyWristRotation(currentPalmNormal, indexBase, pinkyBase);
            
            // 일반 손가락 회전 적용
            ApplyFingerRotations(joints, _fingerMap, _fingerBaseDir, _initRot);
            
            // 엄지 손가락 회전 적용
            ApplyThumbRotation(_thumbMap, _thumbsBaseDir, _initThumbRot);
        }
    }

    #region Process Data

    private void ParseLandmarks()
    {
        string data = udpReceive?.Packet.Data;
        if (string.IsNullOrEmpty(data))
            return;

        try
        {
            // 데이터는 state|points 형태로 구분
            string[] split = data.Split('|');
            if (split.Length < 2) 
                throw new Exception("Invalid data format.");
        
            state = split[0];
            string pointsString = split[1].Trim('[', ']'); 
            string[] points = pointsString.Split(',');
        
            float p0X = float.Parse(points[0]);
            float p0Y = float.Parse(points[1]);
            float p0Z = float.Parse(points[2]);
            handPrefab.transform.localPosition = new Vector3(5 - p0X / 100f, p0Y / 100f, -p0Z / 100f);

            for (int i = 0; i < 21; i++)
            {
                float x = float.Parse(points[i * 3]);
                float y = float.Parse(points[i * 3 + 1]);
                float z = float.Parse(points[i * 3 + 2]);
            
                _lm[i] = new Vector3(5 - x / 100f, y / 100f, -z / 100f);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing landmarks:" + e);
        }
    }
    
    #endregion Process Data
    
    #region Core
    
    private void ApplyWristRotation(Vector3 palmNormal, Vector3 indexBase, Vector3 pinkyBase)
    {
        if (handPrefab == null) 
            return;
    
        // up
        Vector3 palmUp = palmNormal.normalized;
        // right
        Vector3 palmRight = (pinkyBase - indexBase).normalized;
        // forward
        Vector3 palmForward = Vector3.Cross(palmUp, palmRight).normalized;
        
        // Forward를 기준으로 Up을 바라보는 회전 계산
        Quaternion targetRotation = Quaternion.LookRotation(palmForward, palmUp);
        
        // 손목 모델의 기본 회전 뒤틀림을 보정하기 위해 오프셋 적용
        targetRotation *= Quaternion.Euler(handRotationOffset);

        handPrefab.transform.rotation = useSmoothing
            ? Quaternion.Slerp(handPrefab.transform.rotation, targetRotation, Time.deltaTime * smoothSpeed)
            : targetRotation;
    }
    
    private void ApplyThumbRotation(int[,] map, Vector3[] modelBaseDirs, Quaternion[] initRots)
    {
        for (int i = 0; i < thumbsJoints.Length - 1; i++)
        {
            Transform bone = thumbsJoints[i];
            if (bone == null || bone.parent == null)
                continue;

            // 해당 관절 Segment에 대응하는 랜드마크 벡터 계산
            Vector3 lmA = _lm[map[i, 0]];
            Vector3 lmB = _lm[map[i, 1]];
            Vector3 targetDirWorld = lmB - lmA;

            if (targetDirWorld.sqrMagnitude < 1e-6f)
                continue;
            targetDirWorld.Normalize();

            // 월드 방향 벡터를 부모 관절의 로컬 공간으로 변환 -> 로컬 회전을 계산하려면 부모를 기준으로
            Vector3 targetDirLocal = bone.parent.InverseTransformDirection(targetDirWorld).normalized;
            
            // 초기 모델 방향 대비 현재 랜드마크 방향으로의 회전 오프셋 계산
            Quaternion rotationOffset = Quaternion.FromToRotation(modelBaseDirs[i], targetDirLocal);

            bone.localRotation = useSmoothing
                ? Quaternion.Slerp(bone.localRotation, rotationOffset * initRots[i], Time.deltaTime * smoothSpeed)
                : rotationOffset * initRots[i];
        }
    }
    
    private void ApplyFingerRotations(Transform[] bones, int[,] map, Vector3[] modelBaseDirs, Quaternion[] initRots)
    {
        for (int i = 0; i < bones.Length; i++)
        {
            if (i >= map.GetLength(0)) 
                break;
            
            Transform bone = bones[i];
            if (bone == null || bone.parent == null)
                continue;

            // 해당 관절 Segment에 대응되는 랜드마크 벡터 계산
            Vector3 lmA = _lm[map[i, 0]];
            Vector3 lmB = _lm[map[i, 1]];
            Vector3 targetDirWorld = lmB - lmA; 

            if (targetDirWorld.sqrMagnitude < 1e-6f) 
                continue;
            targetDirWorld.Normalize();

            // 월드 방향 벡터를 부모 관절의 로컬 공간으로 변환
            Vector3 targetDirLocal = bone.parent.InverseTransformDirection(targetDirWorld).normalized;
            
            // 초기 방향 대비 회전 오프셋 계산
            Quaternion rotationOffset = Quaternion.FromToRotation(modelBaseDirs[i], targetDirLocal);

            bone.localRotation = useSmoothing
                ? Quaternion.Slerp(bone.localRotation, rotationOffset * initRots[i], Time.deltaTime * smoothSpeed)
                : rotationOffset * initRots[i];
        }
    }
    
    private void CalculateModelDirections()
    {
        _fingerBaseDir = new Vector3[joints.Length];
        _thumbsBaseDir = new Vector3[thumbsJoints.Length];
        
        // 일반 손가락 관절의 기본 방향 계산
        for (int i = 0; i < joints.Length; i++)
        {
            Transform current = joints[i];
            Transform child = null;
            
            // 자식 관절 세팅
            if ((i + 1) % 3 != 0 && i + 1 < joints.Length)
                child = joints[i + 1];
            // 손가락의 마지막 관절이거나, 다음 관절이 배열에 없으면 모델의 자식 Transform을 사용.
            else if
                (current.childCount > 0) child = current.GetChild(0);

            Vector3 worldDir;
            if (child != null)
                // 다음 관절의 위치 - 현재 관절의 위치 = 관절의 방향 벡터
                worldDir = child.position - current.position; 
            else
                // 마지막 관절인 경우, 모델의 기본 Forward 방향을 사용
                worldDir = current.transform.TransformDirection(Vector3.forward);
            
            // 관절의 방향 벡터를 부모의 로컬 공간으로 변환하여 저장
            if(current.parent != null)
                _fingerBaseDir[i] = current.parent.InverseTransformDirection(worldDir.normalized);
        }

        // 엄지 관절의 기본 방향 계산 -> 각 관절의 방향은 다음 관절로 향하는 벡터를 부모의 로컬 공간으로 변환하여 저장
        _thumbsBaseDir[0] = thumbsJoints[0].parent
            .InverseTransformDirection((thumbsJoints[1].position - thumbsJoints[0].position).normalized);

        _thumbsBaseDir[1] = thumbsJoints[1].parent.
            InverseTransformDirection((thumbsJoints[2].position - thumbsJoints[1].position).normalized);

        _thumbsBaseDir[2] = thumbsJoints[2].parent
            .InverseTransformDirection((thumbsJoints[3].position - thumbsJoints[2].position).normalized);
    }
    
    #endregion Core
}