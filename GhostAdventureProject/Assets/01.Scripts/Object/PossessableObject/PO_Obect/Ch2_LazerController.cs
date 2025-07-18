using UnityEngine;

public class Ch2_LazerController : BasePossessable
{
    [Header("레이저")]
    [SerializeField] private GameObject lazer;

    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;
    }

    protected override void Update()
    {
        base.Update();

        if (!isPossessed)
            return;

        // 빙의 상태에서 레이저 On/Off
        if (Input.GetKeyDown(KeyCode.Q))
        {
            lazer.SetActive(!lazer.activeSelf);
        }
    }

    public void ActivateController()
    {
        hasActivated = true;
    }
}
