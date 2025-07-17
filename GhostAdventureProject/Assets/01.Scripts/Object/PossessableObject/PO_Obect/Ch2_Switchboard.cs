using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_Switchboard : BasePossessable
{
    [SerializeField] private Ch2_CCTV[] cams;
    [SerializeField] private CinemachineVirtualCamera zoomCamera;

    private Ch2_SwitchboardPuzzleManager puzzleManager;

    private bool isControlMode = false;
    private Coroutine solvedPuzzleCoroutine;

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
            isControlMode = false;
            zoomCamera.Priority = 5;
            Unpossess();
        }
    }

    public override void OnPossessionEnterComplete()
    {
        isControlMode = true;
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
        isControlMode = false;
        zoomCamera.Priority = 5;
        Unpossess();

        foreach (var cctv in cams)
        {
            cctv.ActivateCCTV();
        }
    }
}
