using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ch2_DollPicture_correct, Ch2_DollPicture_fake를
// Ch2_DollPicture 스크립트 하나로 통일

// public class Ch2_DollPicture_correct : BaseInteractable
// {
//     [SerializeField] Ch2_Doll doll_correct;
//     private CluePickup clue;
//     private bool isPickupAble; // 단서를 획득할 수 있는 영역에 있는지 확인
//     private bool isPickupClue; // 단서를 획득했는지 확인

//     protected override void Start() {
//         base.Start();
//         clue =  GetComponent<CluePickup>();
//     }

//     void Update()
//     {
//         if(!isPickupClue) {
//             if(Input.GetKeyDown(KeyCode.E) && isPickupAble) {
//                 clue.PickupClue();
//                 isPickupClue = true;
//             }
//             // 단서 중복 획득 방지
//             if(doll_correct.isOpen_UnderGroundDoor) {
//                 isPickupClue = true;
//             }
//         }
//     }
//     protected override void OnTriggerEnter2D(Collider2D collision) {
//         if (isPickupClue) return;
//         if (collision.CompareTag("Player")) {
//             isPickupAble = true;
//             player.InteractSystem.AddInteractable(gameObject);
//         }
//     }
//     protected override void OnTriggerExit2D(Collider2D other) {
//         if (other.CompareTag("Player")) {
//             ShowHighlight(false);
//             isPickupAble = false;
//             player.InteractSystem.RemoveInteractable(gameObject);
//         }
//     }
// }
