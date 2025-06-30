using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBasePossessable : BasePossessable
{
    [SerializeField] private float moveSpeed = 3f;

    protected override void Update()
    {
        base.Update();

        if (!isPossessed || !PossessionSystem.Instance.canMove)
            return;

        Move();
    }

    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, v, 0);
        transform.position += move * moveSpeed * Time.deltaTime;
    }
    
}
