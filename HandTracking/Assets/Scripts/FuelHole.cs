using UnityEngine;

public class FuelHole : MonoBehaviour
{
    public RocketManager RM;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("GasHose"))
        {
            RM.hoseAttached = true;
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("GasHose"))
        {
            RM.hoseAttached = false;
        }
    }
}



