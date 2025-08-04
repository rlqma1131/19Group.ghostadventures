using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTeleportState : EnemyState
{
    private bool teleportEnded = false;
    private Vector3 targetPosition;
    private float chaseDuration;
    private Vector3 originalPosition;
    private bool isReturning;

    public SoundTeleportState(EnemyAI enemy, Vector3 targetPos, float duration, bool returning = false)
        : base(enemy)
    {
        targetPosition = targetPos;
        chaseDuration = duration;
        originalPosition = enemy.startPosition;
        isReturning = returning;
    }

    public override void Enter()
    {
        teleportEnded = false;

        // 순간이동
        enemy.transform.position = targetPosition;

        // 텔레포트 애니메이션
        enemy.Animator.SetTrigger("SoundTeleport");

        // 애니메이션 종료 대기
        enemy.StartCoroutine(WaitForTeleportEnd());
    }

    public override void Exit()
    {
        teleportEnded = false;
    }

    public override void Update() { }
    public override void FixedUpdate() { }

    private IEnumerator WaitForTeleportEnd()
    {
        float timeout = 2f; // 애니메이션 길이보다 조금 길게 설정
        float timer = 0f;

        while (!teleportEnded && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!isReturning)
        {
            enemy.ChangeState(enemy.ChaseState);
            yield return new WaitForSeconds(chaseDuration);

            var returnState = new SoundTeleportState(enemy, originalPosition, 0f, true);
            enemy.ChangeState(returnState);
        }
        else
        {
            enemy.ChangeState(enemy.PatrolState);
        }
    }

    public void OnTeleportAnimationEnd()
    {
        teleportEnded = true;
    }
}
