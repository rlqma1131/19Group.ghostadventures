using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch4_SpiderWebLayout : MonoBehaviour
{
    [SerializeField] float linkRadius = 1.2f; // 이 거리 안에 있으면 이웃으로 간주
    [SerializeField] bool drawGizmos = true;

    public List<Ch4_SpiderWebNode> Nodes { get; private set; } = new();
    // 자동 생성된 이웃표 (양방향)
    Dictionary<Ch4_SpiderWebNode, List<Ch4_SpiderWebNode>> neighbors = new();

    void Awake() {
        Nodes.Clear();
        neighbors.Clear();
        GetComponentsInChildren(true, Nodes);

        for (int i = 0; i < Nodes.Count; i++) {
            var a = Nodes[i];
            if (!neighbors.ContainsKey(a)) neighbors[a] = new List<Ch4_SpiderWebNode>();
            for (int j = i + 1; j < Nodes.Count; j++) {
                var b = Nodes[j];
                float d = Vector2.Distance(a.transform.position, b.transform.position);
                if (d <= linkRadius) {
                    neighbors[a].Add(b);
                    if (!neighbors.ContainsKey(b)) neighbors[b] = new List<Ch4_SpiderWebNode>();
                    neighbors[b].Add(a);
                }
            }
        }
    }
    
    public Ch4_SpiderWebNode FindNearestNode(Vector3 worldPos)
    {
        Ch4_SpiderWebNode best = null;
        float bestSqr = float.MaxValue;
        foreach (var n in Nodes)
        {
            if (!n) continue;
            float d = (n.transform.position - worldPos).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; best = n; }
        }
        return best;
    }

    public List<Ch4_SpiderWebNode> GetNeighbors(Ch4_SpiderWebNode n) =>
        neighbors.TryGetValue(n, out var list) ? list : _empty;
    static readonly List<Ch4_SpiderWebNode> _empty = new();

#if UNITY_EDITOR
    void OnDrawGizmos() {
        if (!drawGizmos) return;
        var tmp = new List<Ch4_SpiderWebNode>();
        GetComponentsInChildren(true, tmp);
        Gizmos.color = new Color(0,1,1,0.35f);
        foreach (var n in tmp)
            Gizmos.DrawWireSphere(n.transform.position, linkRadius);
    }
#endif
}