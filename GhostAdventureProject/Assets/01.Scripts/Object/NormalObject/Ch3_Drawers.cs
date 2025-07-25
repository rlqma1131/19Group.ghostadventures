using Cinemachine;
using UnityEngine;

public class Ch3_Drawers : BasePossessable
{
    [Header("줌 카메라")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera;

    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;
        zoomCamera.Priority = 5;
    }

    protected override void Update()
    {
        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            zoomCamera.Priority = 5;

            UIManager.Instance.PlayModeUI_OpenAll();

            Unpossess();
        }
    }

    public override void OnPossessionEnterComplete()
    {
        zoomCamera.Priority = 20;

        UIManager.Instance.PlayModeUI_CloseAll();
    }
}
