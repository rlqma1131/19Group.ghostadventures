using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using System;

public class MemoryScrollContext : MonoBehaviour
{
    public int SelectedIndex;
    public Action<int> OnMemoryClicked;
}
