using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class Think : MonoBehaviour
{
    [SerializeField] private Transform targetWorldObject; // 말풍선 띄울 대상
    [SerializeField] private Camera worldCamera;
    private RectTransform uiElement;     // 말풍선 UI 오브젝트
    public Image itemIcon;

    void Start()
    {
        uiElement = GetComponent<RectTransform>();

    }

    void Update()
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(worldCamera, targetWorldObject.position);
        uiElement.position = screenPos;
        Vector2 offset = new Vector2(0, 2); // 위로 2px만큼
    }
}
