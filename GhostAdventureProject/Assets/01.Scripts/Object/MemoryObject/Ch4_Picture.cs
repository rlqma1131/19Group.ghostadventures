using System.Collections.Generic;
using _01.Scripts.Extensions;
using _01.Scripts.Managers.Puzzle;
using _01.Scripts.Object.NormalObject;
using UnityEngine;

namespace _01.Scripts.Object.MemoryObject
{
    public class Ch4_Picture : BaseInteractable
    {
        [Header("References")]
        [SerializeField] Ch4_ScanToTeleport scanToTeleport;
        
        [Header("Pictures")] 
        [SerializeField] GameObject emptyPicture;
        [SerializeField] GameObject filledPicture;
        
        [Header("Lines to play in each circumstance")]
        [SerializeField] List<string> linesWhenPlayerEntered = new() { "이 그림은 도대체 뭐지..?" };
        [SerializeField] List<string> linesWhenPlayerEnteredActive = new() { "그림 속 이미지가 변했어..." };

        Ch4_FurnacePuzzleManager manager;
        bool alreadyScanned;
        
        override protected void Awake() {
            base.Awake();

            if (!emptyPicture)
                emptyPicture = gameObject.GetComponentInChildren_SearchByName<SpriteRenderer>("Empty").gameObject;
            if (!filledPicture)
                filledPicture = gameObject.GetComponentInChildren_SearchByName<SpriteRenderer>("Filled").gameObject;
            if (!scanToTeleport)
                scanToTeleport = GetComponent<Ch4_ScanToTeleport>();
        }

        override protected void Start() {
            base.Start();
            
            // Field Init.
            isScannable = false;
            alreadyScanned = false;
            
            // Refs. Init.
            manager = Ch4_FurnacePuzzleManager.TryGetInstance();
        }

        public void SetPictureState(bool scannable, bool? already = null) {
            if (already != null) {
                alreadyScanned = already.Value;
                if (already.Value) manager.UpdateProgress();
                else isScannable = false;
            }
            isScannable = scannable;
            
            if (isScannable || !isScannable && alreadyScanned) {
                emptyPicture.SetActive(false); filledPicture.SetActive(true);
                scanToTeleport.SetTeleportable(!alreadyScanned);
            }
            else {
                filledPicture.SetActive(false); emptyPicture.SetActive(true);
                scanToTeleport.SetTeleportable(false);
            }
        }
        
        public bool IsAlreadyScanned() => alreadyScanned;

        override protected void OnTriggerEnter2D(Collider2D other) {
            base.OnTriggerEnter2D(other);
            if (!other.CompareTag("Player")) return;

            switch (isScannable) {
                case false when !alreadyScanned: UIManager.Instance.PromptUI.ShowPrompt_2(linesWhenPlayerEntered.ToArray()); break;
                case true when !alreadyScanned: UIManager.Instance.PromptUI.ShowPrompt_2(linesWhenPlayerEnteredActive.ToArray()); break;
            }
        }
    }
}