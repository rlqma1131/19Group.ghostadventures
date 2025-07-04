using UnityEngine;

public class OpenDoor : BaseDoor
{
    protected override void Start()
    {
        isLocked = false; // 기본적으로 열림
    }

    protected override void TryInteract()
    {
        TeleportPlayer();
    }
}