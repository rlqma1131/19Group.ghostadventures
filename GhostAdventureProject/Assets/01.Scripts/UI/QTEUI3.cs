using System;
using System.Collections.Generic;
using _01.Scripts.Player;
using UnityEngine;
using UnityEngine.UI;

public class QTEUI3 : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform needle;        // 바늘 피벗
    public Transform ringTransform;     // 성공영역 프리팹이 생성될 위치
    public GameObject zonePrefab;       // 성공 영역 프리팹

    [Header("Settings")] // - 난이도 셋팅용(PersonConditionHandler에서 변경)
    public int successZoneCount = 2;    // 성공 영역 개수
    public float minZoneSize = 10f;     // 성공 영역 최소사이즈
    public float maxZoneSize = 40f;     // 성공 영역 최대사이즈
    public float rotationSpeed = 45f;   // degrees per second
    public float timeLimit = 3f;        // 제한시간

    private float currentAngle = 0f;    // 현재 각
    private float previousAngle = 0f;   // 이전 각
    private float timer = 0f;           // 현재 시간
    private bool isRunning = false;     // 실행중인지 확인
    // private bool wasSuccess = false;

    private List<QTERingZone> successZones = new();     // 성공 영역들 
    private HashSet<int> clearedZoneIndices = new();    // 클리어한 성공 영역들
    private HashSet<int> pendingZoneIndices = new();    // 아직 클리어하지 않은 성공 영역들
    private Action<bool> resultCallback;                

    [SerializeField] AudioClip successSound;            // 성공 사운드
    [SerializeField] AudioClip failSound;               // 실패 사운드

    Player player;

    public void Initialize(Player player) {
        this.player = player;
    }

    void Update()
    {
        if (!isRunning) return;

        timer += Time.unscaledDeltaTime;
        if (timer > timeLimit)
        {
            EndQTE(false); // 제한시간 초과
            return;
        }

        // 회전
        previousAngle = currentAngle;
        currentAngle += rotationSpeed * Time.unscaledDeltaTime;
        currentAngle %= 360f;
        needle.localEulerAngles = new Vector3(0, 0, -currentAngle);

        // ✅ 통과 체크
       var pendingList = new List<int>(pendingZoneIndices);
    pendingList.Sort((a, b) => b.CompareTo(a)); // 큰 인덱스 먼저

    foreach (int i in pendingList)
    {
        // 삭제로 리스트가 줄었을 수 있으니 가드
        if (i < 0 || i >= successZones.Count)
        {
            pendingZoneIndices.Remove(i);
            continue;
        }

        if (ZonePassed(successZones[i], previousAngle, currentAngle))
        {
            if (!clearedZoneIndices.Contains(i))
            {
                EndQTE(false); // 안 누르고 지나감 → 실패
                SoundManager.Instance.PlaySFX(failSound, 0.5f);
                return;
            }

            // 누르고 지나감 → 이 즉시 삭제
            RemoveZoneAt(i); // 아래 헬퍼
            SoundManager.Instance.PlaySFX(successSound, 0.5f);

            if (successZones.Count == 0)
            {
                EndQTE(true);
                return;
            }
            // 내림차순 스냅샷이라 계속 돌려도 안전
        }
    }

        // ✅ 입력 체크
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool hit = false;

            for (int i = 0; i < successZones.Count; i++)
            {
                if (clearedZoneIndices.Contains(i)) continue;

                if (IsInZone(currentAngle, successZones[i]))
                {
                    clearedZoneIndices.Add(i);
                    Debug.Log($"성공 영역 {i + 1} 통과!");
                    hit = true;
                }
            }

            if (!hit)
            {
                // ❌ 아무 영역 위에 없는데 누름 → 실패
                EndQTE(false);
                return;
            }

            //✅ 모든 영역 성공 시
            if (clearedZoneIndices.Count == successZones.Count)
            {
                EndQTE(true);
            }

        }    
    }

    public void ShowQTEUI3()
    {
        BasePossessable possessed = player.PossessionSystem.CurrentTarget;
        PersonConditionUI conditionUI = possessed.GetComponent<PersonConditionUI>();
        conditionUI.SetCondition(conditionUI.currentCondition);

        isRunning = true;
        gameObject.SetActive(true);
        timer = 0f;
        currentAngle = 0f;
        clearedZoneIndices.Clear();
        pendingZoneIndices.Clear();

        // 기존 Zone 제거
        foreach (Transform child in ringTransform)
            Destroy(child.gameObject);

        successZones = GenerateZones(successZoneCount, minZoneSize, maxZoneSize);
        for (int i = 0; i < successZones.Count; i++)
        {
            CreateZoneVisual(successZones[i]);
            pendingZoneIndices.Add(i); // ← 추가
        }
    }

    public void EndQTE(bool success)
    {
        isRunning = false;
        InvokeResult(success);
        Debug.Log(success ? "QTE 성공!" : "QTE 실패!");
    }

    List<QTERingZone> GenerateZones(int count, float minSize, float maxSize)
    {
        List<QTERingZone> zones = new List<QTERingZone>();
        int attempts = 0;

        while (zones.Count < count && attempts++ < 500)
        {
            float size = UnityEngine.Random.Range(minSize, maxSize);
            float start = UnityEngine.Random.Range(90f, 340f); // 성공 지점 스폰 범위
            float end = (start + size) % 360f;

            var candidate = new QTERingZone { startAngle = start, endAngle = end };

            bool overlaps = false;
            foreach (var zone in zones)
            {
                if (DoZonesOverlap(candidate, zone))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
                zones.Add(candidate);
        }

        return zones;
    }

    void CreateZoneVisual(QTERingZone zone)
    {
        GameObject zoneObj = Instantiate(zonePrefab, ringTransform);
        zone.visual = zoneObj;
        Image img = zoneObj.GetComponent<Image>();

        float zoneSize = (zone.endAngle >= zone.startAngle)
            ? zone.endAngle - zone.startAngle
            : (360f - zone.startAngle + zone.endAngle);

        img.fillAmount = zoneSize / 360f;

        zoneObj.transform.localEulerAngles = new Vector3(0, 0, -zone.startAngle);
    }

    bool IsInZone(float angle, QTERingZone zone)
    {
        if (zone.startAngle < zone.endAngle)
            return angle >= zone.startAngle && angle <= zone.endAngle;
        else
            return angle >= zone.startAngle || angle <= zone.endAngle;
    }

    bool DoZonesOverlap(QTERingZone a, QTERingZone b)
    {
        float aStart = a.startAngle;
        float aEnd = a.endAngle;
        float bStart = b.startAngle;
        float bEnd = b.endAngle;

        if (aEnd < aStart) aEnd += 360f;
        if (bEnd < bStart) bEnd += 360f;

        return !(aEnd <= bStart || aStart >= bEnd);
    }

    [System.Serializable]
    public class QTERingZone
    {
        public float startAngle;
        public float endAngle;
        public GameObject visual; // ← 추가

    }

    private void InvokeResult(bool result)
    {
        if(resultCallback != null)
            resultCallback.Invoke(result);
        else
            PossessionQTESystem.Instance.HandleQTEResult(result);
        
        gameObject.SetActive(false);
    }

    bool ZonePassed(QTERingZone zone, float prev, float curr)
    {
        // 각도 보정 (wrap-around 방지)
        if (curr < prev)
            curr += 360f;

        float zStart = zone.startAngle;
        float zEnd = zone.endAngle;

        if (zEnd < zStart)
            zEnd += 360f;

        // ✅ prev ~ curr 사이에 zone이 존재했다면 "통과한 것"
        return prev < zEnd && curr >= zEnd;
    }
    public void ApplySettings(QTESettings settings)
    {
        rotationSpeed = settings.rotationSpeed;
        timeLimit = settings.timeLimit;
        successZoneCount = settings.successZoneCount;
        minZoneSize = settings.minZoneSize;
        maxZoneSize = settings.maxZoneSize;
    }

    void RemoveZoneAt(int removeIdx)
{
    var z = successZones[removeIdx];
    if (z.visual != null) Destroy(z.visual); // 비주얼 제거

    successZones.RemoveAt(removeIdx);

    // 인덱스 세트 재매핑
    pendingZoneIndices = ReindexSet(pendingZoneIndices, removeIdx);
    clearedZoneIndices = ReindexSet(clearedZoneIndices, removeIdx);
}

HashSet<int> ReindexSet(HashSet<int> src, int removeIdx)
{
    var dst = new HashSet<int>();
    foreach (var idx in src)
    {
        if (idx == removeIdx) continue;     // 지운 인덱스 제거
        dst.Add(idx > removeIdx ? idx - 1 : idx); // 뒤는 -1
    }
    return dst;
}

}
