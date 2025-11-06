using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch4_Spider : BasePossessable
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float snapRadius = 0.05f;

    [Header("State / Control")]
    [SerializeField] float autoTransitSpeed = 6f;
    bool autoTransit = false;

    [Header("Pattern / Puzzle")]
    [SerializeField] Ch4_PatternAsset currentPattern;
    [SerializeField] Ch4_SpiderPuzzleController controller;

    [Header("Web Rendering")]
    [SerializeField] LineRenderer line;
    [SerializeField] KeyCode drawToggleKey = KeyCode.Q;

    [SerializeField] Ch4_SpiderWebLayout layout;

    // 현재 붙어 있는 거미줄 노드
    [SerializeField] Ch4_SpiderWebNode currentNode;
    
    [SerializeField] private GameObject q_Key;

    // 이동 목표 좌표
    Vector3 targetPos;

    // 현재 패턴을 실제로 그리고 있는지
    bool drawMode = false;

    // 이번 시도에서 밟은 노드 순서
    readonly List<string> visitedNodeIds = new List<string>();

    // 이번 시도에서 사용한 간선들 (A-B / B-A 동일 처리)
    readonly HashSet<(string,string)> usedEdges = new HashSet<(string,string)>();

    // 패턴 완성 판정 후 마지막 노드에 도착했을 때 Success() 호출 예약
    bool successPending = false;

    // 입력 잠깐 봉인용. 0이면 봉인 해제.
    float _inputBlockUntil = 0f;

    protected override void Awake()
    {
        base.Awake();

        // 인스펙터에서 currentNode를 세팅해둔 경우 시작 위치를 그 노드로 맞춘다
        if (currentNode != null)
        {
            targetPos = currentNode.transform.position;
            transform.position = targetPos;
            autoTransit = false;
        }
    }

    protected override void Start()
    {
        base.Start();
        SetActivated(false);
        if (q_Key) q_Key.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();

        if (!hasActivated || !isPossessed)
        {
            if (q_Key && q_Key.activeSelf) q_Key.SetActive(false);
        }
        else
        {
            if (q_Key && !q_Key.activeSelf) q_Key.SetActive(true);
        }
        
        if (!isPossessed && !autoTransit)
            return;

        // 자동 이동(컷신 이동) 상태
        if (autoTransit)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                autoTransitSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPos) <= snapRadius)
            {
                // 자동 이동 종료 후 대기
                autoTransit = false;
                SetActivated(true);
            }
            return;
        }

        // Q 눌러서 그리기 시작 (취소는 불가)
        if (Time.time >= _inputBlockUntil && Input.GetKeyDown(drawToggleKey))
        {
            if (!drawMode)
            {
                TurnOnDrawMode();
                UIManager.Instance?.PromptUI2.ShowPrompt_UnPlayMode("DrawMode On", 1.0f);
            }
            else
            {
                TurnOffDrawMode();
                UIManager.Instance?.PromptUI2.ShowPrompt_UnPlayMode("DrawMode Off", 1.0f);
            }
        }

        // 현재 위치 -> targetPos 로 플레이어 조종 이동
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        bool snapped = Vector3.Distance(transform.position, targetPos) <= snapRadius;

        if (snapped)
        {
            // 방향키 입력으로 인접 노드 선택 전에,
            // 패턴 완성 여부를 한 번 더 확인해서 바로 처리한다.
            if (drawMode && currentPattern != null && DrawingLooksFinished())
            {
                // 바로 성공 처리: 대기/코루틴/지연 없음
                Success();
                return;
            }
        }

        // 아직 목표까지 안 왔으면 이동만 하고 리턴
        if (!snapped)
            return;

        // 여기서부터는 노드 위에 완전히 붙어 있는 상태

        // 방향키 입력으로 인접 노드 선택
        Vector2 dir = Vector2.zero;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector2.right;

        if (dir == Vector2.zero)
            return;

        var next = ChooseNeighborByDirection(currentNode, dir);
        if (next == null)
        {
            SoundManager.Instance?.PlaySFX(controller?.sfxBump);
            return;
        }

        // 그리는 중이면 간선 재사용 제약 등 검사
        if (drawMode && currentPattern != null)
        {
            var e = EdgeKey(currentNode.Id, next.Id);

            if (currentPattern.forbidEdgeReuse && usedEdges.Contains(e))
            {
                Fail("같은 선은 두 번 못 그린다.");
                return;
            }

            usedEdges.Add(e);
        }

        // 실제 이동(다음 노드로)
        currentNode = next;
        targetPos = currentNode.transform.position;

        // 라인 업데이트 및 패턴 완성 체크
        if (drawMode && currentPattern != null)
        {
            visitedNodeIds.Add(currentNode.Id);

            if (line)
            {
                line.positionCount++;
                line.SetPosition(line.positionCount - 1, targetPos);
            }

            if (DrawingLooksFinished())
            {
                successPending = true;
            }
        }
    }
    
    void TurnOnDrawMode()
    {
        drawMode = true;
        visitedNodeIds.Clear();
        usedEdges.Clear();

        if (currentNode != null)
        {
            visitedNodeIds.Add(currentNode.Id);

            if (line)
            {
                line.enabled = true;
                line.positionCount = 1;
                line.SetPosition(0, currentNode.transform.position);
            }
        }
    }
    
    void TurnOffDrawMode()
    {
        drawMode = false;
        successPending = false;
        visitedNodeIds.Clear();
        usedEdges.Clear();

        if (line)
        {
            line.enabled = false;
            line.positionCount = 0;
        }
    }

    // 현재 컨트롤러 단계에 맞는 패턴을 세팅
    public void SetPattern(Ch4_PatternAsset pattern)
    {
        currentPattern = pattern;
        if (currentPattern == null) return;
        if (layout == null) return;
        if (currentPattern.nodeIdSequence == null || currentPattern.nodeIdSequence.Count == 0) return;

        string startId = currentPattern.nodeIdSequence[0];
        Ch4_SpiderWebNode startNode = FindNodeById(startId);

        if (startNode != null)
        {
            currentNode = startNode;
            targetPos = currentNode.transform.position;
            autoTransit = false;
        }
        else
        {
            currentNode = layout.FindNearestNode(transform.position);
            if (currentNode != null)
                targetPos = currentNode.transform.position;
            else
                targetPos = transform.position;

            autoTransit = false;
        }

        SetActivated(true);

        TurnOffDrawMode();
    }

    // 패턴 완성 판정
    bool DrawingLooksFinished()
    {
        if (!drawMode) return false;
        if (currentPattern == null) return false;
        return MatchesShapeAsEdges(usedEdges, currentPattern.nodeIdSequence);
    }

    bool MatchesShapeAsEdges(HashSet<(string,string)> drawnEdges, List<string> goalSeq)
    {
        var goalEdges = new HashSet<(string,string)>();
        for (int i = 0; i < goalSeq.Count - 1; i++)
            goalEdges.Add( EdgeKey(goalSeq[i], goalSeq[i+1]) );

        if (drawnEdges.Count != goalEdges.Count)
            return false;

        foreach (var e in drawnEdges)
        {
            if (!goalEdges.Contains(e))
                return false;
        }
        return true;
    }

    (string,string) EdgeKey(string a, string b)
    {
        return string.CompareOrdinal(a, b) < 0 ? (a,b) : (b,a);
    }

    // 패턴 최종 성공 처리
    void Success()
    {
        // 사운드/메시지
        SoundManager.Instance?.PlaySFX(controller?.sfxPatternSuccess);
        UIManager.Instance?.PromptUI2.ShowPrompt_UnPlayMode("성공", 1.2f);

        // 컨트롤러에 보고해서 step 업데이트 / 인형 내리기 등 진행
        Ch4_PatternAsset solvedPattern = currentPattern;
        if (controller && currentPattern != null)
        {
            controller.OnSpiderPatternSolved(currentPattern);
        }

        // 다음 패턴이 있는지 확인
        Ch4_PatternAsset nextPattern = (controller != null)
            ? controller.GetNextPatternAfter(solvedPattern)
            : null;

        // 플레이어 조작 종료: 강제 빙의 해제
        if (isPossessed)
        {
            Unpossess();
        }
        SetActivated(false); // 플레이어 입력 잠궈둔다 (지금은 거미 혼자 움직이는 구간)

        // 라인/상태 초기화
        drawMode = false;
        successPending = false;
        visitedNodeIds.Clear();
        usedEdges.Clear();
        if (line)
        {
            line.enabled = false;
            line.positionCount = 0;
        }

        // 마지막 패턴이었다면 거미는 사라진다
        if (nextPattern == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // 다음 패턴 시작 노드까지 자동 이동 준비
        if (nextPattern.nodeIdSequence != null && nextPattern.nodeIdSequence.Count > 0)
        {
            string startId = nextPattern.nodeIdSequence[0];
            var startNode = FindNodeById(startId);
            if (startNode != null)
            {
                currentNode = startNode;
                targetPos = startNode.transform.position;
                autoTransit = true;
            }
        }

        // 다음에 플레이어가 다시 빙의했을 때 이 nextPattern을 그리게 할 거니까
        currentPattern = nextPattern;
    }

    void Fail(string reason)
    {
        SoundManager.Instance?.PlaySFX(controller?.sfxPatternFail);
        UIManager.Instance?.PromptUI2.ShowPrompt_UnPlayMode(reason, 1.2f);

        drawMode = false;
        successPending = false;
        visitedNodeIds.Clear();
        usedEdges.Clear();

        if (line)
        {
            line.enabled = false;
            line.positionCount = 0;
        }
    }

    // 방향키로 갈 수 있는 다음 노드 선택
    Ch4_SpiderWebNode ChooseNeighborByDirection(Ch4_SpiderWebNode from, Vector2 dir)
    {
        if (from == null) return null;

        float bestDot = 0.5f;
        Ch4_SpiderWebNode best = null;

        var neighbors = layout.GetNeighbors(from);
        foreach (var n in neighbors)
        {
            if (!n) continue;
            Vector2 ndir = ((Vector2)n.transform.position - (Vector2)from.transform.position).normalized;
            float dot = Vector2.Dot(dir, ndir);
            if (dot > bestDot)
            {
                bestDot = dot;
                best = n;
            }
        }
        return best;
    }

    // nodeId ("A","B",...)로 실제 노드 찾기
    Ch4_SpiderWebNode FindNodeById(string nodeId)
    {
        if (layout == null || string.IsNullOrEmpty(nodeId)) return null;

        foreach (var n in layout.Nodes)
        {
            if (n != null && n.Id == nodeId)
                return n;
        }
        return null;
    }

    // 플레이어가 거미에 빙의 완료했을 때
    public override void OnPossessionEnterComplete()
    {
        base.OnPossessionEnterComplete();

        // 현재 위치 기준으로 가장 가까운 노드를 currentNode로 잡고
        if (currentNode == null && layout != null)
        {
            var nearest = layout.FindNearestNode(transform.position);
            if (nearest != null)
                currentNode = nearest;
        }

        // 그 노드 좌표를 targetPos로 설정만 한다 (즉시 순간이동 금지)
        if (currentNode != null)
            targetPos = currentNode.transform.position;
        else
            targetPos = transform.position;

        // 이제는 플레이어가 직접 조종하는 시간
        autoTransit = false;
        SetActivated(true);

        // 패턴 그리기 준비 상태 초기화
        drawMode = false;
        successPending = false;
        visitedNodeIds.Clear();
        usedEdges.Clear();
        if (line)
        {
            line.enabled = false;
            line.positionCount = 0;
        }

        // 현재 단계에 맞는 패턴을 다시 주입 (pattern1→pattern2→pattern3 순서)
        if (controller != null)
        {
            controller.ApplyCurrentPatternToSpider(this);
        }
    }
}
