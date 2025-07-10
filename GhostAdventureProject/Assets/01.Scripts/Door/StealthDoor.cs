using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class StealthDoor : MonoBehaviour
{
    [Header("감지 설정")]
    [SerializeField] private float detectionRange = 3f;  // 감지 거리
    [SerializeField] private float fadeSpeed = 2f;        // 서서히 보여지는 속도
    [SerializeField] private float maxAlpha = 1f;         // 최대로 보여질 투명도

    private SpriteRenderer spriteRenderer;
    private float currentAlpha = 0f;  // 현재 투명도
    private float targetAlpha = 0f;   // 목표 투명도

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetAlpha(0f); // 시작할 땐 안 보이게
    }

    private void Update()
    {
        CheckTargetDistance();
        UpdateAlpha();
    }

    // "Player" 또는 "Cat" 중 가장 가까운 오브젝트 감지
    private void CheckTargetDistance()
    {
        GameObject target = FindClosestTarget();

        if (target == null)
        {
            Debug.Log("타겟을 찾을 수 없습니다");
            return;
        }

        float distance = Vector2.Distance(transform.position, target.transform.position);
        Debug.Log($"타겟: {target.name}, 거리: {distance}, 감지범위: {detectionRange}");

        if (distance <= detectionRange)
            targetAlpha = maxAlpha;
        else
            targetAlpha = 0f;
    }

    // 가장 가까운 Player 또는 Cat 찾기
    private GameObject FindClosestTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] cats = GameObject.FindGameObjectsWithTag("Cat");

        GameObject closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject obj in players)
        {
            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist < closestDistance)
            {
                closest = obj;
                closestDistance = dist;
            }
        }

        foreach (GameObject obj in cats)
        {
            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist < closestDistance)
            {
                closest = obj;
                closestDistance = dist;
            }
        }

        return closest;
    }

    // 현재 알파값 → 목표 알파값으로 부드럽게 전환
    private void UpdateAlpha()
    {
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
        SetAlpha(currentAlpha);
    }

    // 실제 알파 적용
    private void SetAlpha(float alpha)
    {
        if (spriteRenderer == null) return;

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    // 감지 범위 시각화 (Scene 뷰용)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
