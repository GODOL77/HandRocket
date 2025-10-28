using UnityEngine;

public class LineCode : MonoBehaviour
{
    private LineRenderer _lineRenderer;

    public Transform origin;
    public Transform destination;

    public void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        {
            _lineRenderer.startWidth = 0.2f;
            _lineRenderer.endWidth = 0.1f;
            _lineRenderer.positionCount = 2;
            _lineRenderer.useWorldSpace = true;
        }
    }

    private void Update()
    {
        if (!origin || !destination)
            return;
        
        _lineRenderer.SetPosition(0, origin.position);
        _lineRenderer.SetPosition(1, destination.position);
    }
}
