using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideArea : BasePossessable
{
    [SerializeField] private AudioClip hideAreaEnterSFX;
    [SerializeField] private float energyConsumeCycle = 2f;
    [SerializeField] private int energyCost = 1;

    private Coroutine consumeCoroutine;
    protected bool isHiding = false;
    private bool firstHiding = false;

    protected override void Update()
    {
        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if(firstHiding == false)
            {
                UIManager.Instance.NoticePopupUI.FadeInAndOut("※특정 오브젝트 빙의는 쉽지않을 수 있습니다.");
            }
            isHiding = false;
            Unpossess();
        }

    }

    public override void OnQTESuccess()
    {
        Debug.Log("QTE 성공 - 빙의 완료");

        // 은신 효과음 (바스락)
        //SoundManager.Instance.PlaySFX(hideAreaEnterSFX);
        if(firstHiding == false)
        {
            UIManager.Instance.PromptUI.ShowPrompt("숨을 수 있어");
            firstHiding = true;
        }

        isHiding = true;

        PossessionStateManager.Instance.StartPossessionTransition();
        consumeCoroutine = StartCoroutine(ConsumeEnergyRoutine());
    }
    private IEnumerator ConsumeEnergyRoutine()
    {
        while (isHiding)
        {
            SoulEnergySystem.Instance.Consume(energyCost);
            yield return new WaitForSeconds(energyConsumeCycle);
        }
    }

    public override void Unpossess()
    {
        if (consumeCoroutine != null)
        {
            StopCoroutine(consumeCoroutine);
            consumeCoroutine = null;
        }

        isHiding = false;
        base.Unpossess();
    }

    public void OnMouseEnter()
    {
        UIManager.Instance.HideAreaCursor();
    }
    public void OnMouseExit()
    {
        UIManager.Instance.SetDefaultCursor();
    }

}
