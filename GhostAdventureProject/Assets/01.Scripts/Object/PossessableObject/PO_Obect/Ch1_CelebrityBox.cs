using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_CelebrityBox : BasePossessable
{
    // [SerializeField] private GameObject effect;
    [SerializeField] private GameObject noteObject;
    [SerializeField] private Animator animator;
    

    protected override void Update()
    {
        base.Update();
        
        if (!isPossessed || !hasActivated)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TriggerBoxEvent();
        }
    }

    private void TriggerBoxEvent()
    {
        hasActivated = true;
        
        // 박스 애니메이션 트리거
        if(animator != null)
            animator.SetTrigger("Explode");
        
        // 폭발 이펙트 ( 넣는다면 )
        // if(effect != null)
        //     Instantiate(effect, transform.position, Quaternion.identity);
        
        // note 활성화
        if(noteObject != null)
            noteObject.SetActive(true);
        
        hasActivated = false;
        
        // Unpossess();
    }
}
