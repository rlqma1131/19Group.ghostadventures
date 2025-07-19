using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EnemyVolumeTrigger : MonoBehaviour
{
    [SerializeField] private Volume globalVolume; // 글로벌 볼륨
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRadius = 15f;

    private bool PlayerInTrigger = false; // 트리거 감지
    private bool PlayerFind = false; // 플레이어 찾음?
    private ColorAdjustments colorAdjustments; // 글로벌 볼륨 색상 조정 컴포넌트

    [SerializeField]private Color farColor; //원래 글로벌볼륨 컬러
    [SerializeField]private Color closeColor = new Color(108f / 255f, 0, 0); // 가까이 갈수록 컬러

    private float t = 0f; // 현재 색상 보간 값 (0 = 원래색, 1 = 빨간색)

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
        if (!PlayerFind && player == null)
        {
            GameObject playerObject = GameManager.Instance.Player;
            if (playerObject != null)
            {
                player = playerObject.transform;
                PlayerFind = true;
            }
        }

        if (player == null || colorAdjustments == null) return;

        float targetT = 0f;

        if (PlayerInTrigger)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            targetT = Mathf.Clamp01(1 - (distance / detectionRadius));
            targetT = Mathf.Pow(targetT, 0.5f);
        }

        // t를 부드럽게 변화시킴
        t = Mathf.Lerp(t, targetT, Time.deltaTime * 2f); // 수치 클수록 빠르게 변화

        // 색상 보간 적용
        colorAdjustments.colorFilter.value = Color.Lerp(farColor, closeColor, t);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerInTrigger = true;
            globalVolume.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerInTrigger = false;
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
