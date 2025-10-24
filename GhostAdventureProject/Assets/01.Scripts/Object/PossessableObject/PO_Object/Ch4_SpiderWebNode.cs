using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch4_SpiderWebNode : MonoBehaviour
{
    [SerializeField] string nodeId;   // 예: "A", "B", "C"...
    public string Id => nodeId;

#if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.05f);
    }
#endif
}