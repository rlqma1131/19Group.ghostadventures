using System.Collections;
using UnityEngine;

public static class SoundTriggerer
{
    public static void TriggerSound(Vector3 soundPosition)
    {
        // 소리 위치를 나타내는 임시 GameObject 생성
        GameObject soundMarker = new GameObject("SoundMarker");
        soundMarker.transform.position = soundPosition;

        EnemyAI[] enemies = GameObject.FindObjectsOfType<EnemyAI>();
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                Debug.Log($"[SoundTriggerer] {enemy.name} 소리에 반응 → 위치: {soundPosition}");
            }
        }

        GameObject.Destroy(soundMarker, 6f);
    }
}
