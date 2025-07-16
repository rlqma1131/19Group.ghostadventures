using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_RightSwing : BasePossessable
{
    [SerializeField] private GameObject q_Key;
    
    protected override void Update()
    {
        base.Update();

        if (!isPossessed)
        {
            q_Key.SetActive(false);
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            q_Key.SetActive(false);
            Debug.Log("그네 애니메이션 실행");
            // anim.SetTrigger("RightSwing");
        }
        q_Key.SetActive(true);
    }
}
