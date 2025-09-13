using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class StealthDoor : MonoBehaviour
{
    [Header("감지 설정")]
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float maxAlpha = 0.4f;

    [Header("추가 감지 대상 (Cat 등)")]
    [SerializeField] private List<GameObject> extraTargets;

    private SpriteRenderer spriteRenderer;
    private float currentAlpha = 0f;
    private float targetAlpha = 0f;

    private readonly List<Transform> allTargets = new List<Transform>();
    private List<SpriteRenderer> allRenderers = new List<SpriteRenderer>();
    private bool playerRegistered = false;

    private void Start()
    {
        allRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>(includeInactive: true));
        SetAlpha(0f);
        RegisterTargets();
    }

    private void RegisterTargets()
    {
        allTargets.Clear();

        // Player 자동 등록
        if (GameManager.Instance?.PlayerObj != null)
        {
            allTargets.Add(GameManager.Instance.PlayerObj.transform);
        }

        // 인스펙터로 받은 추가 타겟 등록
        foreach (var go in extraTargets)
        {
            if (go != null)
                allTargets.Add(go.transform);
        }
    }

    private void Update()
    {
        if (!playerRegistered && GameManager.Instance?.PlayerObj != null)
        {
            allTargets.Add(GameManager.Instance.PlayerObj.transform);
            playerRegistered = true;
        }

        Transform nearest = FindNearestTarget();

        if (nearest != null)
        {
            float dist = Vector2.Distance(transform.position, nearest.position);
            targetAlpha = (dist <= detectionRange) ? maxAlpha : 0f;
        }
        else
        {
            targetAlpha = 0f;
        }

        UpdateAlpha();
    }

    private Transform FindNearestTarget()
    {
        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var t in allTargets)
        {
            if (t == null || !t.gameObject.activeInHierarchy) continue;

            float dist = Vector2.Distance(transform.position, t.position);
            if (dist < closestDist)
            {
                closest = t;
                closestDist = dist;
            }
        }

        return closest;
    }

    private void UpdateAlpha()
    {
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
        SetAlpha(currentAlpha);
    }

    private void SetAlpha(float alpha)
    {
        foreach (var renderer in allRenderers)
        {
            if (renderer == null) continue;

            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}