using System.Collections;
using UnityEngine;

public class Ch2_LaserTriggerManager : MonoBehaviour
{
    [Header("레이저 컨트롤러")]
    [SerializeField] private Ch2_LaserController[] laserControllers;

    private bool isTriggered = false; // 처음 한번만 이벤트 발생

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;

            // 레이저컨트롤러 활성화
            foreach(Ch2_LaserController controller in laserControllers)
            {
                controller.ActivateController();
            }

            StartCoroutine(LaserEvent());
        }
    }

    private IEnumerator LaserEvent()
    {
        // 플레이어 조작 불가
        PossessionSystem.Instance.CanMove = false;
        GameManager.Instance.PlayerController.animator.SetBool("Move", false);

        // 카메라 이동 연출 (6초)
        yield return new WaitForSeconds(6f);

        UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode("어째선지 닿으면 위험할 것 같다...", 3f);
        yield return new WaitForSeconds(3f);

        // 카메라 잠깐 멈췄다가 복귀 (3초)
        yield return new WaitForSeconds(3f);

        // 플레이어 조작 가능
        PossessionSystem.Instance.CanMove = true;
    }
}
