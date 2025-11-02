using System;
using _01.Scripts.Extensions;
using _01.Scripts.Object.BaseClasses.Interfaces;
using _01.Scripts.Player;
using UnityEngine;

/// <summary>
/// 상호작용키 팝업 기능 구현하는 클래스
/// 어떤 오브젝트와 상호작용할지는 PlayerInteractSystem.cs 에서 관리
/// </summary>
public class BaseInteractable : MonoBehaviour, IInteractable
{
    [Header("Base Interactable References")]
    [SerializeField] protected GameObject highlightObj;
    [SerializeField] protected bool isScannable = true;
    
    protected Player player;
    
    // Properties
    public GameObject Highlight => highlightObj;

    /// <summary>
    /// Get Highlight Object Component(The class which inherits BaseInteractable)
    /// If overriding is needed, please always use base.Awake();
    /// </summary>
    protected virtual void Awake() {
        Transform component = 
            gameObject.GetComponentInChildren_SearchByName<Transform>("Highlight", true);
        highlightObj = component != null ? component.gameObject : null;
    }
    
    /// <summary>
    /// Turn off the highlight Object at first
    /// </summary>
    protected virtual void Start() {
        player = GameManager.Instance.Player;
        
        highlightObj?.SetActive(false);
    }

    public bool IsScannable() => isScannable;
    public virtual void SetScannable(bool value) => isScannable = value;

    public void ShowHighlight(bool pop) => highlightObj?.SetActive(pop);

    public virtual void TriggerEvent() { }

    // 은신처일때만 적용 (외에는 각 스크립트에서 override 중)
    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            player.InteractSystem.AddInteractable(gameObject);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            ShowHighlight(false);
            player.InteractSystem.RemoveInteractable(gameObject);
        }
    }
}