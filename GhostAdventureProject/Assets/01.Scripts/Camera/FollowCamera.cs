using UnityEngine;
using Cinemachine;
using Unity.VisualScripting;
using TMPro;

public class FollowCamera : MonoBehaviour
{

    public CinemachineVirtualCamera Camera; // Follow할 대상 
    public GameObject Target; // Follow할 대상 오브젝트
   // public CinemachineVirtualCamera  vcam;
    void Start()
    {
        Target = GameObject.Find("ZoomCamera"); // Follow할 대상 오브젝트를 찾음

        

        Camera = Target.GetComponent<CinemachineVirtualCamera>(); // 현재 오브젝트의 Follow를 타겟으로 설정

        Camera.Follow = this.transform; // 현재 오브젝트를 Follow 대상으로 설정


    }


    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("CameraColider"))
    //    { 
    //        if (Camera != null)
    //        {
    //            Camera.OnTargetObjectWarped(this.transform, this.transform.position - Camera.transform.position);
    //        }
    //    }
    //}
}