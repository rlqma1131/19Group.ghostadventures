using Cinemachine;
using System.Collections;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class Ch1_TV : BasePossessable
{
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private Animator zoomAnim;
    [SerializeField] private GameObject memoryObject;
    [SerializeField] private GameObject show;
    [SerializeField] private TextMeshProUGUI channelTxt;
    [SerializeField] private LockedDoor Door;

    private Ch1_BirthdayHat_MemoryFake_01 birthdayHat;

    private bool isControlMode = false;
    private int channel = 1;

    protected override void Start()
    {
        show.SetActive(false);
        birthdayHat = memoryObject.GetComponent<Ch1_BirthdayHat_MemoryFake_01>();
        memoryObject.SetActive(false);
        hasActivated = false;
    }

    public void ActivateTV()
    {
        if (hasActivated) return;

        hasActivated = true;
        if (anim != null)
            anim.SetTrigger("On");
    }

    protected override void Update()
    {
        if (!isPossessed || !hasActivated)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isControlMode)
            {
                // 조작 종료
                isControlMode = false;
                isPossessed = false;
                zoomCamera.Priority = 5;
                Unpossess();
            }
        }
        
        // 채널 변경
        if (Input.GetKeyDown(KeyCode.W)) 
        { 
            channel = Mathf.Clamp(channel + 1, 1, 9); 
            UpdateChannelDisplay();
            // 채널 변경 효과음 추가
            zoomAnim.SetTrigger("Change");
        }
        if (Input.GetKeyDown(KeyCode.S)) 
        { 
            channel = Mathf.Clamp(channel - 1, 1, 9); 
            UpdateChannelDisplay();
            // 채널 변경 효과음 추가
            zoomAnim.SetTrigger("Change");
        }

        // 확정은 Space
        if (channel == 9 && Input.GetKeyDown(KeyCode.Space))
        {
            // 정답 효과음 추가
            ShowMemoryandDoorOpen();
        }
    }

    private void UpdateChannelDisplay()
    {
        if(channelTxt != null)
            channelTxt.text = ($"0{channel}");
    }

    private void ShowMemoryandDoorOpen()
    {
        // 1. TV 줌 애니메이션 재생
        show.SetActive(true);
        anim.SetTrigger("Solved");
        zoomAnim.SetTrigger("Show");
        StartCoroutine(WaitZoomEnding(3f));
    }

    private IEnumerator WaitZoomEnding(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 2. 기억조각 보이기
        if (memoryObject != null)
        {
            memoryObject.SetActive(true);
            birthdayHat.ActivateHat();
            anim.SetTrigger("Solved");
        }

        // 3. 문 열기
        Door.SolvePuzzle();

        // 4. 빙의 해제
        hasActivated = false;
        isPossessed = false;
        isControlMode = false;
        zoomCamera.Priority = 5;
        Unpossess();
    }

    public override void OnPossessionEnterComplete() 
    {
        zoomCamera.Priority = 20; // 빙의 시 카메라 우선순위 높이기
        isControlMode = true;
        isPossessed = true;
        channelTxt.text = "01"; // 초기 채널 표시
        UpdateChannelDisplay();
    }

}
