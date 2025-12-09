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

    public GameObject hand;

    // [추가된 변수] 손목 위치를 참조하기 위해 랜드마크 배열에 접근합니다.
    // MediaPipe 기준 0번 인덱스가 손목입니다.
    private const int WRIST_INDEX = 0;

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

        HandZMoving();



        

    }

    void HandMoving()
    {
        
        handColl.radius = Vector3.Distance(midOfHand.transform.position, bottomOfHand.transform.position) * 0.8f;
        this.transform.position = new Vector3(midOfHand.transform.position.x, midOfHand.transform.position.y);
    }

    void HandZMoving()
    {
        // hand 오브젝트의 Z축을 0으로 설정합니다.
        // X, Y 위치는 기존의 hand.transform.localPosition 값을 그대로 유지합니다.
        hand.transform.localPosition = new Vector3(
            hand.transform.localPosition.x, 
            hand.transform.localPosition.y, 
            // 🌟 Z 값을 0으로 고정!
            -3f 
        );
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
