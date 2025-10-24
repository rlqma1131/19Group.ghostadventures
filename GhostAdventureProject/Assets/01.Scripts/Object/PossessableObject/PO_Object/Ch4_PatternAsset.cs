using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PatternAsset", menuName = "GhostAdventure/Pattern Asset")]
public class Ch4_PatternAsset : ScriptableObject
{
    [Header("ID & Display")]
    public string patternId;                     // "PATTERN_1", "PATTERN_2" ...
    public Sprite noteSprite;                    // 쪽지에 표시될 이미지
    public Sprite wallSymbolSprite;              // 벽 문양 스프라이트(선택)

    [Header("One-Stroke Path (Node Id Sequence)")]
    public List<string> nodeIdSequence = new();  // 예: ["A","B","C","D","E"]

    [Header("Validation Rules")]
    public bool forbidEdgeReuse = true;          // 같은 간선(연결)을 두 번 쓰면 실패
    public bool mustMatchExactOrder = true;      // 정확히 지정된 순서만 허용
    
    [Header("Free-draw Options")]
    public bool validateAtCommit = true;  // 이동 중엔 순서 강제 X, '제출' 때 한 번에 판정
    public bool allowAnyStart   = true;   // 시작 노드는 어디든 OK (회전 등가 허용)
    public bool allowReverse    = true;   // 뒤집힌 경로도 정답으로 허용(원하면 끄기)
}
