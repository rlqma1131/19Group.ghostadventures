using System.Collections;
using UnityEngine;

public class HideArea : BasePossessable
{
    [SerializeField] AudioClip hideAreaEnterSFX;
    [SerializeField] float energyConsumeCycle = 2f;
    [SerializeField] int energyCost = 1;

    Coroutine consumeCoroutine;
    protected bool isHiding;

    override protected void Update() {
        if (!isPossessed) return;

        if (Input.GetKeyDown(KeyCode.E)) {
            isHiding = false;
            Unpossess();
        }

        TriggerEvent();
    }

    public override void OnQTESuccess() {
        Debug.Log("QTE 성공 - 빙의 완료");

        // 은신 효과음 (바스락)
        //SoundManager.Instance.PlaySFX(hideAreaEnterSFX);

        isHiding = true;

        PossessionStateManager.Instance.StartPossessionTransition();
        consumeCoroutine = StartCoroutine(ConsumeEnergyRoutine());
    }

    IEnumerator ConsumeEnergyRoutine() {
        while (isHiding) {
            player.SoulEnergy.Consume(energyCost);
            yield return new WaitForSeconds(energyConsumeCycle);
        }
    }

    public override void Unpossess() {
        if (consumeCoroutine != null) {
            StopCoroutine(consumeCoroutine);
            consumeCoroutine = null;
        }

        isHiding = false;
        base.Unpossess();
    }

    public void OnMouseEnter() {
        UIManager.Instance.SetCursor(UIManager.CursorType.HideArea);
    }

    public void OnMouseExit() {
        UIManager.Instance.SetCursor(UIManager.CursorType.Default);
    }
}