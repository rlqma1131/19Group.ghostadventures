using Cinemachine;
using UnityEngine;

public class Ch3_Drawers : BasePossessable
{
    [Header("줌 카메라")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera;

    [SerializeField] private AudioClip openDrawerSFX;

    protected override void Start()
    {
        isPossessed = false;
        zoomCamera.Priority = 5;
    }

    protected override void Update()
    {
        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            EnemyAI.ResumeAllEnemies();
            zoomCamera.Priority = 5;

            UIManager.Instance.PlayModeUI_OpenAll();

            Unpossess();
        }
    }

    public override void OnPossessionEnterComplete()
    {
        EnemyAI.PauseAllEnemies();
        zoomCamera.Priority = 20;
        SoundManager.Instance.PlaySFX(openDrawerSFX, 1f);
        UIManager.Instance.PlayModeUI_CloseAll();
    }
}
