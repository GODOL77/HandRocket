using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;


[ExecuteAlways]
public class HandBoneReader : MonoBehaviour
{
    [Header("제외할 본")]
    [SerializeField]
    private List<string> excludedBones = new()
    {
        "Hand",
        "root"
    };
        
    [ReadOnly]
    public List<Transform> bonesToViewOnly = new();

    private void Awake()
    {
        excludedBones.Add(gameObject.name);
        bonesToViewOnly = new List<Transform>(GetComponentsInChildren<Transform>());
        bonesToViewOnly.RemoveAll(bone => excludedBones.Contains(bone.name));
            
        bonesToViewOnly = bonesToViewOnly.ToList();
    }
}

// 구상안
// 1. Reader에서 각 본의 위치를 보냄
// 2. Manager의 HandTracking 스크립트에서 List를 읽어 각 본에 대칭되게 세팅함
// 3. 