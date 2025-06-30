using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_Rat : MoveBasePossessable
{
    private Animator anim;
    [SerializeField] private Vector2 point1 = new Vector2(-3.5f, -1.5f); // 포인트1 좌표
    [SerializeField] private Vector2 point2 = new Vector2(3.5f, -1.5f); // 포인트2 좌표

    protected override void Start()
    {
        base.Start();

        anim = GetComponentInChildren<Animator>();

        hasActivated = false;
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnActivate();
        }
    }

    public void ActivateRat()
    {
        hasActivated = true;
        anim.SetBool("Move", true);
        // 포인트1로 이동
        anim.SetTrigger("Escape");
        // 포인트2로 이동
    }

    public void OnActivate()
    {
        // 고유 기능 로직
    }
}
