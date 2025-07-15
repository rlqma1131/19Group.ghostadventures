using UnityEngine;

public class OpenDoor : BaseDoor
{
    protected override void Start()
    {
        isLocked = false; // 기본적으로 열림
        base.Start(); // 부모 클래스의 Start 호출
    }

    protected override void TryInteract()
    {
        TeleportPlayer();

    }
}