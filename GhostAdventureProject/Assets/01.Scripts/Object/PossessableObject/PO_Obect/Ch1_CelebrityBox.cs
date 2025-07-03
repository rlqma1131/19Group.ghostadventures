using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_CelebrityBox : BasePossessable
{
    // [SerializeField] private GameObject effect;
    [SerializeField] private GameObject birthdayLetter;
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

        // Letter 활성화
        StartCoroutine(ShowLetterWithDelay());

        hasActivated = false;
        
         Unpossess();
    }
    private IEnumerator ShowLetterWithDelay()
    {
        birthdayLetter.SetActive(true);
        yield return null; // 한 프레임 기다림
        Animator anim = birthdayLetter.GetComponent<Animator>();
        anim?.SetTrigger("Pop");
    }
}
