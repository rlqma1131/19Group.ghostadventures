using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Ch1_CelebrityBox : BasePossessable
{
    // [SerializeField] private GameObject effect;
    [SerializeField] private GameObject birthdayLetter;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject q_Key;
    

    protected override void Update()
    {
        base.Update();
        
        if (!isPossessed || !hasActivated)
        {
            q_Key.SetActive(false);
            return;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            q_Key.SetActive(false);
            TriggerBoxEvent();
        }
        q_Key.SetActive(true);
    }

    private void TriggerBoxEvent()
    {
        hasActivated = true;
        UIManager.Instance.Hide_Q_Key();
        
        // 박스 애니메이션 트리거
        if(animator != null)
            animator.SetTrigger("Explode");

        // 폭발 이펙트 ( 넣는다면 )
        // if(effect != null)
        //     Instantiate(effect, transform.position, Quaternion.identity);

        // Letter 활성화
        StartCoroutine(ShowLetterWithDelay());

        hasActivated = false;
    }
    private IEnumerator ShowLetterWithDelay()
    {
        birthdayLetter.SetActive(true);
        yield return null; // 한 프레임 기다림
        Animator anim = birthdayLetter.GetComponent<Animator>();
        anim?.SetTrigger("Pop");

        yield return null;
        Unpossess(); // 빙의 해제
    }
}
