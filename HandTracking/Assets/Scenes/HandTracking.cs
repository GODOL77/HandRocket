using UnityEngine;

public class HandTracking : MonoBehaviour
{
    public UDPReceive udpReceive;
    public GameObject[] handPoints;

    public bool calcType = false;

    // [추가된 변수] 손 크기 고정 기능 제어
    [Header("Hand Scale Settings")]
    public bool useFixedScale = true; // 크기 고정 기능을 쓸지 여부
    [Tooltip("손목(0)과 중지뿌리(9) 사이의 고정 거리 (이 값을 조절하여 손 크기를 결정하세요)")]
    public float targetHandSize = 1.5f; // 기본값, 유니티 에디터에서 손 크기에 맞춰 조절 필요

    private string _currentHandState = "UNKNOWN";

    public string HandState 
    { 
        get { return _currentHandState; } 
    }

    
    void Update()
    {
        switch (calcType)
        {
            case true:
                CalcLandMarkPosition();
                break;
            case false:
                CalcLandMarkPositionAndGetState();
                break;
        }
    }

    private void CalcLandMarkPosition()
    {
        string data = udpReceive.Packet.Data;

        if (string.IsNullOrEmpty(data) || data.Length < 2)
            return;
        
        if (data.StartsWith("[") && data.EndsWith("]"))
        {
            data = data.Remove(0, 1);
            data = data.Remove(data.Length - 1, 1);
        }

        string[] points = data.Split(',');
        
        if (points.Length < handPoints.Length * 3)
            return;
        
        for (var i = 0; i < handPoints.Length; i++)
        {
            float x = 5 - float.Parse(points[i * 3]) / 100;
            float y = float.Parse(points[i * 3 + 1]) / 100;
            float z = float.Parse(points[i * 3 + 2]) / 100;
            
            handPoints[i].transform.localPosition = new Vector3(x, y, z);
        }
    }

    private void CalcLandMarkPositionAndGetState()
    {
        string data = udpReceive?.Packet.Data;
        if (string.IsNullOrEmpty(data))
            return;


        //손 랜드마크와 상태 분리하는 부분
        string[] split  = data.Split('|');
        if (split.Length < 2) 
            return;
        


        _currentHandState = split[0];
        string pointsString = split[1];
        //손 상태 데이터 저장하는 부분 = split[0]

        if (_currentHandState == "FIST")
        {
            //Debug.Log("Fist State Detected");
        }
        
        if (pointsString.StartsWith("[") && pointsString.EndsWith("]"))
        {
            pointsString = pointsString.Trim('[', ']');
        }
        
        string[] points = pointsString.Split(',');

        if (points.Length < handPoints.Length * 3)
            return;
        

        // [수정] 좌표 계산 및 적용 함수로 분리
        ApplyPointsToHand(points);
    }

    // [핵심 로직] 파싱된 포인트 데이터를 실제 게임 오브젝트에 적용하는 함수
    private void ApplyPointsToHand(string[] points)
    {
        // 1. 일단 모든 점의 로컬 좌표를 계산해서 임시 리스트에 담습니다.
        Vector3[] tempPositions = new Vector3[handPoints.Length];

        for (var i = 0; i < handPoints.Length; i++)
        {
            float x = 5 - float.Parse(points[i * 3]) / 100;
            float y = float.Parse(points[i * 3 + 1]) / 100;
            float z = float.Parse(points[i * 3 + 2]) / 100;
            
            tempPositions[i] = new Vector3(x, y, z);
        }

        // 2. 크기 고정 로직 적용
        if (useFixedScale && handPoints.Length > 9)
        {
            // 기준점: 0번(손목), 9번(중지 첫마디/뿌리)
            Vector3 wristPos = tempPositions[0];
            Vector3 middleFingerBasePos = tempPositions[9];

            // 현재 데이터 상의 손 크기 계산 (손목과 중지 뿌리 사이 거리)
            float currentDistance = Vector3.Distance(wristPos, middleFingerBasePos);

            // 거리가 너무 0에 가까우면 계산 오류 방지
            if (currentDistance > 0.001f)
            {
                // 확대/축소 비율 계산 (목표크기 / 현재크기)
                float scaleFactor = targetHandSize / currentDistance;

                // 모든 점들을 손목(0번) 기준으로 재배치
                for (int i = 0; i < handPoints.Length; i++)
                {
                    // 손목으로부터의 상대 벡터 계산
                    Vector3 directionFromWrist = tempPositions[i] - wristPos;
                    
                    // 상대 벡터에 비율을 곱해 거리 보정
                    Vector3 scaledDirection = directionFromWrist * scaleFactor;

                    // 보정된 위치 적용 (손목 위치는 그대로 두고 나머지만 확장/축소)
                    tempPositions[i] = wristPos + scaledDirection;
                }
            }
        }

        // 3. 최종 좌표를 실제 게임 오브젝트에 대입
        for (int i = 0; i < handPoints.Length; i++)
        {
            handPoints[i].transform.localPosition = tempPositions[i];
        }
    }
}
