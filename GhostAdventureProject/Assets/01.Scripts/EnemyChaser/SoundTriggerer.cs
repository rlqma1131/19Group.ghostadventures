using UnityEngine;

public static class SoundTriggerer
{
    public static void TriggerSound(Vector3 soundPosition)
    {
        // 1. 소리 위치를 나타내는 임시 GameObject 생성
        GameObject soundMarker = new GameObject("SoundMarker");
        soundMarker.transform.position = soundPosition;

        // 2. 모든 EnemyAI에게 전달
        EnemyAI[] enemies = GameObject.FindObjectsOfType<EnemyAI>();
        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(enemy.transform.position, soundPosition);
            if (distance <= enemy.distractionRange)
            {
                enemy.GetDistractedBy(soundMarker.transform);  // 핵심 변경!
            }
        }

        // 3. 6초 후 자동 제거
        GameObject.Destroy(soundMarker, 6f);
    }
}
