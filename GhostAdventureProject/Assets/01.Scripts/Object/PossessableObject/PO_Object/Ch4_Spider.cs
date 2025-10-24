using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch4_Spider : BasePossessable
{
    [Header("Layout / Visual")]
    [SerializeField] Ch4_SpiderWebLayout layout;  // 부모에 배치
    [SerializeField] LineRenderer line;
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] float snapRadius = 0.08f;

    [Header("Input")]
    [SerializeField] KeyCode drawToggleKey = KeyCode.Q;
    bool drawMode;

    [Header("State")]
    [SerializeField] Ch4_PatternAsset currentPattern;
    [SerializeField] Ch4_SpiderWebNode currentNode;
    [SerializeField] float neighborFacingDot = 0.5f; // 방향 필터(60도 정도)
    [SerializeField] string startNodeIdOverride;

    List<string> visitedNodeIds = new();
    HashSet<(string,string)> usedEdges = new();
    Vector3 targetPos;

    Ch4_SpiderPuzzleController controller;
    
    [SerializeField] KeyCode commitKey = KeyCode.Space; // 제출(판정)
    [SerializeField] KeyCode resetKey  = KeyCode.R;     // 그리기 리셋
    bool successPending;
    float _inputBlockUntil = 0f;
    bool autoTransit = false;
    [SerializeField] float autoTransitSpeed = 6f;

    protected override void Start() {
        base.Start();
        controller = FindObjectOfType<Ch4_SpiderPuzzleController>(true);
        if (!line) line = GetComponent<LineRenderer>();
        if (line) { line.positionCount = 0; line.enabled = false; }
        
        targetPos = transform.position;
    }

    public void SetPattern(Ch4_PatternAsset pattern) {
        currentPattern = pattern;
        visitedNodeIds.Clear();
        usedEdges.Clear();
        successPending = false;

        if (layout == null || currentPattern == null) return;
        if (line) { line.enabled = true; line.positionCount = 0; }

        // 자유 시작 모드: 가장 가까운 노드에서 시작
        var start = layout.FindNearestNode(transform.position);
        if (!start) {
            Debug.LogWarning("[Spider] nearest node not found."); 
            return;
        }
        currentNode = start;
        targetPos = currentNode.transform.position;

        if (line) { line.positionCount = 1; line.SetPosition(0, targetPos); }

        // 시작 노드는 '방문' 처리하되, 정답 경로 강제는 하지 않음
        visitedNodeIds.Add(currentNode.Id);
    }

    Ch4_SpiderWebNode FindNodeById(string id) {
        foreach (var n in layout.Nodes)
            if (n && n.Id == id) return n;
        return null;
    }

    protected override void Update() 
    {
        base.Update();
        if (!isPossessed && !autoTransit) return;

        // --- 자동 이동 블록(빙의 해제 상태에서도 실행) ---
        if (autoTransit) {
            transform.position = Vector3.MoveTowards(
                transform.position, targetPos, autoTransitSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) <= snapRadius) {
                autoTransit = false;

                // 다음 패턴 시작점에 도착 → 다시 빙의 허용
                SetActivated(true);           // 빙의/상호작용 가능 ON (BasePossessable에 이미 있음)
                ShowHighlight(false);         // 하이라이트는 상황에 따라 꺼두고
                // (선택) 플레이어가 옆에 있고, 즉시 후보로 다시 올리고 싶으면:
                // GameManager.Instance?.Player?.InteractSystem.AddInteractable(gameObject);

                // 여기서 끝. 다음에 플레이어가 빙의하면 OnPossessionEnterComplete에서
                // controller.ApplyCurrentPatternToSpider(this)로 패턴이 다시 확실히 덮어씌워짐.
            }
            return; // 자동 이동 중엔 이하 입력/드로잉 로직 스킵
        }

        // Q 토글: 빙의 중 + 디바운스 통과한 프레임에만
        if (Time.time >= _inputBlockUntil && Input.GetKeyDown(drawToggleKey)) {
            drawMode = !drawMode;
            UIManager.Instance?.PromptUI2.ShowPrompt_UnPlayMode(
                drawMode ? "거미줄 그리기 ON" : "거미줄 그리기 OFF", 1.2f);
        }

        // 제출/리셋 키 (드로잉 모드에서만)
        if (drawMode && Input.GetKeyDown(commitKey)) { TryCommit(); return; }
        if (drawMode && Input.GetKeyDown(resetKey))  { ResetDrawing(); return; }

        // 스무스 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        bool snapped = Vector3.Distance(transform.position, targetPos) <= snapRadius;

        // 마지막 도착 프레임에 성공 처리
        if (successPending && snapped) {
            successPending = false;
            Success();
            return;
        }

        if (!drawMode || currentPattern == null) {
            // 자유 이동 모드(기존 WASD 이동이 이 클래스에 없다면, 아무 것도 안 해도 됨)
            return;
        }

        if (!snapped) return;

        // 방향 입력 1타에 인접 노드 선택
        Vector2 dir = Vector2.zero;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector2.right;
        if (dir == Vector2.zero) return;

        var next = ChooseNeighborByDirection(currentNode, dir);
        if (next == null) { SoundManager.Instance?.PlaySFX(controller?.sfxBump); return; }

        // 간선 재사용 금지(즉시 실패)
        var e = EdgeKey(currentNode.Id, next.Id);
        if (currentPattern.forbidEdgeReuse && usedEdges.Contains(e)) { Fail("같은 간선을 두 번 사용할 수 없습니다."); return; }

        // 순서 강제 제거: 자유 이동
        usedEdges.Add(e);
        currentNode = next;
        targetPos = currentNode.transform.position;
        visitedNodeIds.Add(currentNode.Id);

        if (line) {
            line.positionCount++;
            line.SetPosition(line.positionCount - 1, targetPos);
        }
    }
    
    void TryCommit()
    {
        // 옵션이 없으면 기본값처럼 동작
        bool anyStart    = currentPattern ? currentPattern.allowAnyStart   : true;
        bool allowReverse= currentPattern ? currentPattern.allowReverse    : true;

        // 경로 길이 간단 체크(원하면 별도 규칙 가능)
        if (currentPattern && visitedNodeIds.Count != currentPattern.nodeIdSequence.Count) {
            Fail("패턴 길이가 다릅니다.");
            return;
        }

        if (currentPattern == null || MatchesPattern(
                visitedNodeIds, currentPattern.nodeIdSequence, anyStart, allowReverse))
        {
            // 마지막 노드로 이동 완료 후 성공 처리
            successPending = true;
            return;
        }

        Fail("문양이 일치하지 않습니다.");
    }

    bool MatchesPattern(List<string> path, List<string> goal, bool anyStart, bool allowReverse)
    {
        if (path.Count != goal.Count) return false;
        if (EqualSeq(path, goal)) return true;

        if (anyStart) {
            for (int s = 1; s < goal.Count; s++)
                if (EqualSeqRot(path, goal, s)) return true;
        }

        if (!allowReverse) return false;

        var rev = new List<string>(goal);
        rev.Reverse();
        if (EqualSeq(path, rev)) return true;

        if (anyStart) {
            for (int s = 1; s < rev.Count; s++)
                if (EqualSeqRot(path, rev, s)) return true;
        }

        return false;
    }

    bool EqualSeq(IReadOnlyList<string> a, IReadOnlyList<string> b)
    {
        for (int i = 0; i < a.Count; i++) if (a[i] != b[i]) return false;
        return true;
    }

    bool EqualSeqRot(IReadOnlyList<string> a, IReadOnlyList<string> b, int shift)
    {
        int n = a.Count;
        for (int i = 0; i < n; i++)
            if (a[i] != b[(i + shift) % n]) return false;
        return true;
    }

    void ResetDrawing()
    {
        visitedNodeIds.Clear();
        usedEdges.Clear();
        successPending = false;

        // 현재 위치에서 가장 가까운 노드로 리셋
        var start = layout.FindNearestNode(transform.position);
        if (start) {
            currentNode = start;
            targetPos = start.transform.position;
            visitedNodeIds.Add(currentNode.Id);
            if (line) { line.positionCount = 1; line.SetPosition(0, targetPos); }
        } else {
            if (line) { line.positionCount = 0; }
        }
    }

    Ch4_SpiderWebNode ChooseNeighborByDirection(Ch4_SpiderWebNode from, Vector2 dir) {
        if (!from) return null;
        var neighbors = layout.GetNeighbors(from);
        Ch4_SpiderWebNode best = null;
        float bestDot = neighborFacingDot;
        dir = dir.normalized;

        foreach (var n in neighbors) {
            if (!n) continue;
            Vector2 ndir = ((Vector2)n.transform.position - (Vector2)from.transform.position).normalized;
            float dot = Vector2.Dot(dir, ndir);
            if (dot > bestDot) { bestDot = dot; best = n; }
        }
        return best;
    }

    (string,string) EdgeKey(string a, string b) => string.CompareOrdinal(a,b) <= 0 ? (a,b) : (b,a);

    void Success() {
        SoundManager.Instance?.PlaySFX(controller?.sfxPatternSuccess);
        UIManager.Instance?.PromptUI2.ShowPrompt_UnPlayMode("패턴 성공!", 1.5f);
        drawMode = false;
        controller?.OnSpiderPatternSolved(currentPattern);
        controller?.ApplyCurrentPatternToSpider(this);
        
        if (currentPattern != null && currentPattern.nodeIdSequence.Count > 0 && layout != null) {
            var firstId = currentPattern.nodeIdSequence[0];
            var startNode = FindNodeById_Local(firstId); 
            if (startNode != null) {
                targetPos = startNode.transform.position; // 목적지 설정
                autoTransit = true;                       // 자동 이동 ON

                // 자동 이동 동안 빙의/상호작용 금지
                SetActivated(false);         // 후보에서 제외 (BasePossessable)
                ShowHighlight(false);
                // (선택) 캐시에 남은 표시 제거
                // GameManager.Instance?.Player?.InteractSystem.RemoveInteractable(gameObject);

                // 자동 이동 시작 전에 라인/드로잉 상태는 싹 정리
                if (line) { line.enabled = false; line.positionCount = 0; }
                visitedNodeIds.Clear();
                usedEdges.Clear();
            }
        }
        
        Unpossess();
    }

    void Fail(string reason) {
        SoundManager.Instance?.PlaySFX(controller?.sfxPatternFail);
        UIManager.Instance?.PromptUI.ShowPrompt(reason);
        drawMode = false;
        controller?.OnSpiderPatternFailed();
    }

    public override void OnPossessionEnterComplete() {
        base.OnPossessionEnterComplete();
        _inputBlockUntil = Time.time + 0.15f;
        drawMode = false;
        controller?.ApplyCurrentPatternToSpider(this);
    }

    public override void Unpossess() {
        base.Unpossess();
        drawMode = false;
        successPending = false;
        if (line) { line.positionCount = 0; line.enabled = false; }
        visitedNodeIds.Clear();
        usedEdges.Clear();
    }
    
    Ch4_SpiderWebNode FindNodeById_Local(string id)
    {
        if (layout == null || string.IsNullOrEmpty(id)) return null;

        // 레이아웃의 자식에서 전부 찾아서 Id 매칭
        var all = layout.GetComponentsInChildren<Ch4_SpiderWebNode>(true);
        for (int i = 0; i < all.Length; i++)
        {
            var n = all[i];
            if (n != null && n.Id == id) return n;
        }
        return null;
    }
}
