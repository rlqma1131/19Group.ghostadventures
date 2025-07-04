using UnityEngine;

public class StealthDoor : MonoBehaviour
{
    [Header("Stealth Door Settings")]
    [SerializeField] private float detectionRange = 3f; // 플레이어 감지 범위
    [SerializeField] private float fadeSpeed = 5f; // 페이드 속도
    [SerializeField] private float maxAlpha = 0.8f; // 최대 투명도 (0.8 = 80%)

    private GameObject player;
    private SpriteRenderer doorSpriteRenderer;
    private float targetAlpha = 0f;
    private float currentAlpha = 0f;
    private Color originalColor;

    private void Start()
    {
        // 컴포넌트 찾기
        doorSpriteRenderer = GetComponent<SpriteRenderer>();

        if (doorSpriteRenderer != null)
        {
            originalColor = doorSpriteRenderer.color;

            // 항상 안 보이는 상태로 시작
            currentAlpha = 0f;
            targetAlpha = 0f;
            SetDoorAlpha(currentAlpha);
        }
    }

    private void Update()
    {
        // 플레이어 거리 체크
        CheckPlayerDistance();

        // 알파값 부드럽게 변경
        UpdateDoorVisibility();
    }

    private void CheckPlayerDistance()
    {
        // 플레이어가 없으면 다시 찾기
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance <= detectionRange)
        {
            // 플레이어가 범위 안에 있으면 문 보이기 (최대 80%)
            targetAlpha = maxAlpha;
        }
        else
        {
            // 플레이어가 범위 밖에 있으면 문 숨기기
            targetAlpha = 0f;
        }
    }

    private void UpdateDoorVisibility()
    {
        if (doorSpriteRenderer == null) return;

        // 부드럽게 알파값 변경
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
        SetDoorAlpha(currentAlpha);
    }

    private void SetDoorAlpha(float alpha)
    {
        if (doorSpriteRenderer != null)
        {
            Color newColor = originalColor;
            newColor.a = alpha;
            doorSpriteRenderer.color = newColor;
        }
    }
}