using UnityEngine;
using System.Collections;

[System.Serializable]
public class SoundTriggerData
{
    [Header("Target Door 설정")]
    public Transform targetDoor; // Inspector에서 드래그해서 설정

    [Header("Enemy 반응 설정")]
    public float spawnDelay = 2f; // 2초 후 스폰
    public float enhancedDetectionTime = 5f; // 5초간 강화 탐지
    public float detectionMultiplier = 2f; // 탐지 범위 2배

    [Header("스폰 위치 조정")]
    public Vector3 spawnOffset = new Vector3(0, 0, -1.5f); // 문에서의 오프셋
}

public class SoundTriggerObject : MonoBehaviour
{
    [Header("사운드 트리거 설정")]
    [SerializeField] private SoundTriggerData triggerData;

    [Header("디버그")]
    [SerializeField] private bool showDebugInfo = true;

    public void TriggerSound()
    {
        if (triggerData.targetDoor == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Target Door가 설정되지 않았습니다!");
            return;
        }

        // 모든 Enemy에게 알림
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();

        foreach (var enemy in enemies)
        {
            var response = enemy.GetComponent<EnemySoundResponseSimple>();
            if (response != null)
            {
                response.OnSoundTriggered(triggerData);
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] 소리 발생! Enemy들이 {triggerData.spawnDelay}초 후 {triggerData.targetDoor.name}로 이동합니다.");
        }
    }
}