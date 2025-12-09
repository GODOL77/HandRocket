 using UnityEngine;

public class PartSnap : MonoBehaviour
{
    public RocketManager RM;
    public string requiredTag;      
    public Transform snapPoint;    
    private bool isSnapped = false;
    public GameObject RocketObject;

    public GameObject RocketChild;

    MeshRenderer mesh;
    MeshRenderer childMesh;

    

    void Awake()
    {
        mesh = GetComponent<MeshRenderer>();
        if (RocketChild != null)
        childMesh = RocketChild.GetComponent<MeshRenderer>();
        
        if (mesh != null)
            mesh.enabled = false;
        if(childMesh != null)
        {
            Debug.Log("Found MeshRenderer on object: " + childMesh.gameObject.name);
            childMesh.enabled = false;
        }
        
            

    }

    private void OnTriggerEnter(Collider other)
    {
        if (isSnapped) return;

        if (other.gameObject == RocketObject)
        {
            Snap(other.gameObject);
        }
    }

    void Snap(GameObject part)
    {
        Debug.Log($"{requiredTag} 파츠 장착 완료!");
        RM.rocketParts += 1;


        // 기존 파츠 삭제 (들고온 모델)
        Destroy(part);

        // 진짜 로켓 MeshRenderer 활성화
        if (mesh != null)
            mesh.enabled = true;
        if (childMesh != null)
            childMesh.enabled = true;
        // 위치/회전 스냅
        part.transform.position = snapPoint.position;
        part.transform.rotation = snapPoint.rotation;

        // 더 이상 스냅 불가능
        isSnapped = true;
    }
}
