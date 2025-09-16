using System.Collections;
using System.Collections.Generic;
using _01.Scripts.Player;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EnemyVolumeTrigger : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Player player;
    [SerializeField] private float detectionRadius = 15f;
    
    [Header("State")]
    public bool PlayerInTrigger = false;
    public bool PlayerFind = false;
    public bool Ondead = false;

    private int _id;
    private float t = 0f; // 0..1 보간(강도)

    private void Start()
    {
        _id = GetInstanceID();

        // 플레이어 캐시 1회
        player = GameManager.Instance ? GameManager.Instance.Player : GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        if (player) PlayerFind = true; 

        // 매니저 없으면 자동 생성
        if (EnemyVolumeOverlay.Instance == null)
        {
            var go = new GameObject("EnemyVolumeOverlay");
            go.AddComponent<EnemyVolumeOverlay>();
        }
    }

    private void Update()
    {
        // player 참조 보강 -> Nice!
        if (!PlayerFind) {
            player = GameManager.Instance ? GameManager.Instance.Player : GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            if (player) PlayerFind = true;
            return;
        }
        
        if (!EnemyVolumeOverlay.Instance) return;

        // 빙의 중이면 빙의 대상, 아니면 player
        Transform target = null;
        if (PossessionStateManager.Instance
            && PossessionStateManager.Instance.IsPossessing()
            && player
            && player.PossessionSystem.PossessedTarget)
        {
            target = player.PossessionSystem.PossessedTarget.transform;
        }
        else if (player)
        {
            target = player.transform;
        }
        
        if (!target) return;

        bool isDead = UIManager.Instance
                      && UIManager.Instance.QTE_UI_2
                      && UIManager.Instance.QTE_UI_2.isdead;

        float distance = Vector3.Distance(transform.position, target.position);
        bool inRange = !Ondead && !isDead && distance <= detectionRadius;

        // 엣지 사운드 처리만 유지
        if (inRange && !PlayerInTrigger)
        {
            if (SoundManager.Instance)
            {
                SoundManager.Instance.FadeOutAndStopBGM(1f);
                if (SoundManager.Instance.EnemySource)
                    SoundManager.Instance.FadeInLoopingSFX(SoundManager.Instance.EnemySource.clip, 1f, 0.5f);
            }
        }
        else if (!inRange && PlayerInTrigger)
        {
            if (SoundManager.Instance)
            {
                SoundManager.Instance.FadeOutAndStopLoopingSFX(1f);
                SoundManager.Instance.RestoreLastBGM(1f);
            }
        }
        PlayerInTrigger = inRange;

        // 강도 계산(가까울수록 강함)
        float targetT = 0f;
        if (inRange)
        {
            float norm = Mathf.Clamp01(1f - (distance / detectionRadius));
            targetT = Mathf.Pow(norm, 0.5f);

            if (SoundManager.Instance && SoundManager.Instance.EnemySource)
                SoundManager.Instance.EnemySource.volume = Mathf.Lerp(0.01f, 0.3f, targetT);
        }

        // 부드럽게 따라감
        t = Mathf.Lerp(t, targetT, Time.deltaTime * 2f);

        // ✅ 여기서 오버레이 매니저에 “내 강도”만 보고
        EnemyVolumeOverlay.Instance.Report(_id, t);
    }

    private void OnDisable()
    {
        if (EnemyVolumeOverlay.Instance != null)
            EnemyVolumeOverlay.Instance.Clear(_id);
    }

    private void OnDestroy()
    {
        if (EnemyVolumeOverlay.Instance != null)
            EnemyVolumeOverlay.Instance.Clear(_id);
    }
}