using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_MosPerson : BasePossessable
{
    [SerializeField] GameObject q_key;
    private PersonConditionUI targetPerson;
    private HaveItem haveitem;

    protected override void Start()
    {
        base.Start();
        haveitem = GetComponent<HaveItem>();
        targetPerson = GetComponent<PersonConditionUI>();
        targetPerson.currentCondition = PersonCondition.Tired;
    }

    protected override void Update()
    {
        targetPerson.currentCondition = PersonCondition.Tired;
        targetPerson.SetCondition(targetPerson.currentCondition);
        base.Update();
        if(haveitem.inventorySlots == null)
        {
            hasActivated = false;
        }
        
        

    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if(collision.CompareTag("Player"))
        {
            UIManager.Instance.PromptUI.ShowPrompt_2("저 사람, 메모를 들고 있어.", "빙의해볼까?");
        } 
    }

    public override void Unpossess()
    {
        base.Unpossess();
        targetPerson.currentCondition = PersonCondition.Normal;
    }
    
}
