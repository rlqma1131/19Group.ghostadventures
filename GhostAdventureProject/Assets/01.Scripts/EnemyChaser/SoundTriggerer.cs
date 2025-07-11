using UnityEngine;

public static class SoundTriggerer
{
    public static void TriggerSound(Vector3 soundPosition)
    {
        // 1. 소리 위치를 나타내는 임시 GameObject 생성
        GameObject soundMarker = new GameObject("SoundMarker");
        soundMarker.transform.position = soundPosition;

        // 2. 모든 EnemyAI에게 전달 (거리 체크 제거)
        EnemyAI[] enemies = GameObject.FindObjectsOfType<EnemyAI>();
        foreach (var enemy in enemies)
        {
            // 거리 체크 없이 모든 Enemy가 소리에 반응하도록 변경
         
            enemy.GetDistractedBy(soundMarker.transform);
        }

        // 3. 6초 후 자동 제거
        GameObject.Destroy(soundMarker, 6f);

        
    }
}