using System.Collections;
using UnityEngine;

public class Ch2_LazerTriggerManager : MonoBehaviour
{
    [Header("레이저 컨트롤러")]
    [SerializeField] private Ch2_LazerController[] lazerControllers;

    private bool isTriggered = false; // 처음 한번만 이벤트 발생

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;

            // 레이저컨트롤러 활성화
            foreach(Ch2_LazerController controller in lazerControllers)
            {
                controller.ActivateController();
            }

            StartCoroutine(LazerEvent());
        }
    }

    private IEnumerator LazerEvent()
    {
        // 플레이어 조작 불가
        PossessionSystem.Instance.CanMove = false;
        GameManager.Instance.PlayerController.animator.SetBool("Move", false);

        // 카메라 이동 연출 (5초)
        yield return new WaitForSeconds(2f);

        UIManager.Instance.PromptUI.ShowPrompt("어째선지 닿으면 위험할 것 같다...", 3f);
        yield return new WaitForSeconds(3f);

        // 카메라 잠깐 멈췄다가 복귀 (3초)
        yield return new WaitForSeconds(3f);

        // 플레이어 조작 가능
        PossessionSystem.Instance.CanMove = true;
    }
}
