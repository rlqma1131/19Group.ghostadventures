using UnityEngine;

public static class SoundTriggerer
{
    public static void TriggerSound(Vector3 soundPosition)
    {
        // 1. 소리 위치를 나타내는 임시 GameObject 생성
        GameObject soundMarker = new GameObject("SoundMarker");
        soundMarker.name = "SoundMarker";
        soundMarker.transform.position = soundPosition;

        // 2. 모든 EnemyAI에게 전달
        EnemyAI[] enemies = GameObject.FindObjectsOfType<EnemyAI>();
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                Debug.Log($"[SoundTriggerer] {enemy.name} 에게 소리 전달 → 위치: {soundPosition}");
                enemy.GetDistractedBy(soundMarker.transform);
            }
        }

        // 3. 6초 후 자동 제거
        GameObject.Destroy(soundMarker, 6f);
    }
}
