using System.Collections.Generic;
using _01.Scripts.CustomPropertyAttribute;
using _01.Scripts.Extensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace _01.Scripts.Object.PossessableObject.PO_Object
{
    public class Ch4_Candle : BasePossessable
    {
        [Header("Candle Transform")] 
        [SerializeField] Transform candleBody;
        [SerializeField] List<Vector3> path = new();
        [SerializeField] Quaternion endRotation;
        [SerializeField, ReadOnly] Quaternion startRotation;

        [Header("Light Settings")] 
        [SerializeField] Light2D fire;
        [SerializeField] UnityEvent OnLit;
        
        [Header("Drop Animation Settings")]
        [SerializeField] AudioClip dropSound;
        [SerializeField] Ease easeMode = Ease.InQuad;
        [SerializeField] float dropDuration = 1.5f;
        [SerializeField] UnityEvent OnDropped;
        
        [Header("UI References")]
        [SerializeField] GameObject q_Key;

        override protected void Awake() {
            base.Awake();
            if (!fire) fire = GetComponentInChildren<Light2D>(true);
            if (!candleBody) candleBody = gameObject.GetComponentInChildren_SearchByName<Transform>("Body", true);
            if (!q_Key) q_Key = gameObject.GetComponentInChildren_SearchByName<Transform>("Q_Key", true).gameObject;
        }

        override protected void Start() {
            base.Start();
            startRotation = candleBody.rotation;
            q_Key.SetActive(false);
        }
        
        public override void TriggerEvent() {
            if (!isPossessed) { q_Key.SetActive(false); return; }
        
            q_Key.SetActive(true);
            if (Input.GetKeyDown(KeyCode.Q)) OnCandleLit();
        }

        public void TriggerDropEvent() {
            // If fire did not lit, do not drop.
            if (!fire.gameObject.activeInHierarchy) return;
            
            Sequence dropSequence = DOTween.Sequence();
            dropSequence.Append(transform.DOLocalPath(path.ToArray(), dropDuration, PathType.CatmullRom, PathMode.TopDown2D).SetEase(easeMode));
            dropSequence.Join(candleBody.DOLocalRotateQuaternion(endRotation, dropDuration)).SetEase(Ease.Linear);
            dropSequence.JoinCallback(() => {
                hasActivated = false;
                isScannable = false;
            });
            dropSequence.AppendCallback(() => {
                // TODO: Play Animation when dropped
                SoundManager.Instance.PlaySFX(dropSound);
                fire.gameObject.SetActive(false);
                
                // Add Event when brick dropped
                OnDropped?.Invoke();
                MarkActivatedChanged();
            });
        }

        void OnCandleLit() {
            fire.gameObject.SetActive(!fire.gameObject.activeInHierarchy);
            OnLit?.Invoke();
        }
    }
}