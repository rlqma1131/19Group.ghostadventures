using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ch2_MorsePerson : BasePossessable
{
    private PersonConditionUI conditionUI;      // 컨디션 UI
    private HaveItem haveitem;                  // 가지고있는 아이템
    private bool UseAllItem = false;            // 모든 아이템 사용했는지 확인

    protected override void Start()
    {
        base.Start();
        haveitem = GetComponent<HaveItem>();
        conditionUI = GetComponent<PersonConditionUI>();
        conditionUI.currentCondition = PersonCondition.Tired;
    }

    protected override void Update()
    {
        if (!isPossessed)
            return;

        UIManager.Instance.tabkeyUI.SetActive(true);
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (haveitem.IsInventoryEmpty())
            {
                Unpossess();
                hasActivated = false;
                MarkActivatedChanged();

                UseAllItem = true;
            }
            else
            {
                UIManager.Instance.PromptUI.ShowPrompt("뭔가 더 얻을 수 있는게 있을것 같아");
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if(collision.CompareTag("Player") && !UseAllItem)
        {
            UIManager.Instance.PromptUI.ShowPrompt("저 사람, 메모를 들고 있어. 빙의해볼까?");
        } 
    }

    public override void Unpossess()
    {
        base.Unpossess();
        UIManager.Instance.tabkeyUI.SetActive(false);
    }
    
}
