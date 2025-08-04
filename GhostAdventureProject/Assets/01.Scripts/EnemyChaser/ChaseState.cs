using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : EnemyState
{
    public ChaseState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.Animator.SetBool("IsWalking", true);
    }

    public override void FixedUpdate()
    {
        enemy.Movement.ChasePlayer();
    }

    public override void Update()
    {
        if (!enemy.Detection.CanSeePlayer())
            enemy.ChangeState(enemy.PatrolState);
    }

    public override void Exit()
    {
        enemy.Animator.SetBool("IsWalking", false);
    }
}
