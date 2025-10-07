using System;
using System.Collections.Generic;
using _01.Scripts.Extensions;
using UnityEngine;

namespace _01.Scripts.Object.MemoryObject
{
    public class Ch4_Picture : MemoryFragment
    {
        [Header("Pictures")] 
        [SerializeField] GameObject emptyPicture;
        [SerializeField] GameObject filledPicture;
        
        [Header("Lines to play in each circumstance")]
        [SerializeField] List<string> linesWhenPlayerEntered = new() { "이 그림은 도대체 뭐지..?" };
        [SerializeField] List<string> linesWhenPlayerEnteredActive = new() { "그림 속 이미지가 변했어..." };

        override protected void Awake() {
            base.Awake();

            if (!emptyPicture)
                emptyPicture = gameObject.GetComponentInChildren_SearchByName<SpriteRenderer>("Empty").gameObject;
            if (!filledPicture)
                filledPicture = gameObject.GetComponentInChildren_SearchByName<SpriteRenderer>("Filled").gameObject;
        }

        override protected void Start() {
            base.Start();

            isScannable = false;
            alreadyScanned = false;
        }

        public void SetPictureState(bool scannable, bool? already = null) {
            isScannable = scannable;
            if (already != null) alreadyScanned = already.Value;
            
            if (isScannable || !isScannable && alreadyScanned) {
                emptyPicture.SetActive(false); filledPicture.SetActive(true);
            }
            else {
                filledPicture.SetActive(false); emptyPicture.SetActive(true);
            }
        }

        override protected void OnTriggerEnter2D(Collider2D other) {
            base.OnTriggerEnter2D(other);
            if (!other.CompareTag("Player")) return;
            
            switch (isScannable) {
                case false when !IsAlreadyScanned(): UIManager.Instance.PromptUI.ShowPrompt_2(linesWhenPlayerEntered.ToArray()); break;
                case true when !IsAlreadyScanned(): UIManager.Instance.PromptUI.ShowPrompt_2(linesWhenPlayerEnteredActive.ToArray()); break;
            }
        }
    }
}