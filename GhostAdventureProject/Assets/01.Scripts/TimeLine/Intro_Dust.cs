using UnityEngine;

public class Intro_Dust : MonoBehaviour
{
    //인트로씬에서 플레이어가 땅에 착지할때 먼지 파티클 재생 스크립트
    public GameObject dustEffectPrefab;  // 먼지 파티클 프리팹

    public void PlayDustEffect()
    {
        if (dustEffectPrefab != null)
        {
            Instantiate(dustEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}
