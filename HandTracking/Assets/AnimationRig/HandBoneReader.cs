using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace AnimationRig
{
    public class HandBoneReader : MonoBehaviour
    {
        private List<Transform> _allBones;

        [SerializeField]
        private List<string> excludedBones = new()
        {
            "Hand",
            "root"
        };
        
        [ReadOnly]
        public List<Transform> bonesToViewOnly = new();
        
        void Start()
        {
            excludedBones.Add(gameObject.name);
            _allBones = new List<Transform>(GetComponentsInChildren<Transform>());
            _allBones.RemoveAll(bone => excludedBones.Contains(bone.name));
            
            bonesToViewOnly = _allBones.ToList();

            foreach (Transform bone in _allBones)
            {
                Debug.Log($"Bone Name: {bone.name}, Local Position: {bone.localPosition}, Local Rotation: {bone.localRotation}");
            }
        }

        private void OnDrawGizmos()
        {
            if (_allBones != null &&  _allBones.Count > 0)
            {
                foreach (Transform bone in _allBones)
                {
                    Gizmos.color = bone.name == "WRIST" ? Color.red : Color.green;
                    Gizmos.DrawSphere(bone.position, 0.2f);
                }
            }
        }
    }
}

// TODO : 각 정점에 HandTracking 처럼 transform 적용시켜보기

