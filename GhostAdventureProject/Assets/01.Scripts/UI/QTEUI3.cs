using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QTEUI3 : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform needle; // 바늘 피벗
    public Transform ringTransform; // 성공영역 프리팹이 생성될 위치
    public GameObject zonePrefab; // 성공 영역 프리팹

    [Header("Settings")] // - 난이도 셋팅용
    public int successZoneCount = 2; // 성공 영역 개수
    public float minZoneSize = 10f; // 
    public float maxZoneSize = 40f; // 
    public float rotationSpeed = 45f; // degrees per second
    public float timeLimit = 3f; //

    private float currentAngle = 0f;
    private float timer = 0f;

    private List<QTERingZone> successZones = new();
    private HashSet<int> clearedZoneIndices = new();

    private bool isRunning = false;
    private bool wasSuccess = false;
    private Action<bool> resultCallback;
    private float previousAngle = 0f;
    private HashSet<int> pendingZoneIndices = new();



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
    foreach (int i in new List<int>(pendingZoneIndices))
    {
        if (ZonePassed(successZones[i], previousAngle, currentAngle))
        {
            if (!clearedZoneIndices.Contains(i))
            {
                // ❌ 통과 전에 Space를 안 눌렀음 → 실패
                EndQTE(false);
                return;
            }

            pendingZoneIndices.Remove(i); // 통과 완료
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

        // ✅ 모든 영역 성공 시
        if (clearedZoneIndices.Count == successZones.Count)
        {
            EndQTE(true);
        }

    }    
    }

    public void ShowQTEUI3()
    {
        gameObject.SetActive(true);
        isRunning = true;
        timer = 0f;
        currentAngle = 0f;
        clearedZoneIndices.Clear();
        pendingZoneIndices.Clear();

        // 기존 Zone 제거
        foreach (Transform child in ringTransform)
            Destroy(child.gameObject);

        successZones = GenerateZones(successZoneCount, minZoneSize, maxZoneSize);
        foreach (var zone in successZones)
        {
            CreateZoneVisual(zone);
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
            float start = UnityEngine.Random.Range(0f, 360f);
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
}}
