using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : EnemyState
{
    public PatrolState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.Animator.SetBool("IsWalking", true);
        enemy.Movement.PickRandomDirection();
    }

    public override void FixedUpdate()
    {
        enemy.Movement.PatrolMove();
    }

    public override void Update()
    {
        if (enemy.Detection.CanSeePlayer())
            enemy.ChangeState(enemy.ChaseState);
    }

    public override void Exit()
    {
        enemy.Animator.SetBool("IsWalking", false);
    }
}
