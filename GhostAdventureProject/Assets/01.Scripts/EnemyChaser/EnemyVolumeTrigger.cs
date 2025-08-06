using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EnemyVolumeTrigger : MonoBehaviour
{
    public Volume globalVolume; // 글로벌 볼륨
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRadius = 15f;

    public bool PlayerInTrigger = false; // 트리거 감지
    public bool PlayerFind = false; // 플레이어 찾음?
    public ColorAdjustments colorAdjustments; // 글로벌 볼륨 색상 조정 컴포넌트

    public Color farColor; //원래 글로벌볼륨 컬러
    [SerializeField]private Color closeColor = new Color(108f / 255f, 0, 0); // 가까이 갈수록 컬러

    private float t = 0f; // 현재 색상 보간 값 (0 = 원래색, 1 = 빨간색)
    public bool Ondead =false;
    

    void Start()
    {
        if (globalVolume == null)
        {
            globalVolume = GetComponentInChildren<Volume>();
        }

        if (globalVolume.profile.TryGet<ColorAdjustments>(out var ca))
        {
            colorAdjustments = ca;
        }

        globalVolume.enabled = true;
        farColor= colorAdjustments.colorFilter.value; // 초기 색상 설정
        
    }

    void Update()
    {
        if (!PlayerFind)
        {
            GameObject playerObject = GameManager.Instance.Player;
            if (playerObject != null)
            {
                player = playerObject.transform;
                PlayerFind = true;
            }
        }

        if (player == null || colorAdjustments == null) return;
        if (!PlayerInTrigger && Mathf.Approximately(t, 0f)) return;

        float targetT = 0f;

        if (UIManager.Instance.QTE_UI_2.isdead)
        {
            targetT = 0f;
        }
        else if (PlayerInTrigger && !Ondead && !UIManager.Instance.QTE_UI_2.isdead)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            targetT = Mathf.Clamp01(1 - (distance / detectionRadius));
            targetT = Mathf.Pow(targetT, 0.5f);
            SoundManager.Instance.EnemySource.volume = Mathf.Lerp(0.01f, 0.3f, targetT);
        }

        t = Mathf.Lerp(t, targetT, Time.deltaTime * 2f);
        colorAdjustments.colorFilter.value = Color.Lerp(farColor, closeColor, t);
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SoundManager.Instance.FadeOutAndStopBGM(1f); // BGM 페이드 아웃
            SoundManager.Instance.FadeInLoopingSFX(SoundManager.Instance.EnemySource.clip, 1f, 0.5f);
            PlayerInTrigger = true;
            globalVolume.enabled = true;
        }

        if (collision.CompareTag("Volume"))
        {
            globalVolume = collision.GetComponentInChildren<Volume>();
            if (globalVolume.profile.TryGet<ColorAdjustments>(out var ca))
            {
                colorAdjustments = ca;
                farColor = ca.colorFilter.value;
            }
        }
    }



    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!collision.gameObject.activeInHierarchy)
                return;

            PlayerInTrigger = false;
            SoundManager.Instance.FadeOutAndStopLoopingSFX(1f);

            // 이전 BGM 복원
            SoundManager.Instance.RestoreLastBGM(1f);
            // globalVolume.enabled = false;

        }
    }

    void OnDrawGizmos()
    {
        if (globalVolume != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
