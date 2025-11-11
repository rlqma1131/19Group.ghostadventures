using System.Collections;
using Cinemachine;
using UnityEngine;

public class CameraTargetSetter : MonoBehaviour
{
    CinemachineVirtualCamera virtualCamera;

    void Awake() => virtualCamera = GetComponent<CinemachineVirtualCamera>();

    void Start() => StartCoroutine(AssignPlayerWhenReady());

    IEnumerator AssignPlayerWhenReady() {
        yield return new WaitUntil(() => GameManager.Instance != null && GameManager.Instance.PlayerObj != null);
        
        Transform playerTransform = GameManager.Instance.PlayerObj.transform;
        virtualCamera.Follow = playerTransform;

        Debug.Log("카메라 타겟이 플레이어로 설정되었습니다!");
    }
}