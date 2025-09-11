using System.Collections;
using UnityEngine;

public class Ch2_LaserTriggerManager : MonoBehaviour
{
    [Header("레이저 컨트롤러")]
    [SerializeField] private Ch2_LaserController[] laserControllers;

    //private bool isTriggered = false; // 처음 한번만 이벤트 발생

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") 
            //&& !isTriggered
            )
        {
            //isTriggered = true;

            // 레이저컨트롤러 활성화
            foreach(Ch2_LaserController controller in laserControllers)
            {
                controller.ActivateController();
            }

            if(!EventManager.Instance.IsEventCompleted(GetComponent<UniqueId>().Id))
            {
                EventManager.Instance.MarkEventCompleted(GetComponent<UniqueId>().Id);

                StartCoroutine(LaserEvent());
            }
        }
    }

    private IEnumerator LaserEvent()
    {
        // 플레이어 조작 불가
        PossessionSystem.Instance.CanMove = false;
        GameManager.Instance.PlayerController.animator.SetBool("Move", false);

        // 카메라 이동 연출 (6초)
        yield return new WaitForSeconds(6f);

        UIManager.Instance.PromptUI.ShowPrompt("가까이 가면 뜨거워…", 3f);

        yield return new WaitForSeconds(3f);

        UIManager.Instance.PromptUI.ShowPrompt("이 불빛… 닿으면 위험할 것 같아.", 3f);

        // 카메라 잠깐 멈췄다가 복귀 (3초)
        yield return new WaitForSeconds(3f);

        UIManager.Instance.PromptUI.ShowPrompt("반대편에 출구가 있는 것 같네", 2f);
        UIManager.Instance.PromptUI.ShowPrompt("위에 있는 저 장치를 작동해보자", 2f);
        
        // 플레이어 조작 가능
        UIManager.Instance.PlayModeUI_OpenAll();
        PossessionSystem.Instance.CanMove = true;

        gameObject.SetActive(false);
    }
}
