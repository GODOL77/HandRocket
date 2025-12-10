using System.Collections.Generic;
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

    private void OnEnable() => UpdateList();
    private void OnValidate() => UpdateList();

    private void UpdateList()
    {
        if (!excludedBones.Contains(gameObject.name))
        {
            excludedBones.Add(gameObject.name);
        }
        
        bonesToViewOnly = new List<Transform>(GetComponentsInChildren<Transform>());
        bonesToViewOnly.RemoveAll(bone => excludedBones.Contains(bone.name));
    }
}