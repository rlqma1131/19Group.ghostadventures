using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnMouse : MonoBehaviour
{
    void OnMouseEnter()
    {
        UIManager.Instance.SetCursor(UIManager.CursorType.Default);
    }
}
