using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class StealthDoor : MonoBehaviour
{
    [Header("감지 설정")]
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float maxAlpha = 1f;

    private SpriteRenderer spriteRenderer;
    private float currentAlpha = 0f;
    private float targetAlpha = 0f;

    // 최적화: 캐싱된 타겟 리스트
    private static List<GameObject> cachedPlayers = new List<GameObject>();
    private static List<GameObject> cachedCats = new List<GameObject>();
    private static float lastCacheUpdateTime = 0f;
    private const float CACHE_UPDATE_INTERVAL = 1f; // 1초마다 캐시 업데이트

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetAlpha(0f);

        // 첫 실행 시 캐시 초기화
        UpdateTargetCache();
    }

    private void OnEnable()
    {
        // 오브젝트가 활성화될 때마다 캐시 갱신
        lastCacheUpdateTime = 0f;
    }

    private void Update()
    {
        UpdateTargetCacheIfNeeded();
        CheckTargetDistance();
        UpdateAlpha();
    }

    /// <summary>
    /// 필요할 때만 타겟 캐시 업데이트
    /// </summary>
    private void UpdateTargetCacheIfNeeded()
    {
        if (Time.time - lastCacheUpdateTime > CACHE_UPDATE_INTERVAL)
        {
            UpdateTargetCache();
            lastCacheUpdateTime = Time.time;
        }
    }

    /// <summary>
    /// Player와 Cat 오브젝트들을 캐싱
    /// </summary>
    private void UpdateTargetCache()
    {
        // 기존 캐시 정리 (파괴된 오브젝트 제거)
        cachedPlayers.RemoveAll(obj => obj == null);
        cachedCats.RemoveAll(obj => obj == null);

        // 새로운 오브젝트들 추가 (중복 방지)
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] cats = GameObject.FindGameObjectsWithTag("Cat");

        foreach (GameObject player in players)
        {
            if (!cachedPlayers.Contains(player))
                cachedPlayers.Add(player);
        }

        foreach (GameObject cat in cats)
        {
            if (!cachedCats.Contains(cat))
                cachedCats.Add(cat);
        }
    }

    /// <summary>
    /// 캐싱된 타겟들과의 거리 체크
    /// </summary>
    private void CheckTargetDistance()
    {
        GameObject target = FindClosestTargetFromCache();

        if (target == null)
        {
            targetAlpha = 0f;
            return;
        }

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance <= detectionRange)
            targetAlpha = maxAlpha;
        else
            targetAlpha = 0f;
    }

    /// <summary>
    /// 캐시에서 가장 가까운 타겟 찾기
    /// </summary>
    private GameObject FindClosestTargetFromCache()
    {
        GameObject closest = null;
        float closestDistance = Mathf.Infinity;

        // 캐싱된 Players 검사
        foreach (GameObject player in cachedPlayers)
        {
            if (player == null) continue; // null 체크

            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < closestDistance)
            {
                closest = player;
                closestDistance = dist;
            }
        }

        // 캐싱된 Cats 검사
        foreach (GameObject cat in cachedCats)
        {
            if (cat == null) continue; // null 체크

            float dist = Vector2.Distance(transform.position, cat.transform.position);
            if (dist < closestDistance)
            {
                closest = cat;
                closestDistance = dist;
            }
        }

        return closest;
    }

    /// <summary>
    /// 현재 알파값 → 목표 알파값으로 부드럽게 전환
    /// </summary>
    private void UpdateAlpha()
    {
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
        SetAlpha(currentAlpha);
    }

    /// <summary>
    /// 실제 알파 적용
    /// </summary>
    private void SetAlpha(float alpha)
    {
        if (spriteRenderer == null) return;

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    /// <summary>
    /// 외부에서 새로운 Player/Cat이 추가되었을 때 호출
    /// </summary>
    public static void AddNewTarget(GameObject target, string tag)
    {
        if (target == null) return;

        switch (tag)
        {
            case "Player":
                if (!cachedPlayers.Contains(target))
                    cachedPlayers.Add(target);
                break;
            case "Cat":
                if (!cachedCats.Contains(target))
                    cachedCats.Add(target);
                break;
        }
    }

    /// <summary>
    /// 외부에서 타겟이 제거되었을 때 호출
    /// </summary>
    public static void RemoveTarget(GameObject target)
    {
        cachedPlayers.Remove(target);
        cachedCats.Remove(target);
    }

    /// <summary>
    /// 모든 캐시 강제 업데이트 (씬 전환 시 등)
    /// </summary>
    public static void ForceUpdateCache()
    {
        if (cachedPlayers != null) cachedPlayers.Clear();
        if (cachedCats != null) cachedCats.Clear();
        lastCacheUpdateTime = 0f; // 다음 Update에서 즉시 갱신되도록
    }

    private void OnDestroy()
    {
        // 이 인스턴스가 파괴될 때 자신을 캐시에서 제거
        RemoveTarget(gameObject);
    }

    /// <summary>
    /// 감지 범위 시각화 (Scene 뷰용)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}