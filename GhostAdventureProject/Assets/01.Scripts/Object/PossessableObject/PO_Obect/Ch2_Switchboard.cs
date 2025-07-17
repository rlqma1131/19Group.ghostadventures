using Cinemachine;
using System.Collections;
using UnityEngine;

public class Ch2_Switchboard : BasePossessable
{
    [Header("줌 카메라")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera;

    [Header("CCTV")]
    [SerializeField] private Ch2_CCTV[] cams;

    private Ch2_SwitchboardPuzzleManager puzzleManager;

    protected override void Start()
    {
        base.Start();

        zoomCamera.Priority = 5;

        puzzleManager = GetComponent<Ch2_SwitchboardPuzzleManager>();
    }

    protected override void Update()
    {
        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            zoomCamera.Priority = 5;
            Unpossess();
        }
    }

    public override void OnPossessionEnterComplete()
    {
        zoomCamera.Priority = 20;

        puzzleManager.EnablePuzzleControl();
    }

    public void SolvedPuzzle()
    {
        StartCoroutine(SolvedPuzzleRoutine());
    }

    private IEnumerator SolvedPuzzleRoutine()
    {
        yield return new WaitForSeconds(2f);
        zoomCamera.Priority = 5;
        Unpossess();

        // CCTV 빙의 가능
        foreach (var cctv in cams)
        {
            cctv.ActivateCCTV();
        }
    }
}
