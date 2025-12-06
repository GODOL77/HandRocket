using UnityEngine;

public class HandTracking : MonoBehaviour
{
    [Header("state")] 
    public string state;
    
    public UDPReceive udpReceive;
    public GameObject[] handPoints;
    // public GameObject handPrefab;
    
    [Header("Control")]
    public bool stopAllLogic = true;
    
    void Update()
    {
        if (stopAllLogic)
            return;
        
        CalcLandMarkPositionAndGetState();
    }

    private void CalcLandMarkPositionAndGetState()
    {
        string data = udpReceive?.Packet.Data;
        if (string.IsNullOrEmpty(data))
            return;

        string[] split = data.Split('|');
        if (split.Length < 2)
            return;
        
        state =  split[0];
        string pointsString = split[1];

        if (pointsString.StartsWith("[") && pointsString.EndsWith("]"))
        {
            pointsString = pointsString.Trim('[', ']');
        }

        string[] points = pointsString.Split(',');

        if (points.Length < handPoints.Length * 3)
            return;

        // handPrefab.transform.position = new Vector3(5 - float.Parse(points[0]) / 100, float.Parse(points[1 * 3 + 1]) / 100, float.Parse(points[2 * 3 + 2]) / 100);

        for (var i = 0; i < handPoints.Length; i++)
        {
            float x = 5 - float.Parse(points[i * 3]) / 100;
            float y = float.Parse(points[i * 3 + 1]) / 100;
            float z = - float.Parse(points[i * 3 + 2]) / 100;

            handPoints[i].transform.localPosition = new Vector3(x, y, z);
        }
    }
}
