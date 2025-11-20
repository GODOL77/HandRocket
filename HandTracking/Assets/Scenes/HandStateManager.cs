using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class HandStateManager : MonoBehaviour
{

    [SerializeField] GameObject midOfHand;
    [SerializeField] GameObject bottomOfHand;

    [SerializeField] HandTracking handTracking;

    CapsuleCollider handColl;

    Transform heldObject = null;
    string curGesture = "UNKNOWN";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        handColl = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
        HandMoving();
        
        HandInteraction();



        

    }

    void HandMoving()
    {
        this.transform.position = midOfHand.transform.position;
        handColl.radius = Vector3.Distance(midOfHand.transform.position, bottomOfHand.transform.position) * 0.8f;
    }

    void HandInteraction()
    {

        if (handTracking.HandState == null)
            return;

            else
        curGesture = handTracking.HandState;


        if (handTracking.HandState != "FIST" && heldObject != null)
        {
            ReleaseObject();
        }


/*
        switch (curGesture){
            
            case "FIST":


            break;



        default:
            break;
        }

*/






    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Movable")&& curGesture=="FIST"&& heldObject ==null)
        {
            GrabObject(other.gameObject);
        }
    }   

    void GrabObject(GameObject obj)
    {
        heldObject = obj.transform; // 잡은 물체 기억

        // [핵심 수정] 부모를 손(this)으로 설정
        heldObject.SetParent(this.transform);

        // 물리 끄기 (손을 따라다닐 때 덜덜거림 방지)
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    void ReleaseObject()
    {
        if (heldObject == null) return;

        // [핵심 수정] 부모를 없음(null)으로 설정 -> 월드로 돌아감
        heldObject.SetParent(null);

        // 물리 다시 켜기
        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        heldObject = null; // 잡은 물체 기억 삭제
    }

}
