using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_MosPerson : BasePossessable
{
    [SerializeField] GameObject q_key;

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if(collision.CompareTag("Player"))
        {
            UIManager.Instance.PromptUI.ShowPrompt_2("저 사람, 메모를 들고 있어.", "빙의해볼까?");
        } 
    }
}
