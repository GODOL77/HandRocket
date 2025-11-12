using UnityEngine;

public class TestHandTracking : MonoBehaviour
{
    public UDPReceive udpReceive;
    public GameObject[] handPoints;

    public bool calcType = true;
    
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
   
        string[] split  = data.Split('|');
        if (split.Length < 2) 
            return;
        
        string handState = split[0];
        string pointsString = split[1];
        
        if (pointsString.StartsWith("[") && pointsString.EndsWith("]"))
        {
            pointsString = pointsString.Trim('[', ']');
        }
        
        string[] points = pointsString.Split(',');

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
}
