using UnityEngine;

public class HandTracking : MonoBehaviour
{
    public UDPReceive udpReceive;
    public GameObject[] handPoints;

    public bool calcType = false;

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
        

        //손 게임 뷰에 hand 위치 출력하는 부분
        for (var i = 0; i < handPoints.Length; i++)
        {
            float x = 5 - float.Parse(points[i * 3]) / 100;
            float y = float.Parse(points[i * 3 + 1]) / 100;
            float z = float.Parse(points[i * 3 + 2]) / 100;
            
            handPoints[i].transform.localPosition = new Vector3(x, y, z);
        }
    }
}
