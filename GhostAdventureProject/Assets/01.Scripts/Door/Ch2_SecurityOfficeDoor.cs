using TMPro;
using UnityEngine;

// 경비실 문에 붙이는 스크립트입니다.
// 빙의 상태에서 문에 닿았을 때 E키를 누르면 빙의해제 되지 않고 다른문으로 이동할 수 있게 됩니다.

public class Ch2_SecurityOfficeDoor : LockedDoor
{
    [SerializeField] private CH2_SecurityGuard guard;
    [SerializeField] private GameObject e_key;
    public bool isDoorPass; // 빙의상태에서 다른 문으로 이동할 수 있는지


    protected override void Update() {
        if (player.InteractSystem.CurrentClosest != gameObject) return;
        if (playerNearby && !guard.IsPossessed && Input.GetKeyDown(KeyCode.E)) {
            TryInteract();
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other) {
        base.OnTriggerEnter2D(other);
        if(other.CompareTag("Person") && guard.IsPossessed)
        {
            isDoorPass = true;
        }
    }

    protected override void OnTriggerExit2D(Collider2D other) {
        base.OnTriggerExit2D(other);
            if (other.CompareTag("Player") || other.CompareTag("Person")) {
                playerNearby = false;
                isDoorPass = false;
                e_key.SetActive(false);
            }
        }
    
}