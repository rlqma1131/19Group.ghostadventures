using UnityEngine;
using System.Collections;

public class EnemySoundResponseSimple : MonoBehaviour
{
    [Header("디버그")]
    [SerializeField] private bool showDebugInfo = true;

    private EnemyAI enemyAI;
    private EnemyMovement movement;
    private float originalDetectionRange;
    private bool isEnhancedDetection = false;
    private Coroutine currentResponse;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        movement = GetComponent<EnemyMovement>();
        originalDetectionRange = enemyAI.detectionRange;
    }

    public void OnSoundTriggered(SoundTriggerData data)
    {
        // 이미 반응 중이면 중단하고 새로 시작
        if (currentResponse != null)
        {
            StopCoroutine(currentResponse);
            EndEnhancedDetection();
        }

        currentResponse = StartCoroutine(HandleSoundResponse(data));
    }

    private IEnumerator HandleSoundResponse(SoundTriggerData data)
    {
        if (showDebugInfo)
            Debug.Log($"[{gameObject.name}] 소리 감지! {data.spawnDelay}초 후 {data.targetDoor.name}로 이동");

        // 1. 지연 시간 대기
        yield return new WaitForSeconds(data.spawnDelay);

        // 2. Target Door 앞으로 텔레포트
        TeleportToTargetDoor(data);

        // 3. 강화된 탐지 시작
        StartEnhancedDetection(data.detectionMultiplier);

        // 4. 강화 탐지 지속시간 대기
        yield return new WaitForSeconds(data.enhancedDetectionTime);

        // 5. 원래 상태로 복귀
        EndEnhancedDetection();

        currentResponse = null;
    }

    private void TeleportToTargetDoor(SoundTriggerData data)
    {
        // Target Door 위치 + 오프셋
        Vector3 teleportPosition = data.targetDoor.position + data.spawnOffset;

        // Y축 고정 적용
        if (enemyAI.lockYPosition)
        {
            teleportPosition.y = enemyAI.fixedYPosition;
        }

        transform.position = teleportPosition;

        // 순찰 상태로 변경 (주변 탐색 시작)
        enemyAI.ChangeState(EnemyAI.AIState.Patrolling);

        if (showDebugInfo)
            Debug.Log($"[{gameObject.name}] {data.targetDoor.name} 앞으로 텔레포트 완료: {teleportPosition}");
    }

    private void StartEnhancedDetection(float multiplier)
    {
        isEnhancedDetection = true;
        enemyAI.detectionRange = originalDetectionRange * multiplier;

        if (showDebugInfo)
            Debug.Log($"[{gameObject.name}] 강화된 탐지 시작! 범위: {originalDetectionRange} → {enemyAI.detectionRange}");
    }

    private void EndEnhancedDetection()
    {
        if (isEnhancedDetection)
        {
            isEnhancedDetection = false;
            enemyAI.detectionRange = originalDetectionRange;

            if (showDebugInfo)
                Debug.Log($"[{gameObject.name}] 강화된 탐지 종료. 원래 범위로 복귀: {enemyAI.detectionRange}");
        }
    }

    // 강제 종료 (필요시)
    public void ForceEndResponse()
    {
        if (currentResponse != null)
        {
            StopCoroutine(currentResponse);
            currentResponse = null;
        }
        EndEnhancedDetection();
    }

    private void OnDestroy()
    {
        ForceEndResponse();
    }

    // 디버깅용 기즈모
    private void OnDrawGizmosSelected()
    {
        if (isEnhancedDetection)
        {
            // 강화된 탐지 범위 표시 (주황색)
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, enemyAI.detectionRange);
        }
    }
}