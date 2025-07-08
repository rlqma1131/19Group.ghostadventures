using UnityEngine;
using Cinemachine;

public class AutoTeleport : MonoBehaviour
{
    [Header("Auto Teleport Settings")]
    [SerializeField] private Transform targetTransform; // 목표 Transform (드래그 앤 드롭)
    [SerializeField] private Vector2 targetPos; // 목표 좌표 (백업용)

    //[Header("Camera Settings")]
    //[SerializeField] private CinemachineVirtualCamera[] virtualCameras; // 모든 가상 카메라 배열
    //[SerializeField] private int targetCameraIndex = 0; // 이동할 맵에 맞는 카메라 인덱스

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            if (other.gameObject == GameManager.Instance.Player)
            {
                TeleportPlayer(other.gameObject);
            }
        }
    }

    private void TeleportPlayer(GameObject player)
    {
        if (player == null) return;

        Vector3 oldPosition = player.transform.position;
        Vector3 teleportPosition;

        if (targetTransform != null)
        {
            teleportPosition = targetTransform.position;
        }
        else
        {
            teleportPosition = new Vector3(targetPos.x, targetPos.y, player.transform.position.z);
        }

        // 플레이어 위치 이동
        player.transform.position = teleportPosition;

        // 모든 가상 카메라 비활성화
        //foreach (var cam in virtualCameras)
        //{
        //    cam.gameObject.SetActive(false);
        //}

        //// 타겟 카메라 활성화 및 Follow 설정

        //if (targetCameraIndex >= 0 && targetCameraIndex < virtualCameras.Length)
        //{
        //    var targetCam = virtualCameras[targetCameraIndex];



        //    targetCam.gameObject.SetActive(true);
        //    targetCam.OnTargetObjectWarped(player.transform, teleportPosition - oldPosition); // 플레이어가 순간이동했을 때 카메라 위치 업데이트
        //}

        //else
        //{
        //    Debug.LogWarning("Target camera index is out of range!");
        //}
    }
}
