using UnityEngine;

public class CamMoce : MonoBehaviour
{
    public GameObject Rocket;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.LookAt(Rocket.transform);
    }
}
