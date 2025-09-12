using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    public CinemachineVirtualCamera currentCam; // 현재 활성화된 카메라

    void OnTriggerEnter2D(Collider2D collision) {
        if (!collision.CompareTag("Room") || 
            !collision.TryGetComponent(out CameraChange changer)) return;
        
        currentCam = changer.Vcam;
    }
}
